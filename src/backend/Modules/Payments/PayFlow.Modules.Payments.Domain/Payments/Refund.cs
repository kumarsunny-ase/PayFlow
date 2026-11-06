using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.Domain.Payments;

public sealed class Refund : Entity<RefundId>
{
    public PaymentId PaymentId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Refund() { } // EF Core

    internal Refund(RefundId id, PaymentId paymentId, Money amount, DateTimeOffset createdAt) : base(id)
    {
        PaymentId = paymentId;
        Amount = amount;
        CreatedAt = createdAt;
    }
}