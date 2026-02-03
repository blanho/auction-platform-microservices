using Bidding.Application.DTOs;
using Bidding.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetBidsForAuction;

public record GetBidsForAuctionQuery(
    Guid AuctionId,
    BidStatus? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<BidDto>>;
