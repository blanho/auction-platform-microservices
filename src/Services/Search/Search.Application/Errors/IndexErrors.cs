using BuildingBlocks.Application.Abstractions;

namespace Search.Application.Errors;

public static class IndexErrors
{
    public static Error IndexingFailed(Guid auctionId, string reason) =>
        LocalizableError.WithDiagnostic("IndexError.IndexingFailed", $"Failed to index auction {auctionId}: {reason}", auctionId, reason);

    public static Error DocumentNotFound(Guid auctionId) =>
        LocalizableError.Localizable("IndexError.DocumentNotFound", $"Auction document {auctionId} not found in index", auctionId);

    public static Error UpdateFailed(Guid auctionId, string reason) =>
        LocalizableError.WithDiagnostic("IndexError.UpdateFailed", $"Failed to update auction {auctionId}: {reason}", auctionId, reason);

    public static Error DeleteFailed(Guid auctionId, string reason) =>
        LocalizableError.WithDiagnostic("IndexError.DeleteFailed", $"Failed to delete auction {auctionId}: {reason}", auctionId, reason);

    public static Error BulkIndexFailed(int failedCount, string reason) =>
        LocalizableError.WithDiagnostic("IndexError.BulkIndexFailed", $"Bulk index failed for {failedCount} documents: {reason}", failedCount, reason);

    public static Error ConnectionFailed(string reason) =>
        LocalizableError.WithDiagnostic("IndexError.ConnectionFailed", $"Failed to connect to Elasticsearch: {reason}", reason);

    public static Error IndexCreationFailed(string indexName, string reason) =>
        LocalizableError.WithDiagnostic("IndexError.IndexCreationFailed", $"Failed to create index {indexName}: {reason}", indexName, reason);
}
