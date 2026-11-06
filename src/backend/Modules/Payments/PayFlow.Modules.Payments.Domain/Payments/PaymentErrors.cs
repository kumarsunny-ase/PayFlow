using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error MerchantRequired =
        new("Payment.MerchantRequired", "Merchant id is required.");

    public static readonly Error AmountMustBePositive =
        new("Payment.AmountMustBePositive", "Amount must be greater than zero.");

    public static readonly Error InvalidCardLast4 =
        new("Payment.InvalidCardLast4", "Card last 4 digits must be exactly 4 digits.");

    public static readonly Error AuthorizationCodeRequired =
        new("Payment.AuthorizationCodeRequired", "Authorization code is required.");

    public static readonly Error DeclineReasonRequired =
        new("Payment.DeclineReasonRequired", "Decline reason is required.");

    public static readonly Error CurrencyMismatch =
        new("Payment.CurrencyMismatch", "Refund currency must match the payment currency.");

    public static Error InvalidTransition(PaymentStatus current, string action) =>
        new("Payment.InvalidTransition", $"Cannot {action} a payment with status '{current}'.");

    public static Error RefundExceedsRefundable(Money refundable) =>
        new("Payment.RefundExceedsRefundable", $"Refund exceeds the refundable amount of {refundable}.");
}