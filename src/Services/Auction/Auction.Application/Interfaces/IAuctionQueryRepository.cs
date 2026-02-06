using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface IAuctionQueryRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(AuctionFilterDto filter, CancellationToken cancellationToken = default);
}
