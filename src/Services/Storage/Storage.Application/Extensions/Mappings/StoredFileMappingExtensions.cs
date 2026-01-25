using Storage.Application.DTOs;
using Storage.Domain.Entities;

namespace Storage.Application.Extensions.Mappings;

public static class StoredFileMappingExtensions
{
    public static FileMetadataDto ToDto(this StoredFile file)
    {
        return new FileMetadataDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            Size = file.Size,
            Checksum = file.Checksum,
            Provider = file.Provider,
            Status = file.Status,
            OwnerService = file.OwnerService,
            OwnerId = file.OwnerId,
            UploadedBy = file.UploadedBy,
            CreatedAt = file.CreatedAt,
            ConfirmedAt = file.ConfirmedAt,
            Metadata = file.Metadata,
            ResourceId = file.ResourceId,
            ResourceType = file.ResourceType,
            PortalType = file.PortalType,
            ScanResult = file.ScanResult
        };
    }

    public static IEnumerable<FileMetadataDto> ToDtoList(this IEnumerable<StoredFile> files)
    {
        return files.Select(ToDto);
    }
}
