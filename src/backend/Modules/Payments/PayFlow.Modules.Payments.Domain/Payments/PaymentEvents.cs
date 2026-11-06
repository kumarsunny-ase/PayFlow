using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.Domain.Payments;

public sealed record PaymentAuthorised(PaymentId PaymentId, Money Amount, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record PaymentDeclined(PaymentId PaymentId, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record PaymentCaptured(PaymentId PaymentId, Money Amount, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record PaymentVoided(PaymentId PaymentId, DateTimeOffset OccurredAt) : IDomainEvent;
public sealed record PaymentRefunded(PaymentId PaymentId, RefundId RefundId, Money Amount, DateTimeOffset OccurredAt) : IDomainEvent;