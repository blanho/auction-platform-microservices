using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using StorageService.Application.Configuration;
using StorageService.Application.Interfaces;

namespace StorageService.Infrastructure.Storage;

public class LocalStorageProvider : IStorageProvider
{
    private readonly StorageOptions _options;
    private readonly string _basePath;

    public LocalStorageProvider(IOptions<StorageOptions> options, string basePath = "uploads")
    {
        _options = options.Value;
        _basePath = basePath;
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        var tempPath = Path.Combine(_basePath, _options.TempFolder);
        var permanentPath = Path.Combine(_basePath, _options.PermanentFolder);
        
        Directory.CreateDirectory(tempPath);
        Directory.CreateDirectory(permanentPath);
    }

    public async Task<StorageUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var relativePath = Path.Combine(folder, fileName);
            var fullPath = Path.Combine(_basePath, relativePath);
            
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            stream.Position = 0;
            string checksum;
            
            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }
            
            checksum = await ComputeChecksumAsync(relativePath, cancellationToken) ?? string.Empty;

            return new StorageUploadResult(true, relativePath, GetStoragePath(folder, fileName), checksum);
        }
        catch (Exception ex)
        {
            return new StorageUploadResult(false, null, null, null, $"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        
        if (!File.Exists(fullPath))
            return null;

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(_basePath, sourcePath);
        
        if (!File.Exists(sourceFullPath))
            return Task.FromResult<string?>(null);

        var fileName = Path.GetFileName(sourcePath);
        var destinationRelativePath = Path.Combine(destinationFolder, fileName);
        var destinationFullPath = Path.Combine(_basePath, destinationRelativePath);
        
        var destinationDir = Path.GetDirectoryName(destinationFullPath);
        if (!string.IsNullOrEmpty(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        File.Move(sourceFullPath, destinationFullPath, overwrite: true);
        
        return Task.FromResult<string?>(destinationRelativePath);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<PreSignedUrlResult> GenerateUploadUrlAsync(
        string fileName,
        string contentType,
        string folder,
        int expirationMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl ?? "http://localhost:5011";
        var url = $"{baseUrl}/api/files/direct-upload?folder={folder}&fileName={fileName}";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        
        return Task.FromResult(new PreSignedUrlResult(
            true,
            url,
            expiresAt,
            new Dictionary<string, string> { ["Content-Type"] = contentType }
        ));
    }

    public Task<PreSignedUrlResult> GenerateDownloadUrlAsync(
        string path,
        int expirationMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl ?? "http://localhost:5011";
        var token = GenerateDownloadToken(path, expirationMinutes);
        var url = $"{baseUrl}/api/files/download?path={Uri.EscapeDataString(path)}&token={token}";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        
        return Task.FromResult(new PreSignedUrlResult(true, url, expiresAt));
    }

    public async Task<string?> ComputeChecksumAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        
        if (!File.Exists(fullPath))
            return null;

        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToBase64String(hash);
    }

    public string GetStoragePath(string folder, string fileName)
    {
        return Path.Combine(folder, fileName);
    }

    private static string GenerateDownloadToken(string path, int expirationMinutes)
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();
        var data = $"{path}:{expiry}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return $"{expiry}.{Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
    }
}
