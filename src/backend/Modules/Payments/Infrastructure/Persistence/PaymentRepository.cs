using Microsoft.EntityFrameworkCore;
using PayFlow.Modules.Payments.Domain.Payments;

namespace PayFlow.Modules.Payments.Infrastructure.Persistence;

internal sealed class PaymentRepository(PaymentsDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default) =>
        db.Payments
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Payment payment) => db.Payments.Add(payment);
}