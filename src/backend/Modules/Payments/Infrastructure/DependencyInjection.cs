using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayFlow.Modules.Payments.Application.Abstractions;
using PayFlow.Modules.Payments.Domain.Payments;
using PayFlow.Modules.Payments.Infrastructure.Persistence;

namespace PayFlow.Modules.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", PaymentsDbContext.Schema)));

        services.AddScoped<IPaymentsUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}