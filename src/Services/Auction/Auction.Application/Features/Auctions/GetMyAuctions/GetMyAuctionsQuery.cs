using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Features.Auctions.GetMyAuctions;

public record GetMyAuctionsQuery(
    string Username,
    string? Status = null,
    string? SearchTerm = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? OrderBy = null,
    bool Descending = false) : IQuery<PaginatedResult<AuctionDto>>;

