using Grpc.Core;
using Storage.Api.Protos;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Grpc.Clients;

public sealed class FileStorageGrpcClient : IFileStorageGrpcClient
{
    private readonly StorageGrpc.StorageGrpcClient _client;
    private readonly ILogger<FileStorageGrpcClient> _logger;

    public FileStorageGrpcClient(
        StorageGrpc.StorageGrpcClient client,
        ILogger<FileStorageGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<TempUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var call = _client.Upload(cancellationToken: cancellationToken);

            await call.RequestStream.WriteAsync(new UploadRequest
            {
                FileInfo = new Storage.Api.Protos.FileInfo
                {
                    FileName = fileName,
                    ContentType = contentType,
                    UploadedBy = uploadedBy ?? string.Empty
                }
            });

            var buffer = new byte[64 * 1024];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await call.RequestStream.WriteAsync(new UploadRequest
                {
                    Chunk = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
                });
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;

            if (!response.Success)
            {
                _logger.LogWarning("File upload failed: {Error}", response.Error);
                return new TempUploadResult
                {
                    FileId = Guid.Empty,
                    Success = false,
                    Error = response.Error
                };
            }

            return new TempUploadResult
            {
                FileId = Guid.Parse(response.FileId),
                Success = true,
                Error = null
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during file upload");
            return new TempUploadResult
            {
                FileId = Guid.Empty,
                Success = false,
                Error = $"Upload failed: {ex.Status.Detail}"
            };
        }
    }

    public async Task<FileConfirmResult> ConfirmFileAsync(
        Guid fileId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmFileRequest
            {
                FileId = fileId.ToString(),
                EntityType = entityType,
                EntityId = entityId
            };

            var response = await _client.ConfirmFileAsync(request, cancellationToken: cancellationToken);

            return new FileConfirmResult
            {
                FileId = fileId,
                Success = response.Success,
                Error = response.Error,
                Url = response.Metadata?.Url
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error confirming file {FileId}", fileId);
            return new FileConfirmResult
            {
                FileId = fileId,
                Success = false,
                Error = $"Confirmation failed: {ex.Status.Detail}",
                Url = null
            };
        }
    }

    public async Task<List<FileConfirmResult>> ConfirmFilesBatchAsync(
        IEnumerable<(Guid FileId, string EntityType, string EntityId)> files,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmFilesRequest();
            foreach (var (fileId, entityType, entityId) in files)
            {
                request.Files.Add(new ConfirmFileRequest
                {
                    FileId = fileId.ToString(),
                    EntityType = entityType,
                    EntityId = entityId
                });
            }

            var response = await _client.ConfirmFilesAsync(request, cancellationToken: cancellationToken);

            return response.Files.Select(f => new FileConfirmResult
            {
                FileId = Guid.Parse(f.Id),
                Success = response.Success,
                Error = response.Errors.FirstOrDefault(),
                Url = f.Url
            }).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error confirming files batch");
            return files.Select(f => new FileConfirmResult
            {
                FileId = f.FileId,
                Success = false,
                Error = $"Batch confirmation failed: {ex.Status.Detail}",
                Url = null
            }).ToList();
        }
    }

    public async Task<FileConfirmResult> UploadAndConfirmAsync(
        Stream stream,
        string fileName,
        string contentType,
        string entityType,
        string entityId,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {

        var uploadResult = await UploadToTempAsync(stream, fileName, contentType, uploadedBy, cancellationToken);

        if (!uploadResult.Success)
        {
            return new FileConfirmResult
            {
                FileId = uploadResult.FileId,
                Success = false,
                Error = uploadResult.Error,
                Url = null
            };
        }

        return await ConfirmFileAsync(uploadResult.FileId, entityType, entityId, cancellationToken);
    }

    public async Task<FileMetadata?> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetMetadataRequest
            {
                FileId = fileId.ToString()
            };

            var response = await _client.GetMetadataAsync(request, cancellationToken: cancellationToken);

            if (string.IsNullOrEmpty(response.Id))
            {
                return null;
            }

            return new FileMetadata
            {
                FileId = Guid.Parse(response.Id),
                FileName = response.FileName,
                ContentType = response.ContentType,
                Size = response.Size,
                Url = response.Url,
                ThumbnailUrl = null,
                CreatedAt = DateTimeOffset.Parse(response.CreatedAt)
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting metadata for file {FileId}", fileId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteFileRequest
            {
                FileId = fileId.ToString()
            };

            var response = await _client.DeleteFileAsync(request, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error deleting file {FileId}", fileId);
            return false;
        }
    }
}
