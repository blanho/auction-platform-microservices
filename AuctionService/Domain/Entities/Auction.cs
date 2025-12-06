#nullable enable
using Common.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public int ReversePrice { get; set; } = 0;
        public required string Seller { get; set; }
        public string? Winner { get; set; }
        public int? SoldAmount { get; set; }
        public int? CurrentHighBid { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public Status Status { get; set; }
        public required Item Item { get; set; }
        
        /// <summary>
        /// Files associated with this auction stored as JSONB (images, documents, etc.)
        /// </summary>
        public List<AuctionFileInfo> Files { get; set; } = new();
    }

    /// <summary>
    /// File information stored as JSONB in Auction entity
    /// </summary>
    public class AuctionFileInfo
    {
        /// <summary>
        /// The ID of the file in UtilityService's storage
        /// </summary>
        public Guid StorageFileId { get; set; }

        /// <summary>
        /// Original filename
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File content type (e.g., image/jpeg)
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// URL to access the file
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Type of file (e.g., "image", "document")
        /// </summary>
        public string FileType { get; set; } = "image";

        /// <summary>
        /// Display order for images
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Whether this is the primary/featured image
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// When the file was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
