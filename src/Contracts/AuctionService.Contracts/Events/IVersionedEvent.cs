namespace AuctionService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
