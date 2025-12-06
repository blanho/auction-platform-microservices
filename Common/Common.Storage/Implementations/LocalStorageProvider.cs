using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Common.Storage.Models;
using Microsoft.Extensions.Options;

namespace Common.Storage.Implementations;

public class LocalStorageProvider : IStorageProvider
{
    private readonly StorageOptions _options;

    public LocalStorageProvider(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        var tempPath = Path.Combine(_options.LocalBasePath, _options.TempFolder);
        var permanentPath = Path.Combine(_options.LocalBasePath, _options.PermanentFolder);
        
        Directory.CreateDirectory(tempPath);
        Directory.CreateDirectory(permanentPath);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var relativePath = Path.Combine(folder, uniqueFileName);
            var fullPath = Path.Combine(_options.LocalBasePath, relativePath);
            
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await stream.CopyToAsync(fileStream, cancellationToken);

            var metadata = new FileMetadata
            {
                FileName = uniqueFileName,
                OriginalFileName = fileName,
                ContentType = contentType,
                Size = stream.Length,
                Path = relativePath,
                Url = GetUrl(relativePath),
                Status = folder == _options.TempFolder ? FileStatus.Temporary : FileStatus.Permanent
            };

            return FileUploadResult.Ok(metadata);
        }
        catch (Exception ex)
        {
            return FileUploadResult.Fail($"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_options.LocalBasePath, path);
        
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
        var fullPath = Path.Combine(_options.LocalBasePath, path);
        
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(_options.LocalBasePath, sourcePath);
        
        if (!File.Exists(sourceFullPath))
            return Task.FromResult<string?>(null);

        var fileName = Path.GetFileName(sourcePath);
        var destinationRelativePath = Path.Combine(destinationFolder, fileName);
        var destinationFullPath = Path.Combine(_options.LocalBasePath, destinationRelativePath);
        
        var directory = Path.GetDirectoryName(destinationFullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Move(sourceFullPath, destinationFullPath, overwrite: true);
        
        return Task.FromResult<string?>(destinationRelativePath);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_options.LocalBasePath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string? GetUrl(string path)
    {
        if (string.IsNullOrEmpty(_options.BaseUrl))
            return null;
            
        return $"{_options.BaseUrl.TrimEnd('/')}/files/{path.Replace("\\", "/")}";
    }
}
