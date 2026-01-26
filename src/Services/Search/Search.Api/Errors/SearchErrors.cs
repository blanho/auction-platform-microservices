using BuildingBlocks.Application.Abstractions;

namespace Search.Api.Errors;

public static class SearchErrors
{
    public static class Search
    {
        public static Error Failed(string reason) => Error.Create("Search.Failed", $"Search failed: {reason}");
        public static Error InvalidQuery => Error.Create("Search.InvalidQuery", "Invalid search query");
        public static Error IndexNotReady => Error.Create("Search.IndexNotReady", "Search index is not ready");
    }

    public static class Auction
    {
        public static Error NotFound => Error.Create("Auction.NotFound", "Auction not found in search index");
        public static Error NotFoundById(Guid id) => Error.Create("Auction.NotFound", $"Auction with ID {id} was not found in search index");
        public static Error IndexFailed(Guid id) => Error.Create("Auction.IndexFailed", $"Failed to index auction {id}");
        public static Error RemoveFailed(Guid id) => Error.Create("Auction.RemoveFailed", $"Failed to remove auction {id} from index");
    }

    public static class Index
    {
        public static Error CreateFailed(string reason) => Error.Create("Index.CreateFailed", $"Failed to create index: {reason}");
        public static Error RebuildFailed(string reason) => Error.Create("Index.RebuildFailed", $"Failed to rebuild index: {reason}");
        public static Error HealthCheckFailed(string reason) => Error.Create("Index.HealthCheckFailed", $"Index health check failed: {reason}");
        public static Error NotFoundByName(string indexName) => Error.Create("Index.NotFound", $"Index '{indexName}' not found");
    }

    public static class Suggestion
    {
        public static Error Failed(string reason) => Error.Create("Suggestion.Failed", $"Failed to get suggestions: {reason}");
    }
}
