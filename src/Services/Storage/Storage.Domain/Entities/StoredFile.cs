#nullable enable
using BuildingBlocks.Domain.Exceptions;
using Storage.Domain.Enums;
using Storage.Domain.ValueObjects;

namespace Storage.Domain.Entities;

public class StoredFile : BaseEntity
{
    private FileStatus _status = FileStatus.Pending;

    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string? Checksum { get; private set; }
    public string? ContentHash { get; private set; }

    public StorageProvider Provider { get; private set; } = StorageProvider.Local;
    public string StoragePath { get; private set; } = string.Empty;
    public string? BucketName { get; private set; }
    public string? TempPath { get; private set; }

    public Guid? ResourceId { get; private set; }
    public StorageResourceType? ResourceType { get; private set; }
    public PortalType PortalType { get; private set; } = PortalType.System;

    public string OwnerService { get; private set; } = string.Empty;
    public string? OwnerId { get; private set; }
    public string? UploadedBy { get; private set; }

    public FileStatus Status
    {
        get => _status;
        private set => _status = value;
    }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public string? FailureReason { get; private set; }

    public FilePermissions? Permissions { get; private set; }
    public ScanResult? ScanResult { get; private set; }

    public FileExtendedProperties ExtendedProperties { get; private set; } = new();

    public Dictionary<string, string>? Metadata { get; private set; }

