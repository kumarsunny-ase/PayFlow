namespace PayFlow.SharedKernel
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredAt { get; }
    }
}