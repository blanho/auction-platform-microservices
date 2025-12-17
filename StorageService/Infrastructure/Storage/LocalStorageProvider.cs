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

            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            stream.Position = 0;
            await stream.CopyToAsync(fileStream, cancellationToken);

            return new StorageUploadResult(true, relativePath, GetUrl(relativePath));
        }
        catch (Exception ex)
        {
            return new StorageUploadResult(false, null, null, $"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
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

    public Task<string> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(_basePath, sourcePath);
        
        if (!File.Exists(sourceFullPath))
            return Task.FromResult<string>(null);

        var fileName = Path.GetFileName(sourcePath);
        var destinationRelativePath = Path.Combine(destinationFolder, fileName);
        var destinationFullPath = Path.Combine(_basePath, destinationRelativePath);
        
        var destinationDir = Path.GetDirectoryName(destinationFullPath);
        if (!string.IsNullOrEmpty(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        File.Move(sourceFullPath, destinationFullPath, overwrite: true);
        
        return Task.FromResult(destinationRelativePath);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetUrl(string path)
    {
        return $"/files/{path.Replace("\\", "/")}";
    }
}
