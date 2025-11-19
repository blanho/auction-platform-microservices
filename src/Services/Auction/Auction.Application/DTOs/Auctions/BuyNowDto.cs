using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs.Auctions;

public class BuyNowDto
{
    [Required]
    public Guid AuctionId { get; set; }
}

public class BuyNowResultDto
{
    public Guid AuctionId { get; set; }
    public Guid OrderId { get; set; }
    public string Buyer { get; set; } = string.Empty;
    public string Seller { get; set; } = string.Empty;
    public decimal BuyNowPrice { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

