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

        var storedPath = $"{subFolder}/{storedFileName}";

        _logger.LogInformation("File uploaded successfully: {FileId} -> {FilePath}", fileId, filePath);

        return new FileUploadResult(
            FileId: fileId,
            FileName: request.FileName,
            StoredFileName: storedPath,
            ContentType: request.ContentType,
            FileSize: request.FileSize,
            Url: url,
            UploadedAt: DateTimeOffset.UtcNow
        );
    }

    public async Task<FileDownloadResult?> DownloadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(storedFileName);
        if (filePath is null || !File.Exists(filePath))
        {
            return null;
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(filePath);
        var fileInfo = new FileInfo(filePath);

        var memoryStream = _streamManager.GetStream(tag: storedFileName);
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

    public Task<string?> GetUrlAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(storedFileName);
        if (filePath is null || !File.Exists(filePath))
        {
            return Task.FromResult<string?>(null);
        }

        var url = $"{_settings.BaseUrl}/{storedFileName}";
        return Task.FromResult<string?>(url);
    }

    public Task<bool> DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(storedFileName);
        if (filePath is null || !File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);
        _logger.LogInformation("File deleted: {StoredFileName} -> {FilePath}", storedFileName, filePath);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(storedFileName);
        return Task.FromResult(filePath is not null && File.Exists(filePath));
    }

    public Task<PresignedUploadResult?> GenerateUploadSasTokenAsync(
        PresignedUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SAS token generation is not supported for local storage. Use the standard upload endpoint.");
        return Task.FromResult<PresignedUploadResult?>(null);
    }

    public Task<PresignedDownloadResult?> GenerateDownloadSasTokenAsync(
        string storedFileName,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(storedFileName);
        if (filePath is null || !File.Exists(filePath))
        {
            return Task.FromResult<PresignedDownloadResult?>(null);
        }

        var downloadUrl = $"{_settings.BaseUrl}/{storedFileName}";
        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(filePath);

        return Task.FromResult<PresignedDownloadResult?>(new PresignedDownloadResult(
            DownloadUrl: downloadUrl,
            FileName: fileName,
            ContentType: contentType,
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1)
        ));
    }

    private string? ResolveFilePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return null;
        }

        var fullPath = Path.Combine(_settings.BasePath, storedFileName.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(_settings.BasePath))
            ? fullPath
            : null;
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
