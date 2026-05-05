using Search.Api.Documents;
using Search.Api.Models;

namespace Search.Api.Interfaces;

public interface IAuctionIndexService
{
    Task<Result> IndexAsync(AuctionDocument document, CancellationToken ct = default);

    Task<Result<BulkIndexResult>> BulkIndexAsync(IEnumerable<AuctionDocument> documents, CancellationToken ct = default);

    Task<Result> PartialUpdateAsync(Guid auctionId, object partialDocument, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid auctionId, CancellationToken ct = default);

    Task<Result> UpdateBidInfoAsync(Guid auctionId, decimal currentPrice, int bidCount, CancellationToken ct = default);
}
