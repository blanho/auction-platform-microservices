using Common.Storage.Grpc;
using Common.Storage.Enums;
using Common.Storage.Models;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Storage.Clients;

public class GrpcFileStorageOptions
{
    public string ServiceUrl { get; set; } = "https://localhost:5003";
    public int ChunkSize { get; set; } = 64 * 1024; // 64KB chunks
}

public interface IFileStorageGrpcClient
{
    Task<TempUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);

    Task<FileConfirmResult> ConfirmFileAsync(
        Guid fileId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<List<FileConfirmResult>> ConfirmFilesBatchAsync(
        IEnumerable<(Guid FileId, string EntityType, string EntityId)> files,
        CancellationToken cancellationToken = default);


    Task<FileConfirmResult> UploadAndConfirmAsync(
        Stream stream,
        string fileName,
        string contentType,
        string entityType,
        string entityId,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);


    Task<FileMetadata?> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);


    Task<bool> DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
}

public class FileStorageGrpcClient : IFileStorageGrpcClient, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly FileStorageGrpc.FileStorageGrpcClient _client;
    private readonly GrpcFileStorageOptions _options;
    private readonly ILogger<FileStorageGrpcClient> _logger;
    private bool _disposed;

    public FileStorageGrpcClient(
        IOptions<GrpcFileStorageOptions> options,
        ILogger<FileStorageGrpcClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        _channel = GrpcChannel.ForAddress(_options.ServiceUrl);
        _client = new FileStorageGrpc.FileStorageGrpcClient(_channel);
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
            using var call = _client.UploadFile(cancellationToken: cancellationToken);

            // Send metadata first
            await call.RequestStream.WriteAsync(new FileChunk
            {
                Metadata = new FileMetadataRequest
                {
                    FileName = fileName,
                    ContentType = contentType,
                    UploadedBy = uploadedBy ?? "",
                    AutoConfirm = false
                }
            }, cancellationToken);

            var buffer = new byte[_options.ChunkSize];
            int bytesRead;
            long totalBytes = 0;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await call.RequestStream.WriteAsync(new FileChunk
                {
                    Content = ByteString.CopyFrom(buffer, 0, bytesRead)
                }, cancellationToken);
                totalBytes += bytesRead;
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;

            if (!response.Success)
            {
                _logger.LogWarning("Failed to upload file {FileName}: {Error}", fileName, response.Error);
                return new TempUploadResult { Success = false, Error = response.Error };
            }

            _logger.LogInformation("File uploaded via gRPC: {FileId}, Size: {Size} bytes", 
                response.FileId, response.Size);

            return new TempUploadResult
            {
                Success = true,
                FileId = Guid.Parse(response.FileId),
                FileName = response.FileName,
                Size = response.Size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC upload failed for file {FileName}", fileName);
            return new TempUploadResult { Success = false, Error = ex.Message };
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
            var response = await _client.ConfirmFileAsync(new Grpc.ConfirmFileRequest
            {
                FileId = fileId.ToString(),
                EntityType = entityType,
                EntityId = entityId
            }, cancellationToken: cancellationToken);

            if (!response.Success)
            {
                return new FileConfirmResult { Success = false, FileId = fileId, Error = response.Error };
            }

            return new FileConfirmResult
            {
                Success = true,
                FileId = Guid.Parse(response.FileId),
                FileName = response.FileName,
                Url = response.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC confirm failed for file {FileId}", fileId);
            return new FileConfirmResult { Success = false, FileId = fileId, Error = ex.Message };
        }
    }

    public async Task<List<FileConfirmResult>> ConfirmFilesBatchAsync(
        IEnumerable<(Guid FileId, string EntityType, string EntityId)> files,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmFilesBatchRequest();
            foreach (var (fileId, entityType, entityId) in files)
            {
                request.Files.Add(new Grpc.ConfirmFileRequest
                {
                    FileId = fileId.ToString(),
                    EntityType = entityType,
                    EntityId = entityId
                });
            }

            var response = await _client.ConfirmFilesBatchAsync(request, cancellationToken: cancellationToken);

            return response.Results.Select(r => new FileConfirmResult
            {
                Success = r.Success,
                FileId = Guid.TryParse(r.FileId, out var id) ? id : Guid.Empty,
                FileName = r.FileName,
                Url = r.Url,
                Error = r.Error
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC batch confirm failed");
            return new List<FileConfirmResult> { new() { Success = false, Error = ex.Message } };
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
        try
        {
            using var call = _client.UploadAndConfirm(cancellationToken: cancellationToken);

            // Send metadata first with entity info
            await call.RequestStream.WriteAsync(new FileChunk
            {
                Metadata = new FileMetadataRequest
                {
                    FileName = fileName,
                    ContentType = contentType,
                    EntityType = entityType,
                    EntityId = entityId,
                    UploadedBy = uploadedBy ?? "",
                    AutoConfirm = true
                }
            }, cancellationToken);

            // Stream file content
            var buffer = new byte[_options.ChunkSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await call.RequestStream.WriteAsync(new FileChunk
                {
                    Content = ByteString.CopyFrom(buffer, 0, bytesRead)
                }, cancellationToken);
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;

            if (!response.Success)
            {
                return new FileConfirmResult { Success = false, Error = response.Error };
            }

            _logger.LogInformation("File uploaded and confirmed via gRPC: {FileId}", response.FileId);

            return new FileConfirmResult
            {
                Success = true,
                FileId = Guid.Parse(response.FileId),
                FileName = response.FileName,
                Url = response.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC upload and confirm failed for file {FileName}", fileName);
            return new FileConfirmResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<FileMetadata?> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetFileMetadataAsync(new GetFileRequest
            {
                FileId = fileId.ToString()
            }, cancellationToken: cancellationToken);

            if (!response.Success)
            {
                return null;
            }

            return new FileMetadata
            {
                Id = Guid.Parse(response.FileId),
                FileName = response.FileName,
                ContentType = response.ContentType,
                Size = response.Size,
                Url = response.Url,
                Path = response.Path,
                Status = (FileStatus)response.Status,
                EntityType = response.EntityType,
                EntityId = response.EntityId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC get metadata failed for file {FileId}", fileId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteFileAsync(new DeleteFileRequest
            {
                FileId = fileId.ToString()
            }, cancellationToken: cancellationToken);

            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC delete failed for file {FileId}", fileId);
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel.Dispose();
            _disposed = true;
        }
    }
}
