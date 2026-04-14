namespace BuildingBlocks.Domain.Events;

public interface IVersionedDomainEvent : IDomainEvent
{
    int EventVersion { get; }
}

public abstract record VersionedDomainEvent : DomainEvent, IVersionedDomainEvent
{
    public abstract int EventVersion { get; }
}
