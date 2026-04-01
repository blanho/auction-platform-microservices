using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IAuctionUserRepository
{
    Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default);
}
