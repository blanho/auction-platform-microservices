using Search.Api.Documents;
using Search.Api.Models;
using Search.Api.Services;

namespace Search.Api.Interfaces;

public interface IAuctionSearchService
{
    Task<AuctionSearchResponse> SearchAsync(
        AuctionSearchRequest request,
        CancellationToken ct = default);

    Task<AuctionSearchResult?> GetByIdAsync(
        Guid auctionId,
        CancellationToken ct = default);

    Task<IReadOnlyList<AutocompleteSuggestion>> AutocompleteAsync(
        string prefix,
        int maxSuggestions = 10,
        CancellationToken ct = default);
}

public interface IIndexManagementService
{
    Task<bool> EnsureIndexExistsAsync(CancellationToken ct = default);

    Task RecreateIndexAsync(CancellationToken ct = default);

    Task<IndexStats> GetIndexStatsAsync(CancellationToken ct = default);

    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}

public interface IAuctionIndexService
{
    Task<bool> IndexAsync(AuctionDocument document, CancellationToken ct = default);

    Task<BulkIndexResult> BulkIndexAsync(IEnumerable<AuctionDocument> documents, CancellationToken ct = default);

    Task<bool> PartialUpdateAsync(Guid auctionId, object partialDocument, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid auctionId, CancellationToken ct = default);

    Task<bool> UpdateBidInfoAsync(Guid auctionId, decimal currentPrice, int bidCount, CancellationToken ct = default);
}
