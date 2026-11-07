namespace PayFlow.Modules.Payments.Domain.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
    void Add(Payment payment);
}