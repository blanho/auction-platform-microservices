using BuildingBlocks.Application.Abstractions;

namespace Search.Api.Errors;

public static class IndexErrors
{
    public static Error IndexingFailed(Guid auctionId, string reason) =>
        Error.Create("IndexError.IndexingFailed", $"Failed to index auction {auctionId}: {reason}");

    public static Error DocumentNotFound(Guid auctionId) =>
        Error.Create("IndexError.DocumentNotFound", $"Auction document {auctionId} not found in index");

    public static Error UpdateFailed(Guid auctionId, string reason) =>
        Error.Create("IndexError.UpdateFailed", $"Failed to update auction {auctionId}: {reason}");

    public static Error DeleteFailed(Guid auctionId, string reason) =>
        Error.Create("IndexError.DeleteFailed", $"Failed to delete auction {auctionId}: {reason}");

    public static Error BulkIndexFailed(int failedCount, string reason) =>
        Error.Create("IndexError.BulkIndexFailed", $"Bulk index failed for {failedCount} documents: {reason}");

    public static Error ConnectionFailed(string reason) =>
        Error.Create("IndexError.ConnectionFailed", $"Failed to connect to Elasticsearch: {reason}");

    public static Error IndexCreationFailed(string indexName, string reason) =>
        Error.Create("IndexError.IndexCreationFailed", $"Failed to create index {indexName}: {reason}");
}
