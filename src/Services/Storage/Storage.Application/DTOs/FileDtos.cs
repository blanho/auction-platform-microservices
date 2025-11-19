using Storage.Domain.Enums;
using Storage.Domain.ValueObjects;

namespace Storage.Application.DTOs;

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

    public Guid? ResourceId { get; init; }
    public StorageResourceType? ResourceType { get; init; }
    public PortalType PortalType { get; init; }
    public ScanResult? ScanResult { get; init; }
    public string? DownloadUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
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
    public StorageResourceType? ResourceType { get; init; }
    public PortalType PortalType { get; init; } = PortalType.System;
    public bool RequireScan { get; init; } = true;
}

public record UploadUrlResponse
{
    public Guid FileId { get; init; }
    public string UploadUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public Dictionary<string, string>? RequiredHeaders { get; init; }

    public long MaxSizeBytes { get; init; }
    public string? RequiredContentType { get; init; }
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

public record StorageUploadResult(
    bool Success,
    string? Path = null,
    string? Url = null,
    string? Checksum = null,
    string? Error = null);

public record PreSignedUrlResult(
    bool Success,
    string? Url = null,
    DateTimeOffset? ExpiresAt = null,
    Dictionary<string, string>? RequiredHeaders = null,
    string? Error = null);

#region New DTOs for Enhanced Flow

public record SubmitFileRequest
{
    public Guid FileId { get; init; }
    public Guid? ResourceId { get; init; }
    public StorageResourceType? ResourceType { get; init; }
    public string? OwnerId { get; init; }
    public string? OwnerService { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
    public FilePermissions? Permissions { get; init; }
}

public record SubmitFileResponse
{
    public bool Success { get; init; }
    public FileMetadataDto? File { get; init; }
    public string? DownloadUrl { get; init; }
    public string? Error { get; init; }
}

public record ScanStatusResponse
{
    public Guid FileId { get; init; }
    public FileStatus Status { get; init; }
    public ScanResult? ScanResult { get; init; }
    public bool IsComplete { get; init; }
    public string? Message { get; init; }
}

public record InitiateMultipartUploadRequest
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long TotalSize { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public StorageResourceType? ResourceType { get; init; }
}

public record MultipartUploadSession
{
    public Guid FileId { get; init; }
    public string UploadId { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public List<UploadPartInfo> Parts { get; init; } = new();
    public DateTimeOffset ExpiresAt { get; init; }
}

public record UploadPartInfo
{
    public int PartNumber { get; init; }
    public long Size { get; init; }
    public string UploadUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}

public record CompleteMultipartUploadRequest
{
    public Guid FileId { get; init; }
    public string UploadId { get; init; } = string.Empty;
    public List<CompletedPartInfo> Parts { get; init; } = new();
}

public record CompletedPartInfo
{
    public int PartNumber { get; init; }
    public string ETag { get; init; } = string.Empty;
}

public record FileTypeValidationResult
{
    public bool IsAllowed { get; init; }
    public string? Message { get; init; }
    public bool HasWarning { get; init; }

    public static FileTypeValidationResult Allowed() => new() { IsAllowed = true };
    public static FileTypeValidationResult AllowedWithWarning(string warning) => new()
    {
        IsAllowed = true,
        HasWarning = true,
        Message = warning
    };
    public static FileTypeValidationResult Rejected(string reason) => new()
    {
        IsAllowed = false,
        Message = reason
    };
}

public record StorageFileInfo
{
    public string Path { get; init; } = string.Empty;
    public string? ContentType { get; init; }
    public long Size { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? ContentHash { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

#endregion
