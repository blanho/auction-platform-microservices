namespace Storage.Application.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string TempFolder { get; set; } = "temp";
    public string PermanentFolder { get; set; } = "files";
    public string QuarantineFolder { get; set; } = "quarantine";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
    public string[] AllowedContentTypes { get; set; } =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    public int TempFileExpirationHours { get; set; } = 24;
    public int UploadUrlExpirationMinutes { get; set; } = 15;
    public int DownloadUrlExpirationMinutes { get; set; } = 60;
    public string? BucketName { get; set; }
    public string? BaseUrl { get; set; }

    public AzureStorageOptions Azure { get; set; } = new();

    public ScanOptions Scanning { get; set; } = new();

    public LifecycleOptions Lifecycle { get; set; } = new();

    public MultipartOptions Multipart { get; set; } = new();

    public string DefaultProvider { get; set; } = "azure";
}

public class AzureStorageOptions
{
    public const string SectionName = "Storage:Azure";

    public string? ConnectionString { get; set; }

    public string? AccountName { get; set; }

    public string? AccountKey { get; set; }

    public bool UseManagedIdentity { get; set; } = false;

    public string TempContainerName { get; set; } = "auction-temp";
    public string MediaContainerName { get; set; } = "auction-storage";
    public string QuarantineContainerName { get; set; } = "auction-quarantine";

    public string TempPrefix { get; set; } = "temp/";
    public string MediaPrefix { get; set; } = "files/";
    public string QuarantinePrefix { get; set; } = "quarantine/";

    public string? CdnEndpoint { get; set; }
    public bool UseCdnForDownloads { get; set; } = false;

    public bool UseDefenderForStorage { get; set; } = true;
    public string ScanStatusTagKey { get; set; } = "Malware Scanning scan result";
    public string ScanResultClean { get; set; } = "No threats found";
    public string ScanResultMalicious { get; set; } = "Malicious";
}

public class ScanOptions
{
    public const string SectionName = "Storage:Scanning";

    public bool Enabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 10;

    public int StatusCheckIntervalSeconds { get; set; } = 5;
    public int MaxStatusCheckAttempts { get; set; } = 60;

    public string ScanStatusTagKey { get; set; } = "scan-status";
    public string ScanResultTagKey { get; set; } = "scan-result";
    public string ScanTimestampTagKey { get; set; } = "scan-timestamp";
    public string ThreatDetailsTagKey { get; set; } = "threat-details";

    public string? ScannerEndpoint { get; set; }
    public string? ScannerApiKey { get; set; }

    public string[] ExemptContentTypes { get; set; } = Array.Empty<string>();
    public long MaxScanFileSize { get; set; } = 100 * 1024 * 1024;
}

public class LifecycleOptions
{
    public const string SectionName = "Storage:Lifecycle";

    public int TempFileMaxAgeHours { get; set; } = 24;
    public int PendingScanMaxAgeHours { get; set; } = 4;
    public int FailedScanRetentionDays { get; set; } = 7;

    public int QuarantineRetentionDays { get; set; } = 30;

    public int SoftDeleteRetentionDays { get; set; } = 30;

    public int OrphanFileMaxAgeDays { get; set; } = 7;

    public int ReconciliationBatchSize { get; set; } = 100;
    public bool EnableReconciliation { get; set; } = true;
}

public class MultipartOptions
{
    public const string SectionName = "Storage:Multipart";

    public long ThresholdBytes { get; set; } = 100 * 1024 * 1024;

    public long PartSize { get; set; } = 10 * 1024 * 1024;
    public long MinPartSize { get; set; } = 5 * 1024 * 1024;
    public long MaxPartSize { get; set; } = 100 * 1024 * 1024;
    public int MaxParts { get; set; } = 10000;

    public int PartUploadUrlExpirationMinutes { get; set; } = 60;

    public int IncompleteUploadMaxAgeDays { get; set; } = 7;
}
