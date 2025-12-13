using Common.Storage.Abstractions;
using Common.Storage.Models;
using Grpc.Core;
using UtilityService.Grpc;

namespace UtilityService.GrpcServices;

public class FileStorageGrpcService : FileStorageGrpc.FileStorageGrpcBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileStorageGrpcService> _logger;

    public FileStorageGrpcService(
        IFileStorageService fileStorageService,
        ILogger<FileStorageGrpcService> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public override async Task<UploadFileResponse> UploadFile(
        IAsyncStreamReader<FileChunk> requestStream,
        ServerCallContext context)
    {
        FileMetadataRequest? metadata = null;
        var tempFile = Path.GetTempFileName();
        long totalSize = 0;

        try
        {
            await using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            await foreach (var chunk in requestStream.ReadAllAsync(context.CancellationToken))
            {
                if (chunk.DataCase == FileChunk.DataOneofCase.Metadata)
                {
                    metadata = chunk.Metadata;
                    _logger.LogInformation("Receiving file: {FileName}, ContentType: {ContentType}", 
                        metadata.FileName, metadata.ContentType);
                }
                else if (chunk.DataCase == FileChunk.DataOneofCase.Content)
                {
                    var bytes = chunk.Content.ToByteArray();
                    await fileStream.WriteAsync(bytes, context.CancellationToken);
                    totalSize += bytes.Length;
                }
            }

            if (metadata == null)
            {
                return new UploadFileResponse
                {
                    Success = false,
                    Error = "No metadata received"
                };
            }

            // Now upload to storage service using the temp file
            await using var uploadStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var result = await _fileStorageService.UploadToTempAsync(
                uploadStream,
                metadata.FileName,
                metadata.ContentType,
                metadata.UploadedBy,
                context.CancellationToken);

            if (!result.Success || result.Metadata == null)
            {
                return new UploadFileResponse
                {
                    Success = false,
                    Error = result.Error ?? "Upload failed"
                };
            }

            _logger.LogInformation("File uploaded successfully: {FileId}, Size: {Size} bytes", 
                result.Metadata.Id, result.Metadata.Size);

            return new UploadFileResponse
            {
                Success = true,
                FileId = result.Metadata.Id.ToString(),
                FileName = result.Metadata.FileName,
                Size = result.Metadata.Size,
                TempPath = result.Metadata.Path
            };
        }
        finally
        {
            // Cleanup temp file
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }

    public override async Task<ConfirmFileResponse> ConfirmFile(
        Grpc.ConfirmFileRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            return new ConfirmFileResponse
            {
                Success = false,
                Error = "Invalid file ID format"
            };
        }

        var confirmRequest = new FileConfirmRequest
        {
            FileId = fileId,
            EntityType = request.EntityType,
            EntityId = request.EntityId
        };

        var result = await _fileStorageService.ConfirmFileAsync(confirmRequest, context.CancellationToken);

        if (!result.Success || result.Metadata == null)
        {
            return new ConfirmFileResponse
            {
                Success = false,
                FileId = request.FileId,
                Error = result.Error ?? "Confirm failed"
            };
        }

        _logger.LogInformation("File confirmed: {FileId} -> {EntityType}/{EntityId}", 
            fileId, request.EntityType, request.EntityId);

        return new ConfirmFileResponse
        {
            Success = true,
            FileId = result.Metadata.Id.ToString(),
            FileName = result.Metadata.FileName,
            Url = result.Metadata.Url ?? "",
            Path = result.Metadata.Path,
            Size = result.Metadata.Size
        };
    }

    public override async Task<ConfirmFilesBatchResponse> ConfirmFilesBatch(
        ConfirmFilesBatchRequest request,
        ServerCallContext context)
    {
        var response = new ConfirmFilesBatchResponse { Success = true };

        // Process all files in parallel
        var tasks = request.Files.Select(async fileRequest =>
        {
            if (!Guid.TryParse(fileRequest.FileId, out var fileId))
            {
                return new ConfirmFileResponse
                {
                    Success = false,
                    FileId = fileRequest.FileId,
                    Error = "Invalid file ID format"
                };
            }

            var confirmRequest = new FileConfirmRequest
            {
                FileId = fileId,
                EntityType = fileRequest.EntityType,
                EntityId = fileRequest.EntityId
            };

            var result = await _fileStorageService.ConfirmFileAsync(confirmRequest, context.CancellationToken);

            if (!result.Success || result.Metadata == null)
            {
                return new ConfirmFileResponse
                {
                    Success = false,
                    FileId = fileRequest.FileId,
                    Error = result.Error ?? "Confirm failed"
                };
            }

            return new ConfirmFileResponse
            {
                Success = true,
                FileId = result.Metadata.Id.ToString(),
                FileName = result.Metadata.FileName,
                Url = result.Metadata.Url ?? "",
                Path = result.Metadata.Path,
                Size = result.Metadata.Size
            };
        });

        var results = await Task.WhenAll(tasks);
        response.Results.AddRange(results);

        // Set overall success based on individual results
        response.Success = results.All(r => r.Success);

        _logger.LogInformation("Batch confirmed {Count} files, Success: {Success}", 
            results.Length, response.Success);

        return response;
    }

    public override async Task<ConfirmFileResponse> UploadAndConfirm(
        IAsyncStreamReader<FileChunk> requestStream,
        ServerCallContext context)
    {
        // First upload
        var uploadResponse = await UploadFileInternal(requestStream, context);
        
        if (!uploadResponse.Success || uploadResponse.Metadata == null)
        {
            return new ConfirmFileResponse
            {
                Success = false,
                Error = uploadResponse.Error ?? "Upload failed"
            };
        }

        // Then confirm
        var confirmRequest = new FileConfirmRequest
        {
            FileId = uploadResponse.FileId,
            EntityType = uploadResponse.EntityType ?? "",
            EntityId = uploadResponse.EntityId ?? ""
        };

        var result = await _fileStorageService.ConfirmFileAsync(confirmRequest, context.CancellationToken);

        if (!result.Success || result.Metadata == null)
        {
            return new ConfirmFileResponse
            {
                Success = false,
                FileId = uploadResponse.FileId.ToString(),
                Error = result.Error ?? "Confirm failed"
            };
        }

        return new ConfirmFileResponse
        {
            Success = true,
            FileId = result.Metadata.Id.ToString(),
            FileName = result.Metadata.FileName,
            Url = result.Metadata.Url ?? "",
            Path = result.Metadata.Path,
            Size = result.Metadata.Size
        };
    }

    public override async Task<FileMetadataResponse> GetFileMetadata(
        GetFileRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            return new FileMetadataResponse
            {
                Success = false,
                Error = "Invalid file ID format"
            };
        }

        var metadata = await _fileStorageService.GetMetadataAsync(fileId, context.CancellationToken);

        if (metadata == null)
        {
            return new FileMetadataResponse
            {
                Success = false,
                Error = "File not found"
            };
        }

        return new FileMetadataResponse
        {
            Success = true,
            FileId = metadata.Id.ToString(),
            FileName = metadata.FileName,
            ContentType = metadata.ContentType,
            Size = metadata.Size,
            Url = metadata.Url ?? "",
            Path = metadata.Path,
            Status = (int)metadata.Status,
            EntityType = metadata.EntityType ?? "",
            EntityId = metadata.EntityId ?? ""
        };
    }

    public override async Task<DeleteFileResponse> DeleteFile(
        DeleteFileRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            return new DeleteFileResponse
            {
                Success = false,
                Error = "Invalid file ID format"
            };
        }

        var result = await _fileStorageService.DeleteAsync(fileId, context.CancellationToken);

        return new DeleteFileResponse
        {
            Success = result,
            Error = result ? "" : "Delete failed"
        };
    }

    // Internal helper for upload with metadata extraction
    private record UploadResult(bool Success, Guid FileId, string? Error, FileMetadata? Metadata, string? EntityType, string? EntityId);

    private async Task<UploadResult> UploadFileInternal(
        IAsyncStreamReader<FileChunk> requestStream,
        ServerCallContext context)
    {
        FileMetadataRequest? metadata = null;
        var tempFile = Path.GetTempFileName();

        try
        {
            await using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            await foreach (var chunk in requestStream.ReadAllAsync(context.CancellationToken))
            {
                if (chunk.DataCase == FileChunk.DataOneofCase.Metadata)
                {
                    metadata = chunk.Metadata;
                }
                else if (chunk.DataCase == FileChunk.DataOneofCase.Content)
                {
                    await fileStream.WriteAsync(chunk.Content.ToByteArray(), context.CancellationToken);
                }
            }

            if (metadata == null)
            {
                return new UploadResult(false, Guid.Empty, "No metadata received", null, null, null);
            }

            await using var uploadStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var result = await _fileStorageService.UploadToTempAsync(
                uploadStream,
                metadata.FileName,
                metadata.ContentType,
                metadata.UploadedBy,
                context.CancellationToken);

            if (!result.Success || result.Metadata == null)
            {
                return new UploadResult(false, Guid.Empty, result.Error, null, null, null);
            }

            return new UploadResult(true, result.Metadata.Id, null, result.Metadata, metadata.EntityType, metadata.EntityId);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }
}
