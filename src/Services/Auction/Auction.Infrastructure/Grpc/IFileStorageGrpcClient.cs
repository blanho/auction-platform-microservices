namespace Auctions.Infrastructure.Grpc;

public record FileConfirmResult
{
    public Guid FileId { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Url { get; init; }
}

public record FileMetadata
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Url { get; init; }
    public string? ThumbnailUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record TempUploadResult
{
    public Guid FileId { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public interface IFileStorageGrpcClient
{
    Task<TempUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);

    Task<FileConfirmResult> ConfirmFileAsync(
        Guid fileId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<List<FileConfirmResult>> ConfirmFilesBatchAsync(
        IEnumerable<(Guid FileId, string EntityType, string EntityId)> files,
        CancellationToken cancellationToken = default);

    Task<FileConfirmResult> UploadAndConfirmAsync(
        Stream stream,
        string fileName,
        string contentType,
        string entityType,
        string entityId,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);

    Task<FileMetadata?> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
}
