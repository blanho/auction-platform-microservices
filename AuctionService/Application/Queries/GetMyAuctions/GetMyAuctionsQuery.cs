using Common.CQRS.Abstractions;
using AuctionService.Application.DTOs;

namespace AuctionService.Application.Queries.GetMyAuctions;

public record GetMyAuctionsQuery(
    string Username,
    string? Status = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? OrderBy = null,
    bool Descending = false) : IQuery<PagedResult<AuctionDto>>;
