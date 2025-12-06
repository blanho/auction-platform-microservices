using Common.Storage.Enums;

namespace Common.Storage.Models;

public record FileMetadata
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Path { get; init; } = string.Empty;
    public string? Url { get; init; }
    public FileStatus Status { get; init; } = FileStatus.Temporary;
    public string? EntityId { get; init; }
    public string? EntityType { get; init; }
    public string? UploadedBy { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; init; }
    public Dictionary<string, string>? Tags { get; init; }
}

public record FileUploadResult
{
    public bool Success { get; init; }
    public FileMetadata? Metadata { get; init; }
    public string? Error { get; init; }
    
    public static FileUploadResult Ok(FileMetadata metadata) => new() { Success = true, Metadata = metadata };
    public static FileUploadResult Fail(string error) => new() { Success = false, Error = error };
}

public record FileConfirmRequest
{
    public Guid FileId { get; init; }
    public string? EntityId { get; init; }
    public string? EntityType { get; init; }
    public Dictionary<string, string>? Tags { get; init; }
}
