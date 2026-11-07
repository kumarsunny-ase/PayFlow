using PayFlow.Modules.Payments.Application.Abstractions;
using PayFlow.Modules.Payments.Domain.Payments;
using PayFlow.Modules.Payments.Infrastructure;
using PayFlow.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PayFlow")
    ?? throw new InvalidOperationException("Connection string 'PayFlow' is missing.");

builder.Services.AddPaymentsInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // ---- TEMPORARY smoke test (remove in Step 5) ----
    app.MapPost("/dev/smoke-test", async (
        IPaymentRepository repository, IPaymentsUnitOfWork unitOfWork, CancellationToken ct) =>
    {
        var now = DateTimeOffset.UtcNow;
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(100m, "EUR").Value, "4242", now).Value;
        payment.Authorise("AUTH-123", now);
        payment.Capture(now);
        payment.IssueRefund(Money.Create(30m, "EUR").Value, now);

        repository.Add(payment);
        await unitOfWork.SaveChangesAsync(ct);

        return Results.Ok(new { Id = payment.Id.Value });
    });

    app.MapGet("/dev/payments/{id:guid}", async (Guid id, IPaymentRepository repository, CancellationToken ct) =>
    {
        var p = await repository.GetByIdAsync(new PaymentId(id), ct);
        return p is null
            ? Results.NotFound()
            : Results.Ok(new
            {
                Id = p.Id.Value,
                Status = p.Status.ToString(),
                Amount = p.Amount.ToString(),
                Refunded = p.RefundedAmount.ToString(),
                Refundable = p.RefundableAmount.ToString(),
                Refunds = p.Refunds.Select(r => r.Amount.ToString())
            });
    });
    // --------------------------------------------------
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();