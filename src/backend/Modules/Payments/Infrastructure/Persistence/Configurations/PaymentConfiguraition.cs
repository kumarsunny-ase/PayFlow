using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Modules.Payments.Domain.Payments;

namespace PayFlow.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentsConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        //Key: strongly typed ID <-> Guid column
        builder.HasKey(p=> p.Id).IsClustered(false);
        builder.Property(p=> p.Id)
        .HasConversion(p => p.Value, p => new PaymentId(p))
        .ValueGeneratedNever();

        // Money -> two columns in the same table (complex type)
        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsFixedLength();
        });

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.CardLast4).HasMaxLength(4).IsFixedLength();
        builder.Property(p => p.AuthorizationCode).HasMaxLength(64);
        builder.Property(p => p.DeclineReason).HasMaxLength(256);

        // Optimistic concurrency: SQL Server updates this value on every write
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Refunds: child entities, loaded through the private _refunds field
        builder.HasMany(p => p.Refunds)
            .WithOne()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.Refunds)
            .HasField("_refunds")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Computed values and events are not columns
        builder.Ignore(p => p.RefundedAmount);
        builder.Ignore(p => p.RefundableAmount);
        builder.Ignore(p => p.DomainEvents);

        // Indexes
        builder.HasIndex(p => p.CreatedAt).IsClustered();
        builder.HasIndex(p => new { p.MerchantId, p.CreatedAt });
        builder.HasIndex(p => p.Status);
    }

}