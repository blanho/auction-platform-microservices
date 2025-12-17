namespace StorageService.Application.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";
    
    public string TempFolder { get; set; } = "temp";
    public string PermanentFolder { get; set; } = "files";
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
}
