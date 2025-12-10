#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities
{
    public class Item : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Mileage { get; set; }
        
        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public Auction? Auction { get; set; }
        public Guid AuctionId { get; set; }
        
        public List<ItemFileInfo> Files { get; set; } = new();
    }
}
