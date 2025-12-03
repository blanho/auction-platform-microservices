using System.ComponentModel.DataAnnotations;

namespace AuctionService.Application.DTOs
{
    public class CreateAuctionDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Make { get; set; }

        [Required]
        public required string Model { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        public required string Color { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ReservePrice { get; set; }

        [Required]
        public DateTimeOffset AuctionEnd { get; set; }
    }
}
