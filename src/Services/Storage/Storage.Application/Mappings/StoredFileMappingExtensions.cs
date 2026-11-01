using Storage.Application.DTOs;
using Storage.Domain.Constants;
using Storage.Domain.Entities;

namespace Storage.Application.Mappings;

public static class StoredFileMappingExtensions
{
    public static StoredFileDto ToSummaryDto(this StoredFile file) => new(
        FileId: file.Id,
        FileName: file.FileName,
        ContentType: file.ContentType,
        FileSize: file.FileSize,
        Url: file.Url,
        UploadedAt: file.CreatedAt);

    public static StoredFileDto ToDetailsDto(this StoredFile file)
    {
        ProcessedImageDto? image = null;
        if (file.Metadata.TryGetValue(StorageDefaults.Images.WidthKey, out var widthText)
            && file.Metadata.TryGetValue(StorageDefaults.Images.HeightKey, out var heightText)
            && file.Metadata.TryGetValue(StorageDefaults.Images.ThumbnailKey, out var thumbnail)
            && int.TryParse(widthText, out var width)
            && int.TryParse(heightText, out var height))
        {
            image = new ProcessedImageDto(width, height, thumbnail);
        }

        return file.ToSummaryDto() with { Checksum = file.Checksum, Image = image };
    }
}
