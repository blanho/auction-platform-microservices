using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IAuctionBulkRepository
{
    Task BulkInsertAsync(IReadOnlyList<Auction> auctions, CancellationToken cancellationToken = default);
    Task<int> CountByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}
