#nullable enable

namespace Storage.Domain.ValueObjects;

public class FileExtendedProperties
{

    public string? TempPath { get; set; }

    public string? CdnUrl { get; set; }

    public ImageDimensions? Dimensions { get; set; }

    public string? ThumbnailPath { get; set; }

    public string? ThumbnailCdnUrl { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    public string? ProcessingJobId { get; set; }

    public string? ProcessingError { get; set; }

    public string? UploadId { get; set; }

    public int? PartsCount { get; set; }

    public List<int>? CompletedParts { get; set; }

    public string? DeduplicationHash { get; set; }
}

public class ImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }

    public ImageDimensions() { }

    public ImageDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
