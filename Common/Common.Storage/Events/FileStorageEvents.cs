namespace Common.Storage.Events;

public record FileUploadedEvent
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string TempPath { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? UploadedBy { get; init; }
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
}

public record FileConfirmedEvent
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string PermanentPath { get; init; } = string.Empty;
    public string? Url { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? ConfirmedBy { get; init; }
    public DateTime ConfirmedAt { get; init; } = DateTime.UtcNow;
}

public record FileDeletedEvent
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? DeletedBy { get; init; }
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
}
