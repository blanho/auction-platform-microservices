using Common.Domain.Entities;

namespace AuctionService.Domain.Entities
{
    public class Item : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public Auction Auction { get; set; }
        public Guid AuctionId { get; set; }
    }
}
