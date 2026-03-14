#nullable enable
namespace BuildingBlocks.Application.Abstractions.Storage;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = "Local";
    public LocalStorageSettings Local { get; set; } = new();
    public AzureBlobSettings AzureBlob { get; set; } = new();
    public FileValidationSettings Validation { get; set; } = new();
}

public class LocalStorageSettings
{
    public string BasePath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/files";
}

public class AzureBlobSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "uploads";
}

public class FileValidationSettings
{
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".csv", ".xlsx"];
    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf", "text/csv",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];
    public int MaxFilesPerUpload { get; set; } = 10;
}
