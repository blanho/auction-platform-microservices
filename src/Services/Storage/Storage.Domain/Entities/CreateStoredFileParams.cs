using Storage.Domain.Enums;

namespace Storage.Domain.Entities;

public sealed record CreateStoredFileParams
{
    public required string FileName { get; init; }
    public required string StoredFileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required string Url { get; init; }
    public string? SubFolder { get; init; }
    public Guid? OwnerId { get; init; }
    public required StorageProvider Provider { get; init; }
}
