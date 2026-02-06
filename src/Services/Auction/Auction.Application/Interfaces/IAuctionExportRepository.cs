using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Application.Interfaces;

public interface IAuctionExportRepository
{
    Task<List<Auction>> GetAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
}
