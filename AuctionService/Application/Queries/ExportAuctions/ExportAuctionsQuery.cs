using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.ExportAuctions;

public record ExportAuctionsQuery(
    string? Status = null,
    string? Seller = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
) : IQuery<List<ExportAuctionDto>>;
