using AuctionService.Domain.Entities;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
}
