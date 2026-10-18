using BuildingBlocks.Domain.Guards;
using Storage.Domain.Constants;
using Storage.Domain.Enums;
using Storage.Domain.Events;

namespace Storage.Domain.Entities;

public class StoredFile : AggregateRoot
{
    private StoredFile() { }
    public string FileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? SubFolder { get; private set; }
    public Guid? OwnerId { get; private set; }
    public FileStatus Status { get; private set; } = FileStatus.Pending;
    public StorageProvider Provider { get; private set; } = StorageProvider.Local;
    public Guid? ReportRequestId { get; private set; }
    public DateTimeOffset? UploadExpiresAt { get; private set; }
    public string? Checksum { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    public static StoredFile Create(CreateStoredFileParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        Guard.AgainstNullOrEmpty(parameters.FileName, nameof(parameters.FileName));
        Guard.AgainstNullOrEmpty(parameters.StoredFileName, nameof(parameters.StoredFileName));
        Guard.AgainstNullOrEmpty(parameters.ContentType, nameof(parameters.ContentType));
        Guard.AgainstNonPositive(parameters.FileSize, nameof(parameters.FileSize));
        Guard.AgainstNullOrEmpty(parameters.Url, nameof(parameters.Url));

        var file = new StoredFile
        {
            Id = Guid.NewGuid(),
            FileName = parameters.FileName,
            StoredFileName = parameters.StoredFileName,
            ContentType = parameters.ContentType,
            FileSize = parameters.FileSize,
            Url = parameters.Url,
            SubFolder = parameters.SubFolder,
            OwnerId = parameters.OwnerId,
            Status = FileStatus.Ready,
            Provider = parameters.Provider,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        file.AddDomainEvent(new FileUploadedDomainEvent(
            file.Id,
            file.FileName,
            file.ContentType,
            file.FileSize,
            file.Url,
            file.OwnerId));

        return file;
    }

    public static StoredFile ReserveUpload(ReserveStoredFileUploadParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return new StoredFile
        {
            Id = parameters.Id,
            FileName = parameters.FileName,
            StoredFileName = parameters.StoredFileName,
            ContentType = parameters.ContentType,
            FileSize = parameters.FileSize,
            SubFolder = parameters.SubFolder,
            OwnerId = parameters.OwnerId,
            Provider = parameters.Provider,
            UploadExpiresAt = parameters.ExpiresAt,
            Status = FileStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetReportRequestId(Guid requestId) => ReportRequestId = requestId;

    public void RecordProcessedImage(string checksum, int width, int height, string thumbnailDataUrl)
    {
        if (Status != FileStatus.Ready || IsDeleted)
            throw new InvalidOperationException("Only ready files can be processed.");
        Guard.AgainstNullOrEmpty(checksum, nameof(checksum));
        Guard.AgainstNonPositive(width, nameof(width));
        Guard.AgainstNonPositive(height, nameof(height));
        Guard.AgainstNullOrEmpty(thumbnailDataUrl, nameof(thumbnailDataUrl));
        Checksum = checksum;
        Metadata[StorageDefaults.Images.WidthKey] = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Metadata[StorageDefaults.Images.HeightKey] = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Metadata[StorageDefaults.Images.ThumbnailKey] = thumbnailDataUrl;
        Metadata[StorageDefaults.Images.VersionKey] = StorageDefaults.Images.ProcessingVersion;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ConfirmUpload(string url)
    {
        if (Status != FileStatus.Pending || UploadExpiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Upload reservation is no longer valid.");
        Guard.AgainstNullOrEmpty(url, nameof(url));
        Url = url;
        Status = FileStatus.Ready;
        AddDomainEvent(new FileUploadedDomainEvent(Id, FileName, ContentType, FileSize, Url, OwnerId));
    }

    public void MarkAsDeleted(Guid? deletedBy)
    {
        Status = FileStatus.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy ?? Guid.Empty;

        AddDomainEvent(new FileDeletedDomainEvent(Id, FileName, OwnerId));
    }
}
