using Common.Storage.Enums;

namespace Common.Storage.Configuration;

    
public class StorageOptions
{
    public const string SectionName = "Storage";
    
    public StorageType Provider { get; set; } = StorageType.Local;
    
    public string LocalBasePath { get; set; } = "uploads";
    
    public string TempFolder { get; set; } = "temp";
    
    public string PermanentFolder { get; set; } = "files";

    public string? AzureBlobConnectionString { get; set; }
    
    public string? AzureBlobContainer { get; set; }
    
    public S3Options? S3 { get; set; }
    
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    
    public List<string> AllowedExtensions { get; set; } = new();
    
    public int TempFileExpirationHours { get; set; } = 24;
    
    public string? BaseUrl { get; set; }
}

public class S3Options
{
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? Region { get; set; }
    public string? BucketName { get; set; }
}
