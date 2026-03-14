using BuildingBlocks.Application.Abstractions;
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
    Task<Result> EnsureIndexExistsAsync(CancellationToken ct = default);

    Task<Result> RecreateIndexAsync(CancellationToken ct = default);

    Task<Result<IndexStats>> GetIndexStatsAsync(CancellationToken ct = default);

    Task<Result> IsHealthyAsync(CancellationToken ct = default);
}

public interface IAuctionIndexService
{
    Task<Result> IndexAsync(AuctionDocument document, CancellationToken ct = default);

    Task<Result<BulkIndexResult>> BulkIndexAsync(IEnumerable<AuctionDocument> documents, CancellationToken ct = default);

    Task<Result> PartialUpdateAsync(Guid auctionId, object partialDocument, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid auctionId, CancellationToken ct = default);

    Task<Result> UpdateBidInfoAsync(Guid auctionId, decimal currentPrice, int bidCount, CancellationToken ct = default);
}
