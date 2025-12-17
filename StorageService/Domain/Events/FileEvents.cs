#nullable enable

namespace StorageService.Domain.Events;

public record FileUploadedEvent
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Checksum { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public string? UploadedBy { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}

public record FileConfirmedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public DateTimeOffset ConfirmedAt { get; init; }
}

public record FileDeletedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}

public record FileUploadFailedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
}
