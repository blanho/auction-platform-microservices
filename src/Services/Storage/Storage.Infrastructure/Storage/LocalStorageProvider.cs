using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Web.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Storage.Application.Configuration;
using Storage.Application.DTOs;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;

namespace Storage.Infrastructure.Storage;

public class LocalStorageProvider : IStorageProvider
{
    private readonly StorageOptions _options;
    private readonly string _basePath;
    private readonly string _tokenSigningKey;

    public LocalStorageProvider(
        IOptions<StorageOptions> options, 
        IConfiguration configuration,
        string basePath = "uploads")
    {
        _options = options.Value;
        _basePath = basePath;
        
        _tokenSigningKey = configuration["Storage:TokenSigningKey"]
            ?? throw new ConfigurationException(
                "Storage:TokenSigningKey must be configured. " +
                "Generate a secure random string of at least 32 characters.");
        
        if (_tokenSigningKey.Length < 32)
        {
            throw new ConfigurationException(
                "Storage:TokenSigningKey must be at least 32 characters long for security.");
        }
        
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
        int expirationMinutes = StorageDefaults.DefaultUrlExpirationMinutes,
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
        int expirationMinutes = StorageDefaults.DefaultUrlExpirationMinutes,
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

    private string GenerateDownloadToken(string path, int expirationMinutes)
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();
        var data = $"{path}:{expiry}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tokenSigningKey));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var signatureBase64 = Convert.ToBase64String(signature)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        
        return $"{expiry}.{signatureBase64}";
    }
    
    public bool ValidateDownloadToken(string path, string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2)
                return false;
            
            if (!long.TryParse(parts[0], out var expiry))
                return false;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
                return false;
            var data = $"{path}:{expiry}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tokenSigningKey));
            var expectedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var expectedSignatureBase64 = Convert.ToBase64String(expectedSignature)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parts[1]),
                Encoding.UTF8.GetBytes(expectedSignatureBase64));
        }
        catch
        {
            return false;
        }
    }
}
