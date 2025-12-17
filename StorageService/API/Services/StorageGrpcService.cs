using Grpc.Core;
using StorageService.API.Protos;
using StorageService.Application.DTOs;
using StorageService.Application.Interfaces;

namespace StorageService.API.Services;

public class StorageGrpcService : StorageGrpc.StorageGrpcBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<StorageGrpcService> _logger;

    public StorageGrpcService(IFileStorageService fileStorageService, ILogger<StorageGrpcService> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public override async Task<UploadResponse> Upload(
        IAsyncStreamReader<UploadRequest> requestStream,
        ServerCallContext context)
    {
        Protos.FileInfo fileInfo = null;
        var memoryStream = new MemoryStream();

        await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            if (request.DataCase == UploadRequest.DataOneofCase.FileInfo)
            {
                fileInfo = request.FileInfo;
            }
            else if (request.DataCase == UploadRequest.DataOneofCase.Chunk)
            {
                await memoryStream.WriteAsync(request.Chunk.Memory, context.CancellationToken);
            }
        }

        if (fileInfo == null)
        {
            return new UploadResponse { Success = false, Error = "File info not provided" };
        }

        memoryStream.Position = 0;
        var result = await _fileStorageService.UploadToTempAsync(
            memoryStream,
            fileInfo.FileName,
            fileInfo.ContentType,
            fileInfo.UploadedBy,
            context.CancellationToken);

        if (!result.Success)
        {
            return new UploadResponse { Success = false, Error = result.Error };
        }

        return new UploadResponse
        {
            Success = true,
            FileId = result.Metadata.Id.ToString(),
            FileName = result.Metadata.FileName,
            Url = result.Metadata.Url ?? string.Empty
        };
    }

    public override async Task<ConfirmFileResponse> ConfirmFile(
        ConfirmFileRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            return new ConfirmFileResponse { Success = false, Error = "Invalid file ID" };
        }

        var confirmRequest = new FileConfirmRequest
        {
            FileId = fileId,
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            Tags = request.Tags?.ToDictionary(x => x.Key, x => x.Value)
        };

        var result = await _fileStorageService.ConfirmFileAsync(confirmRequest, context.CancellationToken);

        if (!result.Success)
        {
            return new ConfirmFileResponse { Success = false, Error = result.Error };
        }

        return new ConfirmFileResponse
        {
            Success = true,
            Metadata = MapToGrpc(result.Metadata)
        };
    }

    public override async Task<ConfirmFilesResponse> ConfirmFiles(
        ConfirmFilesRequest request,
        ServerCallContext context)
    {
        var response = new ConfirmFilesResponse { Success = true };
        var errors = new List<string>();

        foreach (var file in request.Files)
        {
            var confirmResult = await ConfirmFile(file, context);
            if (confirmResult.Success)
            {
                response.Files.Add(confirmResult.Metadata);
            }
            else
            {
                errors.Add($"File {file.FileId}: {confirmResult.Error}");
            }
        }

        if (errors.Any())
        {
            response.Errors.AddRange(errors);
            response.Success = errors.Count < request.Files.Count;
        }

        return response;
    }

    public override async Task<FileMetadataResponse> GetMetadata(
        GetMetadataRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid file ID"));
        }

        var metadata = await _fileStorageService.GetMetadataAsync(fileId, context.CancellationToken);
        
        if (metadata == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
        }

        return MapToGrpc(metadata);
    }

    public override async Task<FilesResponse> GetFilesByEntity(
        GetFilesByEntityRequest request,
        ServerCallContext context)
    {
        var files = await _fileStorageService.GetByEntityAsync(
            request.EntityType,
            request.EntityId,
            context.CancellationToken);

        var response = new FilesResponse();
        response.Files.AddRange(files.Select(MapToGrpc));
        return response;
    }

    public override async Task<DeleteFileResponse> DeleteFile(
        DeleteFileRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.FileId, out var fileId))
        {
            return new DeleteFileResponse { Success = false, Error = "Invalid file ID" };
        }

        var result = await _fileStorageService.DeleteAsync(fileId, context.CancellationToken);
        return new DeleteFileResponse { Success = result, Error = result ? null : "File not found" };
    }

    private static FileMetadataResponse MapToGrpc(FileMetadataDto metadata)
    {
        return new FileMetadataResponse
        {
            Id = metadata.Id.ToString(),
            FileName = metadata.FileName ?? string.Empty,
            OriginalFileName = metadata.OriginalFileName ?? string.Empty,
            ContentType = metadata.ContentType ?? string.Empty,
            Size = metadata.Size,
            Path = metadata.Path ?? string.Empty,
            Url = metadata.Url ?? string.Empty,
            Status = metadata.Status.ToString(),
            EntityId = metadata.EntityId ?? string.Empty,
            EntityType = metadata.EntityType ?? string.Empty,
            UploadedBy = metadata.UploadedBy ?? string.Empty,
            CreatedAt = metadata.CreatedAt.ToString("O"),
            ConfirmedAt = metadata.ConfirmedAt?.ToString("O") ?? string.Empty
        };
    }
}
