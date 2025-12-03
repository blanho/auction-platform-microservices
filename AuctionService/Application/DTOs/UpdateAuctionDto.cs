using System.ComponentModel.DataAnnotations;

namespace AuctionService.Application.DTOs
{
    public class UpdateAuctionDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        [Range(1900, 2100)]
        public int? Year { get; set; }

        public string Color { get; set; }

        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }
    }
}
