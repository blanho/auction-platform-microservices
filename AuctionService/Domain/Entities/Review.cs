#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class Review : BaseEntity
{
    public Guid AuctionId { get; set; }
    public required Auction Auction { get; set; }
    public Guid? OrderId { get; set; }
    
    public Guid ReviewerId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;
    
    public Guid ReviewedUserId { get; set; }
    public string ReviewedUsername { get; set; } = string.Empty;
    
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? SellerResponse { get; set; }
    public DateTimeOffset? SellerResponseAt { get; set; }
}
