#nullable enable
namespace Catalog.Domain.Entities;

/// <summary>
/// Shared media file value object. Stored as JSON in the owning entity's column.
/// </summary>
public class MediaFile
{
    public Guid FileId { get; private set; }
    public string FileType { get; private set; } = "image";
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private MediaFile() { }

    public static MediaFile Create(Guid fileId, string fileType, int displayOrder, bool isPrimary)
    {
        return new MediaFile
        {
            FileId = fileId,
            FileType = fileType,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary
        };
    }
}
