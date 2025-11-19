namespace BidService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
