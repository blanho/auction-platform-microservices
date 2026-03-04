using BuildingBlocks.Application.Abstractions;

namespace Search.Api.Errors;

public static class SearchErrors
{
    public static class Search
    {
        public static Error Failed(string reason) => LocalizableError.Localizable("Search.Failed", $"Search failed: {reason}", reason);
        public static Error InvalidQuery => Error.Create("Search.InvalidQuery", "Invalid search query");
        public static Error IndexNotReady => Error.Create("Search.IndexNotReady", "Search index is not ready");
    }

    public static class Auction
    {
        public static Error NotFound => Error.Create("Auction.NotFound", "Auction not found in search index");
        public static Error NotFoundById(Guid id) => Error.Create("Auction.NotFound", $"Auction with ID {id} was not found in search index");
        public static Error IndexFailed(Guid id) => LocalizableError.Localizable("Auction.IndexFailed", $"Failed to index auction {id}", id);
        public static Error RemoveFailed(Guid id) => LocalizableError.Localizable("Auction.RemoveFailed", $"Failed to remove auction {id} from index", id);
    }

    public static class Index
    {
        public static Error CreateFailed(string reason) => LocalizableError.Localizable("Index.CreateFailed", $"Failed to create index: {reason}", reason);
        public static Error RebuildFailed(string reason) => LocalizableError.Localizable("Index.RebuildFailed", $"Failed to rebuild index: {reason}", reason);
        public static Error HealthCheckFailed(string reason) => LocalizableError.Localizable("Index.HealthCheckFailed", $"Index health check failed: {reason}", reason);
        public static Error NotFoundByName(string indexName) => LocalizableError.Localizable("Index.NotFound", $"Index '{indexName}' not found", indexName);
    }

    public static class Suggestion
    {
        public static Error Failed(string reason) => LocalizableError.Localizable("Suggestion.Failed", $"Failed to get suggestions: {reason}", reason);
    }
}
