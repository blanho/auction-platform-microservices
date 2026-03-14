using Auctions.Domain.Entities;

namespace Auctions.Application.DTOs.Audit;

public record AuctionAuditData
{
    public required Guid AuctionId { get; init; }
    public required Guid SellerId { get; init; }
    public string? SellerUsername { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public decimal ReservePrice { get; init; }
    public decimal? BuyNowPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTimeOffset AuctionEnd { get; init; }
    public Guid? WinnerId { get; init; }
    public string? WinnerUsername { get; init; }
    public decimal? SoldAmount { get; init; }
    public decimal? CurrentHighBid { get; init; }
    public bool IsFeatured { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public static AuctionAuditData FromAuction(Auction auction)
    {
        return new AuctionAuditData
        {
            AuctionId = auction.Id,
            SellerId = auction.SellerId,
            SellerUsername = auction.SellerUsername,
            Title = auction.Item.Title,
            Description = auction.Item.Description,
            Status = auction.Status.ToString(),
            ReservePrice = auction.ReservePrice,
            BuyNowPrice = auction.BuyNowPrice,
            Currency = auction.Currency,
            AuctionEnd = auction.AuctionEnd,
            WinnerId = auction.WinnerId,
            WinnerUsername = auction.WinnerUsername,
            SoldAmount = auction.SoldAmount,
            CurrentHighBid = auction.CurrentHighBid,
            IsFeatured = auction.IsFeatured,
            CategoryId = auction.Item.CategoryId,
            BrandId = auction.Item.BrandId,
            Condition = auction.Item.Condition,
            YearManufactured = auction.Item.YearManufactured,
            CreatedAt = auction.CreatedAt,
            UpdatedAt = auction.UpdatedAt
        };
    }
}
