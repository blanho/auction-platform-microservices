using Search.Api.Models;

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
