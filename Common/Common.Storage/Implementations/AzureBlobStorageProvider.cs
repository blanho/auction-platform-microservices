using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Common.Storage.Models;
using Microsoft.Extensions.Options;

namespace Common.Storage.Implementations;


public class AzureBlobStorageProvider : IStorageProvider
{
    private readonly StorageOptions _options;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageProvider(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        
        if (string.IsNullOrEmpty(_options.AzureBlobConnectionString))
            throw new ArgumentException("Azure Blob connection string is required");
            
        if (string.IsNullOrEmpty(_options.AzureBlobContainer))
            throw new ArgumentException("Azure Blob container name is required");

        var blobServiceClient = new BlobServiceClient(_options.AzureBlobConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_options.AzureBlobContainer);
        _containerClient.CreateIfNotExists();
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
            var blobPath = $"{folder}/{uniqueFileName}";
            
            var blobClient = _containerClient.GetBlobClient(blobPath);
            
            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };
            
            stream.Position = 0;
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHeaders
            }, cancellationToken);

            var metadata = new FileMetadata
            {
                FileName = uniqueFileName,
                OriginalFileName = fileName,
                ContentType = contentType,
                Size = stream.Length,
                Path = blobPath,
                Url = blobClient.Uri.ToString(),
                Status = folder == _options.TempFolder ? FileStatus.Temporary : FileStatus.Permanent
            };

            return FileUploadResult.Ok(metadata);
        }
        catch (Exception ex)
        {
            return FileUploadResult.Fail($"Failed to upload file to Azure Blob: {ex.Message}");
        }
    }

    public async Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(path);
            
            if (!await blobClient.ExistsAsync(cancellationToken))
                return null;

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(path);
            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceClient = _containerClient.GetBlobClient(sourcePath);
            
            if (!await sourceClient.ExistsAsync(cancellationToken))
                return null;

            var fileName = Path.GetFileName(sourcePath);
            var destinationPath = $"{destinationFolder}/{fileName}";
            var destinationClient = _containerClient.GetBlobClient(destinationPath);
            
            await destinationClient.StartCopyFromUriAsync(sourceClient.Uri, cancellationToken: cancellationToken);
            
            var properties = await destinationClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            while (properties.Value.CopyStatus == CopyStatus.Pending)
            {
                await Task.Delay(100, cancellationToken);
                properties = await destinationClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            }
            
            await sourceClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            
            return destinationPath;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public string? GetUrl(string path)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        return blobClient.Uri.ToString();
    }
}
