using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs.Auctions;

public class BuyNowDto
{
    [Required]
    public Guid AuctionId { get; set; }
}

public class BuyNowResultDto
{
    public Guid CorrelationId { get; set; }
    public string Status { get; set; } = "Processing";

    public static BuyNowResultDto FromPurchase(global::Auctions.Domain.Entities.BuyNowPurchase purchase) => new()
    {
        CorrelationId = purchase.CorrelationId,
        AuctionId = purchase.AuctionId,
        OrderId = purchase.OrderId ?? Guid.Empty,
        Buyer = purchase.Buyer,
        Seller = purchase.Seller,
        BuyNowPrice = purchase.BuyNowPrice,
        ItemTitle = purchase.ItemTitle,
        Status = purchase.Status,
        Success = purchase.Status == "Completed"
    };

    public Guid AuctionId { get; set; }
    public Guid OrderId { get; set; }
    public string Buyer { get; set; } = string.Empty;
    public string Seller { get; set; } = string.Empty;
    public decimal BuyNowPrice { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

