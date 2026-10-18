#nullable enable
using System.Text.Json.Serialization;

namespace Catalog.Domain.Entities;

public class MediaFile
{
    public Guid FileId { get; private set; }
    public string FileType { get; private set; } = "image";
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    [JsonConstructor]
    private MediaFile(Guid fileId, string fileType, int displayOrder, bool isPrimary)
    {
        FileId = fileId;
        FileType = fileType;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }

    public static MediaFile Create(Guid fileId, string fileType, int displayOrder, bool isPrimary) =>
        new(fileId, fileType, displayOrder, isPrimary);
}
