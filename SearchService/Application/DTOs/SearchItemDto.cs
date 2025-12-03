namespace SearchService.Application.DTOs
{
    public class SearchItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public Guid SourceId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public decimal Relevance { get; set; }
        public DateTimeOffset LastIndexed { get; set; }
        
        // Auction-specific fields
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
    }
}