using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Modules.Payments.Domain.Payments;

namespace PayFlow.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");

        builder.HasKey(r => r.Id).IsClustered(false);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new RefundId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.PaymentId)
            .HasConversion(id => id.Value, value => new PaymentId(value));

        builder.ComplexProperty(r => r.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsFixedLength();
        });
    }
}