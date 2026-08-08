using BuildingBlocks.Application.Abstractions;
using Search.Domain.Documents;
using Search.Domain.Models;

namespace Search.Application.Interfaces;

public interface IAuctionIndexService
{
    Task<Result> IndexAsync(AuctionDocument document, CancellationToken ct = default);

    Task<Result<BulkIndexResult>> BulkIndexAsync(IEnumerable<AuctionDocument> documents, CancellationToken ct = default);

    Task<Result> PartialUpdateAsync(Guid auctionId, object partialDocument, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid auctionId, CancellationToken ct = default);

    Task<Result> UpdateBidInfoAsync(Guid auctionId, decimal currentPrice, int bidCount, CancellationToken ct = default);
}
