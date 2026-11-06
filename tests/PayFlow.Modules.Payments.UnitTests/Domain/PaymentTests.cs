using PayFlow.Modules.Payments.Domain.Payments;
using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.UnitTests.Domain;

public class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    private static Money Eur(decimal amount) => Money.Create(amount, "EUR").Value;

    private static Payment NewPayment(decimal amount = 100m) =>
        Payment.Create(Guid.NewGuid(), Eur(amount), "4242", Now).Value;

    private static Payment CapturedPayment(decimal amount = 100m)
    {
        var payment = NewPayment(amount);
        payment.Authorise("AUTH123", Now);
        payment.Capture(Now);
        payment.ClearDomainEvents();
        return payment;
    }

    // ---------- Create ----------

    [Fact]
    public void Create_ValidInput_StartsAsPending()
    {
        var payment = NewPayment();

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(Eur(100m), payment.Amount);
        Assert.Empty(payment.DomainEvents);
    }

    [Fact]
    public void Create_ZeroAmount_Fails()
    {
        var result = Payment.Create(Guid.NewGuid(), Eur(0m), "4242", Now);
        Assert.Equal(PaymentErrors.AmountMustBePositive, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12a4")]
    [InlineData("12345")]
    public void Create_InvalidCardLast4_Fails(string last4)
    {
        var result = Payment.Create(Guid.NewGuid(), Eur(10m), last4, Now);
        Assert.Equal(PaymentErrors.InvalidCardLast4, result.Error);
    }

    // ---------- Authorise / Decline ----------

    [Fact]
    public void Authorise_PendingPayment_BecomesAuthorised_AndRaisesEvent()
    {
        var payment = NewPayment();

        var result = payment.Authorise("AUTH123", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Authorised, payment.Status);
        Assert.IsType<PaymentAuthorised>(Assert.Single(payment.DomainEvents));
    }

    [Fact]
    public void Authorise_Twice_Fails()
    {
        var payment = NewPayment();
        payment.Authorise("AUTH123", Now);

        var result = payment.Authorise("AUTH456", Now);

        Assert.Equal("Payment.InvalidTransition", result.Error.Code);
    }

    [Fact]
    public void Decline_PendingPayment_BecomesDeclined()
    {
        var payment = NewPayment();

        var result = payment.Decline("Insufficient funds", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Declined, payment.Status);
    }

    // ---------- Capture / Void ----------

    [Fact]
    public void Capture_WithoutAuthorisation_Fails()
    {
        var payment = NewPayment();

        var result = payment.Capture(Now);

        Assert.Equal("Payment.InvalidTransition", result.Error.Code);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Void_AuthorisedPayment_BecomesVoided()
    {
        var payment = NewPayment();
        payment.Authorise("AUTH123", Now);

        var result = payment.Void(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Voided, payment.Status);
    }

    [Fact]
    public void Void_AfterCapture_Fails()
    {
        var payment = CapturedPayment();

        var result = payment.Void(Now);

        Assert.Equal("Payment.InvalidTransition", result.Error.Code);
    }

    // ---------- Refunds ----------

    [Fact]
    public void Refund_BeforeCapture_Fails()
    {
        var payment = NewPayment();
        payment.Authorise("AUTH123", Now);

        var result = payment.IssueRefund(Eur(10m), Now);

        Assert.Equal("Payment.InvalidTransition", result.Error.Code);
    }

    [Fact]
    public void PartialRefund_SetsPartiallyRefunded()
    {
        var payment = CapturedPayment(100m);

        var result = payment.IssueRefund(Eur(30m), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(Eur(70m), payment.RefundableAmount);
        Assert.IsType<PaymentRefunded>(Assert.Single(payment.DomainEvents));
    }

    [Fact]
    public void TwoPartialRefunds_CoveringFullAmount_SetsRefunded()
    {
        var payment = CapturedPayment(100m);

        payment.IssueRefund(Eur(40m), Now);
        payment.IssueRefund(Eur(60m), Now);

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(2, payment.Refunds.Count);
        Assert.Equal(Eur(0m), payment.RefundableAmount);
    }

    [Fact]
    public void Refund_MoreThanRefundable_Fails()
    {
        var payment = CapturedPayment(100m);
        payment.IssueRefund(Eur(80m), Now);

        var result = payment.IssueRefund(Eur(30m), Now);

        Assert.Equal("Payment.RefundExceedsRefundable", result.Error.Code);
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
    }

    [Fact]
    public void Refund_DifferentCurrency_Fails()
    {
        var payment = CapturedPayment(100m);
        var usd = Money.Create(10m, "USD").Value;

        var result = payment.IssueRefund(usd, Now);

        Assert.Equal(PaymentErrors.CurrencyMismatch, result.Error);
    }

    [Fact]
    public void Refund_FullyRefundedPayment_Fails()
    {
        var payment = CapturedPayment(50m);
        payment.IssueRefund(Eur(50m), Now);

        var result = payment.IssueRefund(Eur(1m), Now);

        Assert.Equal("Payment.InvalidTransition", result.Error.Code);
    }
}