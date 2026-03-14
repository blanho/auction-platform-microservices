namespace Storage.Application.DTOs;

public record StoredFileDto(
    Guid FileId,
    string FileName,
    string ContentType,
    long FileSize,
    string Url,
    DateTimeOffset UploadedAt
);

public record BatchUploadResultDto(
    List<StoredFileDto> Files
);

public record FileUrlDto(
    Guid FileId,
    string Url
);

public record PresignedUploadDto(
    string FileId,
    string StoredFileName,
    string UploadUrl,
    Dictionary<string, string> Headers,
    DateTimeOffset ExpiresAt
);

public record PresignedDownloadDto(
    string DownloadUrl,
    string FileName,
    string ContentType,
    DateTimeOffset ExpiresAt
);
