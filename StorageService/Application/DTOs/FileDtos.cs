using StorageService.Domain.Entities;

namespace StorageService.Application.DTOs;

public record FileMetadataDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Path { get; init; } = string.Empty;
    public string Url { get; init; }
    public FileStatus Status { get; init; }
    public string EntityId { get; init; }
    public string EntityType { get; init; }
    public string UploadedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public Dictionary<string, string> Tags { get; init; }
}

public record FileUploadResult
{
    public bool Success { get; init; }
    public FileMetadataDto Metadata { get; init; }
    public string Error { get; init; }
    
    public static FileUploadResult Ok(FileMetadataDto metadata) => new() { Success = true, Metadata = metadata };
    public static FileUploadResult Fail(string error) => new() { Success = false, Error = error };
}

public record FileConfirmRequest
{
    public Guid FileId { get; init; }
    public string EntityId { get; init; }
    public string EntityType { get; init; }
    public Dictionary<string, string> Tags { get; init; }
}

public record BatchConfirmRequest
{
    public List<FileConfirmRequest> Files { get; init; } = new();
}

public record FileUploadRequest
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public long Size { get; init; }
    public string UploadedBy { get; init; }
}
