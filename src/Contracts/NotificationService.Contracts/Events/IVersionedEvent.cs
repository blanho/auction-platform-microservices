namespace NotificationService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
