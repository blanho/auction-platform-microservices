using Common.Core.Constants;
using Common.CQRS.Abstractions;
using AuctionService.Application.DTOs;

namespace AuctionService.Application.Queries.GetMyAuctions;

public record GetMyAuctionsQuery(
    string Username,
    string? Status = null,
    string? SearchTerm = null,
    int PageNumber = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? OrderBy = null,
    bool Descending = false) : IQuery<PagedResult<AuctionDto>>;
