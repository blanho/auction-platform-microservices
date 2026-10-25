using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface IPaginatedAuctionQueryService
{
    Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken);
}
