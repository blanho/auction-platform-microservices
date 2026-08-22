#nullable enable
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace BuildingBlocks.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalStorageSettings _settings;
    private readonly string _basePath;
    private readonly string _basePathPrefix;
    private readonly string _baseUrl;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static StringComparison PathComparison => OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        RecyclableMemoryStreamManager streamManager,
        ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value.Local;
        _streamManager = streamManager;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.BasePath))
        {
            throw new InvalidOperationException("The local file storage base path must be configured.");
        }

        _basePath = Path.GetFullPath(_settings.BasePath);
        _basePathPrefix = Path.EndsInDirectorySeparator(_basePath)
            ? _basePath
            : _basePath + Path.DirectorySeparatorChar;
        _baseUrl = _settings.BaseUrl.Replace('\\', '/').TrimEnd('/');

        EnsureDirectoryExists(_basePath);
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fileId = Guid.NewGuid().ToString("N");
        var normalizedFileName = request.FileName.Replace('\\', '/');
        var extension = Path.GetExtension(Path.GetFileName(normalizedFileName)).ToLowerInvariant();
        var storedFileName = $"{fileId}{extension}";

        var subFolder = request.SubFolder ?? DateTime.UtcNow.ToString("yyyy/MM");
        if (!TryResolvePath(subFolder, out var directoryPath, out var normalizedSubFolder))
        {
            throw new ArgumentException(
                "The upload subfolder must be a relative path within the configured storage root.",
                nameof(request.SubFolder));
        }

        EnsureDirectoryExists(directoryPath);

        var filePath = Path.Combine(directoryPath, storedFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await request.Content.CopyToAsync(fileStream, cancellationToken);

        var storedPath = $"{normalizedSubFolder}/{storedFileName}";
        var url = BuildUrl(storedPath);

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
        if (!TryResolvePath(storedFileName, out var filePath, out var normalizedStoredFileName)
            || !File.Exists(filePath))
        {
            return Task.FromResult<string?>(null);
        }

        var url = BuildUrl(normalizedStoredFileName);
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
        if (!TryResolvePath(storedFileName, out var filePath, out var normalizedStoredFileName)
            || !File.Exists(filePath))
        {
            return Task.FromResult<PresignedDownloadResult?>(null);
        }

        var downloadUrl = BuildUrl(normalizedStoredFileName);
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
        return TryResolvePath(storedFileName, out var fullPath, out _) ? fullPath : null;
    }

    private bool TryResolvePath(string relativePath, out string fullPath, out string normalizedRelativePath)
    {
        fullPath = string.Empty;
        normalizedRelativePath = string.Empty;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        string decodedPath;
        try
        {
            decodedPath = Uri.UnescapeDataString(relativePath);
        }
        catch (UriFormatException)
        {
            return false;
        }

        var urlNormalizedPath = decodedPath.Replace('\\', '/');
        if (urlNormalizedPath.IndexOf('\0') >= 0
            || urlNormalizedPath.StartsWith('/')
            || Path.IsPathRooted(urlNormalizedPath)
            || LooksLikeWindowsDrivePath(urlNormalizedPath))
        {
            return false;
        }

        var segments = urlNormalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            return false;
        }

        normalizedRelativePath = string.Join('/', segments);

        try
        {
            var nativeRelativePath = Path.Combine(segments);
            fullPath = Path.GetFullPath(Path.Combine(_basePath, nativeRelativePath));
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException)
        {
            fullPath = string.Empty;
            normalizedRelativePath = string.Empty;
            return false;
        }

        if (!fullPath.StartsWith(_basePathPrefix, PathComparison) || ContainsReparsePoint(fullPath))
        {
            fullPath = string.Empty;
            normalizedRelativePath = string.Empty;
            return false;
        }

        return true;
    }

    private bool ContainsReparsePoint(string fullPath)
    {
        var relativePath = Path.GetRelativePath(_basePath, fullPath);
        var segments = relativePath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        var currentPath = _basePath;

        foreach (var segment in segments)
        {
            currentPath = Path.Combine(currentPath, segment);
            if (!Directory.Exists(currentPath) && !File.Exists(currentPath))
            {
                break;
            }

            try
            {
                if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
                {
                    return true;
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildUrl(string normalizedRelativePath)
    {
        var escapedPath = string.Join(
            '/',
            normalizedRelativePath.Split('/').Select(Uri.EscapeDataString));

        return string.IsNullOrEmpty(_baseUrl)
            ? $"/{escapedPath}"
            : $"{_baseUrl}/{escapedPath}";
    }

    private static bool LooksLikeWindowsDrivePath(string path)
    {
        return path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':';
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
