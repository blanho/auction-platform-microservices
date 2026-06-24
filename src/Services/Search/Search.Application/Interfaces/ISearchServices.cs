using Search.Domain.Constants;
using Search.Domain.Models;

namespace Search.Application.Interfaces;

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
        int maxSuggestions = SearchDefaults.DefaultAutocompleteLimit,
        CancellationToken ct = default);
}
