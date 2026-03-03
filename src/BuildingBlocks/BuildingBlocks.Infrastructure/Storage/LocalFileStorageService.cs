#nullable enable
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace BuildingBlocks.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalStorageSettings _settings;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        RecyclableMemoryStreamManager streamManager,
        ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value.Local;
        _streamManager = streamManager;
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

    public async Task<FileDownloadResult?> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        if (filePath is null)
        {
            return null;
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(filePath);
        var fileInfo = new FileInfo(filePath);

        var memoryStream = _streamManager.GetStream(tag: fileId);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return new FileDownloadResult(
            Content: memoryStream,
            FileName: fileName,
            ContentType: contentType,
            FileSize: fileInfo.Length
        );
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

    public Task<PresignedUploadResult?> GenerateUploadSasTokenAsync(
        PresignedUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SAS token generation is not supported for local storage. Use the standard upload endpoint.");
        return Task.FromResult<PresignedUploadResult?>(null);
    }

    public Task<PresignedDownloadResult?> GenerateDownloadSasTokenAsync(
        string fileId,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(fileId);
        if (filePath is null)
        {
            return Task.FromResult<PresignedDownloadResult?>(null);
        }

        var relativePath = Path.GetRelativePath(_settings.BasePath, filePath).Replace('\\', '/');
        var downloadUrl = $"{_settings.BaseUrl}/{relativePath}";
        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(filePath);

        return Task.FromResult<PresignedDownloadResult?>(new PresignedDownloadResult(
            DownloadUrl: downloadUrl,
            FileName: fileName,
            ContentType: contentType,
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1)
        ));
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
