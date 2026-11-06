using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.Domain.Payments;

public sealed class Payment : AggregateRoot<PaymentId>
{
    public readonly List<Refund> _refunds = [];
    public Guid MerchantId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? CardLast4 { get; private set; } = null!;
    public string? AuthorizationCode { get; private set; }
    public string? DeclineReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? AuthorisedAt { get; private set; }
    public DateTimeOffset? CapturedAt { get; private set; }
    public DateTimeOffset? VoidedAt { get; private set; }

    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

    public Money RefundedAmount => _refunds.Aggregate(Money.Zero(Amount.Currency), (sum, r) => sum.Add(r.Amount));

    public Money RefundableAmount => Amount.Subtract(RefundedAmount);

    private Payment() { } // EF Core

    private Payment(PaymentId id, Guid merchantId, Money amount, string cardLast4, DateTimeOffset now) : base(id)
    {
        MerchantId = merchantId;
        Amount = amount;
        CardLast4 = cardLast4;
        CreatedAt = now;
        Status = PaymentStatus.Pending;
    }

    public static Result<Payment> Create(Guid merchantId, Money amount, string cardLast4, DateTimeOffset now)
    {
        if (merchantId == Guid.Empty)
            return PaymentErrors.MerchantRequired;

        if (amount.Amount <= 0)
            return PaymentErrors.AmountMustBePositive;

        if (cardLast4 is not { Length: 4 } || !cardLast4.All(char.IsDigit))
            return PaymentErrors.InvalidCardLast4;

        return new Payment(PaymentId.New(), merchantId, amount, cardLast4, now);
    }

    public Result Authorise(string authorizationCode, DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending)
            return PaymentErrors.InvalidTransition(Status, "authorise");
        if (string.IsNullOrWhiteSpace(authorizationCode))
            return PaymentErrors.AuthorizationCodeRequired;

        Status = PaymentStatus.Authorised;
        AuthorizationCode = authorizationCode;
        AuthorisedAt = now;
        Raise(new PaymentAuthorised(Id, Amount, now));
        return Result.Success();
    }

    public Result Decline(string reason, DateTimeOffset now)
    {
        if (Status != PaymentStatus.Pending)
            return PaymentErrors.InvalidTransition(Status, "decline");
        if (string.IsNullOrWhiteSpace(reason))
            return PaymentErrors.DeclineReasonRequired;

        Status = PaymentStatus.Declined;
        DeclineReason = reason;
        Raise(new PaymentDeclined(Id, reason, now));
        return Result.Success();
    }

    public Result Capture(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Authorised)
            return PaymentErrors.InvalidTransition(Status, "capture");

        Status = PaymentStatus.Captured;
        CapturedAt = now;
        Raise(new PaymentCaptured(Id, Amount, now));
        return Result.Success();
    }

    public Result Void(DateTimeOffset now)
    {
        if (Status != PaymentStatus.Authorised)
            return PaymentErrors.InvalidTransition(Status, "void");

        Status = PaymentStatus.Voided;
        VoidedAt = now;
        Raise(new PaymentVoided(Id, now));
        return Result.Success();
    }

    public Result<Refund> IssueRefund(Money amount, DateTimeOffset now)
    {
        if (Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded))
            return PaymentErrors.InvalidTransition(Status, "refund");
        if (amount.Currency != Amount.Currency)
            return PaymentErrors.CurrencyMismatch;
        if (amount.Amount <= 0)
            return PaymentErrors.AmountMustBePositive;
        if (amount.Amount > RefundableAmount.Amount)
            return PaymentErrors.RefundExceedsRefundable(RefundableAmount);

        var refund = new Refund(RefundId.New(), Id, amount, now);
        _refunds.Add(refund);

        Status = RefundableAmount.Amount == 0
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        Raise(new PaymentRefunded(Id, refund.Id, amount, now));
        return refund;
    }

}