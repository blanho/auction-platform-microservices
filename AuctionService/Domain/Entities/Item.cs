#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities
{
    public class Item : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Condition { get; set; }
        public int? YearManufactured { get; set; }
        
        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public Guid? BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        public Auction? Auction { get; set; }
        public Guid AuctionId { get; set; }
        
        public List<ItemFileInfo> Files { get; set; } = new();
        public Dictionary<string, string> Attributes { get; set; } = new();
        
        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
