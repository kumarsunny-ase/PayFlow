namespace PayFlow.Modules.Payments.Domain.Payments;

public enum PaymentStatus
{
    Pending = 0,
    Authorised = 1,
    Declined = 2,
    Captured = 3,
    PartiallyRefunded = 4,
    Refunded = 5,
    Voided = 6
}