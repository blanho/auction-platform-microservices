using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Common.Storage.Models;
using Microsoft.Extensions.Options;

namespace Common.Storage.Implementations;

public class CloudinaryStorageProvider : IStorageProvider
{
    private readonly Cloudinary _cloudinary;
    private readonly StorageOptions _options;

    public CloudinaryStorageProvider(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        
        if (_options.Cloudinary == null)
            throw new ArgumentException("Cloudinary configuration is required");

        var account = new Account(
            _options.Cloudinary.CloudName,
            _options.Cloudinary.ApiKey,
            _options.Cloudinary.ApiSecret
        );
        
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = _options.Cloudinary.UseSecureUrl;
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
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}";
            var cloudinaryFolder = $"auction/{folder}";
            
            var isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            
            string? publicId;
            string? secureUrl;
            string? url;
            long bytes;
            CloudinaryDotNet.Actions.Error? error;
            
            if (isImage)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = $"{cloudinaryFolder}/{uniqueFileName}",
                    Folder = cloudinaryFolder,
                    UseFilename = false,
                    UniqueFilename = false,
                    Overwrite = false
                };
                
                var imageResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
                publicId = imageResult.PublicId;
                secureUrl = imageResult.SecureUrl?.ToString();
                url = imageResult.Url?.ToString();
                bytes = imageResult.Bytes;
                error = imageResult.Error;
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = $"{cloudinaryFolder}/{uniqueFileName}",
                    Folder = cloudinaryFolder,
                    UseFilename = false,
                    UniqueFilename = false,
                    Overwrite = false
                };
                
                var rawResult = await _cloudinary.UploadAsync(uploadParams);
                publicId = rawResult.PublicId;
                secureUrl = rawResult.SecureUrl?.ToString();
                url = rawResult.Url?.ToString();
                bytes = rawResult.Bytes;
                error = rawResult.Error;
            }

            if (error != null)
            {
                return FileUploadResult.Fail($"Cloudinary upload failed: {error.Message}");
            }

            var metadata = new FileMetadata
            {
                FileName = uniqueFileName,
                OriginalFileName = fileName,
                ContentType = contentType,
                Size = bytes,
                Path = publicId ?? string.Empty,
                Url = secureUrl ?? url,
                Status = folder == _options.TempFolder ? FileStatus.Temporary : FileStatus.Permanent
            };

            return FileUploadResult.Ok(metadata);
        }
        catch (Exception ex)
        {
            return FileUploadResult.Fail($"Failed to upload file to Cloudinary: {ex.Message}");
        }
    }

    public async Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = GetUrl(path);
            if (string.IsNullOrEmpty(url))
                return null;

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream, cancellationToken);
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
            var deletionParams = new DeletionParams(path)
            {
                Invalidate = true
            };
            
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
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
            var fileName = Path.GetFileName(sourcePath);
            var newPublicId = $"auction/{destinationFolder}/{fileName}";
            
            var renameParams = new RenameParams(sourcePath, newPublicId)
            {
                Overwrite = false,
                Invalidate = true
            };
            
            var result = await _cloudinary.RenameAsync(renameParams);
            
            if (result.Error != null)
                return null;
                
            return result.PublicId;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var getResourceParams = new GetResourceParams(path);
            var result = await _cloudinary.GetResourceAsync(getResourceParams);
            
            return result != null && result.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    public string? GetUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        try
        {
            var url = _cloudinary.Api.UrlImgUp
                .Secure(_options.Cloudinary?.UseSecureUrl ?? true)
                .BuildUrl(path);
                
            return url;
        }
        catch
        {
            return null;
        }
    }
}
