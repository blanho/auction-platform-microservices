#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsageCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}

public class ItemTag
{
    public Guid ItemId { get; set; }
    public Item? Item { get; set; }
    public Guid TagId { get; set; }
    public Tag? Tag { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
