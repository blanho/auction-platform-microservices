using Storage.Api.Protos;
using Storage.Application.DTOs;

namespace Storage.Api.Extensions.Mappings;

public static class FileMetadataGrpcMappingExtensions
{
    public static FileMetadataResponse ToGrpcResponse(this FileMetadataDto metadata)
    {
        return new FileMetadataResponse
        {
            Id = metadata.Id.ToString(),
            FileName = metadata.FileName,
            OriginalFileName = metadata.OriginalFileName,
            ContentType = metadata.ContentType,
            Size = metadata.Size,
            Path = string.Empty,
            Url = string.Empty,
            Status = metadata.Status.ToString(),
            EntityId = metadata.OwnerId ?? string.Empty,
            EntityType = metadata.OwnerService,
            UploadedBy = metadata.UploadedBy ?? string.Empty,
            CreatedAt = metadata.CreatedAt.ToString("O"),
            ConfirmedAt = metadata.ConfirmedAt?.ToString("O") ?? string.Empty
        };
    }

    public static IEnumerable<FileMetadataResponse> ToGrpcResponseList(this IEnumerable<FileMetadataDto> metadataList)
    {
        return metadataList.Select(ToGrpcResponse);
    }
}
