using System.ComponentModel.DataAnnotations;

namespace AuctionService.Application.DTOs
{
    public class UpdateAuctionDto
    {
        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string Make { get; set; }

        public required string Model { get; set; }

        [Range(1900, 2100)]
        public int? Year { get; set; }

        public required string Color { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }
    }
}
