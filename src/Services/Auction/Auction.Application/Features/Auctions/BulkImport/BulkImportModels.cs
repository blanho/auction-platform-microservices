namespace Auctions.Application.Features.Auctions.BulkImport;

public record BulkImportJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public BulkImportStatus Status { get; set; } = BulkImportStatus.Pending;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkImportError> Errors { get; set; } = new();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum BulkImportStatus
{
    Pending,
    Counting,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public class BulkImportError
{
    public int RowNumber { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class BulkImportProgress
{
    public Guid JobId { get; set; }
    public BulkImportStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double ProgressPercentage => TotalRows > 0 ? Math.Round((double)ProcessedRows / TotalRows * 100, 1) : 0;
    public int EstimatedSecondsRemaining { get; set; }
    public List<BulkImportError> RecentErrors { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
