using StorageService.Domain.Enums;

namespace StorageService.Application.DTOs;

public record FileMetadataDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Checksum { get; init; }
    public StorageProvider Provider { get; init; }
    public FileStatus Status { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public string? UploadedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

public record FileUploadResult
{
    public bool Success { get; init; }
    public FileMetadataDto? Metadata { get; init; }
    public string? Error { get; init; }
    
    public static FileUploadResult Ok(FileMetadataDto metadata) => new() { Success = true, Metadata = metadata };
    public static FileUploadResult Fail(string error) => new() { Success = false, Error = error };
}

public record RequestUploadDto
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}

public record UploadUrlResponse
{
    public Guid FileId { get; init; }
    public string UploadUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public Dictionary<string, string>? RequiredHeaders { get; init; }
}

public record ConfirmUploadRequest
{
    public Guid FileId { get; init; }
    public string? OwnerId { get; init; }
    public string? Checksum { get; init; }
}

public record BatchConfirmRequest
{
    public List<ConfirmUploadRequest> Files { get; init; } = new();
}

public record DownloadUrlResponse
{
    public Guid FileId { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
}

public record FileConfirmRequest
{
    public Guid FileId { get; init; }
    public string? OwnerId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}
