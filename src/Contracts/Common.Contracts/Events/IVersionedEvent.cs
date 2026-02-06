namespace Common.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}