    public static StoredFile Create(
        string fileName,
        string originalFileName,
        string contentType,
        long size,
        StorageProvider provider,
        string storagePath,
        string ownerService,
        string? ownerId = null,
        string? uploadedBy = null,
        string? bucketName = null,
        TimeSpan? expiresIn = null,
        PortalType portalType = PortalType.System,
        StorageResourceType? resourceType = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName cannot be empty", nameof(fileName));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("OriginalFileName cannot be empty", nameof(originalFileName));
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        return new StoredFile
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            Size = size,
            Provider = provider,
            StoragePath = storagePath,
            BucketName = bucketName,
            OwnerService = ownerService,
            OwnerId = ownerId,
            UploadedBy = uploadedBy,
            PortalType = portalType,
            ResourceType = resourceType,
            _status = FileStatus.Pending,
            ExpiresAt = expiresIn.HasValue ? DateTimeOffset.UtcNow.Add(expiresIn.Value) : null,
            CreatedAt = DateTimeOffset.UtcNow,
            Permissions = FilePermissions.OwnerOnly(),
            ExtendedProperties = new FileExtendedProperties
            {
                TempPath = storagePath
            }
        };
    }

    #region Status Transitions

    public void MarkInTemp(string? tempPath = null)
    {
        if (Status != FileStatus.Pending)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only mark Pending files as InTemp");

        Status = FileStatus.InTemp;
        if (tempPath != null)
        {
            TempPath = tempPath;
            ExtendedProperties.TempPath = tempPath;
        }
    }

    public void MarkScanning()
    {
        if (Status != FileStatus.InTemp)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only scan files in InTemp status");

        Status = FileStatus.Scanning;
    }

    public void MarkScanned(ScanResult scanResult)
    {
        if (Status != FileStatus.Scanning && Status != FileStatus.InTemp)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only mark InTemp/Scanning files as Scanned");

        if (!scanResult.IsClean)
            throw new ArgumentException("Scan result must be clean to mark as scanned");

        Status = FileStatus.Scanned;
        ScanResult = scanResult;

        ExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
    }

    public void MarkInfected(ScanResult scanResult)
    {
        if (Status != FileStatus.Scanning && Status != FileStatus.InTemp)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only mark InTemp/Scanning files as Infected");

        Status = FileStatus.Infected;
        ScanResult = scanResult;
        FailureReason = $"Infected: {scanResult.ThreatDetails}";
    }

    public void MarkInMedia(string permanentPath, Guid? resourceId = null, StorageResourceType? resourceType = null)
    {
        if (Status != FileStatus.Scanned && Status != FileStatus.Uploaded)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only move Scanned/Uploaded files to InMedia");

        Status = FileStatus.InMedia;
        StoragePath = permanentPath;
        TempPath = null;
        ExtendedProperties.TempPath = null;
        ExpiresAt = null;
        ConfirmedAt = DateTimeOffset.UtcNow;

        if (resourceId.HasValue)
            ResourceId = resourceId;
        if (resourceType.HasValue)
            ResourceType = resourceType;
    }

    public void MarkProcessing(string jobId)
    {
        if (Status != FileStatus.InMedia)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only process files in InMedia status");

        Status = FileStatus.Processing;
        ExtendedProperties.ProcessingJobId = jobId;
    }

    public void MarkProcessingComplete()
    {
        if (Status != FileStatus.Processing)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only complete processing for Processing files");

        Status = FileStatus.InMedia;
        ExtendedProperties.ProcessingJobId = null;
        ExtendedProperties.ProcessingError = null;
    }

    public void MarkProcessingFailed(string error)
    {
        if (Status != FileStatus.Processing)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only fail processing for Processing files");

        Status = FileStatus.InMedia;
        ExtendedProperties.ProcessingError = error;
    }

    public void MarkExpired(string? reason = null)
    {
        Status = FileStatus.Expired;
        if (reason != null)
            FailureReason = reason;
    }

    public void MarkRemoved(string? reason = null)
    {
        Status = FileStatus.Removed;
        DeletedAt = DateTimeOffset.UtcNow;
        IsDeleted = true;
        if (reason != null)
            FailureReason = reason;
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

        Status = FileStatus.Failed;
        FailureReason = reason;
    }

    public void AssociateWithResource(Guid resourceId, StorageResourceType resourceType)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
    }

    public void SetQuarantinePath(string? quarantinePath)
    {
        if (quarantinePath != null)
        {
            ExtendedProperties.TempPath = quarantinePath;
            if (ScanResult != null)
            {
                ScanResult.QuarantinePath = quarantinePath;
                ScanResult.IsQuarantined = true;
            }
        }
    }

    public void SetMultipartUpload(string uploadId, int partsCount)
    {
        ExtendedProperties.UploadId = uploadId;
        ExtendedProperties.PartsCount = partsCount;
        ExtendedProperties.CompletedParts = new List<int>();
    }

    public void CompleteMultipartUpload(List<int> completedParts)
    {
        ExtendedProperties.CompletedParts = completedParts;
    }

    #endregion

    #region Legacy Status Methods (for backward compatibility)

    public void MarkUploaded(string? ownerId = null)
    {
        if (Status != FileStatus.Scanned && Status != FileStatus.Pending)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only mark Scanned or Pending files as Uploaded");

        Status = FileStatus.Uploaded;
        if (ownerId != null)
        {
            OwnerId = ownerId;
        }
    }

    public void Confirm(string? newStoragePath = null, string? ownerId = null, string? ownerService = null)
    {
        if (Status != FileStatus.Uploaded && Status != FileStatus.Scanned)
            throw new InvalidEntityStateException(nameof(StoredFile), Status.ToString(), "Can only confirm Uploaded or Scanned files");

        Status = FileStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        ExpiresAt = null;

        if (!string.IsNullOrEmpty(newStoragePath))
        {
            StoragePath = newStoragePath;
        }

        if (ownerId != null)
        {
            OwnerId = ownerId;
        }

        if (ownerService != null)
        {
            OwnerService = ownerService;
        }
    }

    #endregion

    #region Property Setters

    public void SetChecksum(string checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
            throw new ArgumentException("Checksum cannot be empty", nameof(checksum));
        Checksum = checksum;
    }

    public void SetContentHash(string hash)
    {
        ContentHash = hash;
        ExtendedProperties.DeduplicationHash = hash;
    }

    public void SetMetadata(Dictionary<string, string> metadata)
    {
        Metadata = metadata ?? new Dictionary<string, string>();
        ExtendedProperties.Metadata = Metadata;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata ??= new Dictionary<string, string>();
        Metadata[key] = value;
        ExtendedProperties.Metadata = Metadata;
    }

    public void SetPermissions(FilePermissions permissions)
    {
        Permissions = permissions;
    }

    public void SetResourceLink(Guid resourceId, StorageResourceType resourceType)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
    }

    public void SetCdnUrl(string cdnUrl)
    {
        ExtendedProperties.CdnUrl = cdnUrl;
    }

    public void SetThumbnail(string thumbnailPath, string? cdnUrl = null)
    {
        ExtendedProperties.ThumbnailPath = thumbnailPath;
        ExtendedProperties.ThumbnailCdnUrl = cdnUrl;
    }

    public void SetDimensions(int width, int height)
    {
        ExtendedProperties.Dimensions = new ImageDimensions(width, height);
    }

    #endregion

    #region Computed Properties

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;

    public bool IsAvailableForDownload => Status == FileStatus.InMedia || Status == FileStatus.Confirmed;

    public bool IsPendingUpload => Status == FileStatus.Pending;

    public bool IsPendingScan => Status == FileStatus.InTemp || Status == FileStatus.Scanning;

    public bool IsPendingSubmit => Status == FileStatus.Scanned || Status == FileStatus.Uploaded;

    public bool CanBeDeleted => Status == FileStatus.Pending ||
                                Status == FileStatus.Failed ||
                                Status == FileStatus.Removed ||
                                Status == FileStatus.Expired ||
                                IsExpired;

    public bool RequiresScan => Status == FileStatus.InTemp;

    #endregion
}
