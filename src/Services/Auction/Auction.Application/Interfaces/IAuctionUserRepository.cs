using Auctions.Application.DTOs;
using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IAuctionUserRepository
{
    Task<List<Auction>> GetBySellerUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetWonByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default);
    Task<int> GetWatchlistCountByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<SellerStatsDto> GetSellerStatsAsync(
        string username,
        DateTimeOffset periodStart,
        DateTimeOffset? previousPeriodStart = null,
        CancellationToken cancellationToken = default);
}
