namespace PayFlow.Modules.Payments.Domain.Payments;

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct RefundId(Guid Value)
{
    public static RefundId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}