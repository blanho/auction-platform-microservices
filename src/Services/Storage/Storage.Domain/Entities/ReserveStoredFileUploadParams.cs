using Storage.Domain.Enums;

namespace Storage.Domain.Entities;

public sealed record ReserveStoredFileUploadParams
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string StoredFileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public string? SubFolder { get; init; }
    public required Guid OwnerId { get; init; }
    public required StorageProvider Provider { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
