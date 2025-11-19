using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Queries.GetMyAuctions;

public record GetMyAuctionsQuery(
    string Username,
    string? Status = null,
    string? SearchTerm = null,
    int PageNumber = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? OrderBy = null,
    bool Descending = false) : IQuery<PaginatedResult<AuctionDto>>;

