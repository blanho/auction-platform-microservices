using AuctionService.Application.DTOs;
using Common.Core.Constants;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctions;

public record GetAuctionsQuery(
    string? Status = null,
    string? Seller = null,
    string? Winner = null,
    string? SearchTerm = null,
    string? Category = null,
    bool? IsFeatured = null,
    int PageNumber = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? OrderBy = null,
    bool Descending = false
) : IQuery<PagedResult<AuctionDto>>;
