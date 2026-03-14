using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IAuctionSchedulerRepository
{
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsEndingBetweenAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}
