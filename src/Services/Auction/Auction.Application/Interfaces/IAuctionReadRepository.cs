using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface IAuctionReadRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(AuctionFilterDto filter, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsEndingBetweenAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
}
