namespace PayFlow.SharedKernel
{
    public abstract class Entity<TId> where TId : notnull
    {
        public TId Id { get; protected set;} = default!;

        protected Entity(TId id) => Id =id;

        protected Entity() {} // required by EF Core

        public override bool Equals(object? obj) =>
            obj is Entity<TId> other && other.GetType() == GetType() && Id.Equals(other.Id);

        public override int GetHashCode() => Id.GetHashCode();
    }
}