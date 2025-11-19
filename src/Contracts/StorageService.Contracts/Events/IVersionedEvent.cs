namespace StorageService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
