namespace AuctionService.Application.DTOs;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public string AuctionTitle { get; set; } = string.Empty;
    public string? AuctionImageUrl { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
}
