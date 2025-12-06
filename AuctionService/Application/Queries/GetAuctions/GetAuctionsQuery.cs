using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctions;

public record GetAuctionsQuery(
    string? Status = null,
    string? Seller = null,
    string? Winner = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? OrderBy = null,
    bool Descending = false
) : IQuery<PagedResult<AuctionDto>>;
