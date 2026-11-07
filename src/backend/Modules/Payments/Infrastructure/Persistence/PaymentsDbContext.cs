using Microsoft.EntityFrameworkCore;
using PayFlow.Modules.Payments.Domain.Payments;
using PayFlow.Modules.Payments.Application.Abstractions;

namespace PayFlow.Modules.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options), IPaymentsUnitOfWork
{
   public const string Schema = "payments";
   public DbSet<Payment> Payments => Set<Payment>();

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.HasDefaultSchema(Schema);
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
   }
}
