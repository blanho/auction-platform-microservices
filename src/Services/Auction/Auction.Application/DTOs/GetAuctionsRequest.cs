using BuildingBlocks.Application.Constants;

namespace Auctions.Application.DTOs;

public sealed record GetAuctionsRequest(
    string? Status = null,
    string? Seller = null,
    string? Winner = null,
    string? SearchTerm = null,
    string? Category = null,
    bool? IsFeatured = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? OrderBy = null,
    bool? Descending = null);

