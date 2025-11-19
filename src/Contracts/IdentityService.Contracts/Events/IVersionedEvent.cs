namespace IdentityService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
