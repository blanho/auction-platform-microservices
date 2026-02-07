#nullable enable
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value.Local;
        _logger = logger;
        EnsureDirectoryExists(_settings.BasePath);
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var storedFileName = $"{fileId}{extension}";

        var subFolder = request.SubFolder ?? DateTime.UtcNow.ToString("yyyy/MM");
        var directoryPath = Path.Combine(_settings.BasePath, subFolder);
        EnsureDirectoryExists(directoryPath);

        var filePath = Path.Combine(directoryPath, storedFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await request.Content.CopyToAsync(fileStream, cancellationToken);

        var url = $"{_settings.BaseUrl}/{subFolder}/{storedFileName}";

        _logger.LogInformation("File uploaded successfully: {FileId} -> {FilePath}", fileId, filePath);

        return new FileUploadResult(
            FileId: fileId,
            FileName: request.FileName,
            StoredFileName: storedFileName,
            ContentType: request.ContentType,
            FileSize: request.FileSize,
            Url: url,
            UploadedAt: DateTimeOffset.UtcNow
        );
    }

    public Task<FileDownloadResult?> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        if (filePath is null)
        {
            return Task.FromResult<FileDownloadResult?>(null);
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(filePath);
        var fileInfo = new FileInfo(filePath);
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);

        return Task.FromResult<FileDownloadResult?>(new FileDownloadResult(
            Content: stream,
            FileName: fileName,
            ContentType: contentType,
            FileSize: fileInfo.Length
        ));
    }

    public Task<string?> GetUrlAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        if (filePath is null)
        {
            return Task.FromResult<string?>(null);
        }

        var relativePath = Path.GetRelativePath(_settings.BasePath, filePath).Replace('\\', '/');
        var url = $"{_settings.BaseUrl}/{relativePath}";
        return Task.FromResult<string?>(url);
    }

    public Task<bool> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        if (filePath is null)
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);
        _logger.LogInformation("File deleted: {FileId} -> {FilePath}", fileId, filePath);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        return Task.FromResult(filePath is not null);
    }

    private string? FindFile(string fileId)
    {
        if (!Directory.Exists(_settings.BasePath))
        {
            return null;
        }

        var files = Directory.GetFiles(_settings.BasePath, $"{fileId}.*", SearchOption.AllDirectories);
        return files.Length > 0 ? files[0] : null;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
