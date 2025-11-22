using Common.Domain.Entities;

namespace SearchService.Domain.Entities
{
    public class SearchItem : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string Source { get; set; } 
        public Guid SourceId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public DateTime? AuctionEnd { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public int? SoldAmount { get; set; }
        public int? CurrentHighBid { get; set; }
        public int ReservePrice { get; set; }
        public SearchMetadata Metadata { get; set; }
    }
}