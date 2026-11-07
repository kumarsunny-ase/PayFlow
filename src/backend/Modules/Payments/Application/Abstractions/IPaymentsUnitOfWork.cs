namespace PayFlow.Modules.Payments.Application.Abstractions;

public interface IPaymentsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}