using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs.Auctions;

public class CancelAuctionDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
