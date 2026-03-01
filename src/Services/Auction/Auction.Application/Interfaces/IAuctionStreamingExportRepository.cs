using Auctions.Domain.Entities;
using Auctions.Domain.Enums;

namespace Auctions.Application.Interfaces;

public interface IAuctionStreamingExportRepository
{
    IAsyncEnumerable<Auction> StreamAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        int batchSize = 500,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<Auction> StreamAuctionsByCursorAsync(
        Guid? lastCursorId = null,
        int batchSize = 500,
        Status? status = null,
        CancellationToken cancellationToken = default);
}
