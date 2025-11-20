using System.ComponentModel.DataAnnotations;

namespace AuctionService.DTOs
{
    public class CreateAuctionDto
    {
        [Required]
        public string Make { get; set; }
        
        [Required]
        public string Model { get; set; }
        
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
        
        [Required]
        public string Color { get; set; }
        
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
