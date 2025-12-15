#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class Review : BaseEntity
{
    public Guid AuctionId { get; set; }
    public Guid? OrderId { get; set; }
    public required string ReviewerUsername { get; set; }
    public required string ReviewedUsername { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? SellerResponse { get; set; }
    public DateTimeOffset? SellerResponseAt { get; set; }
    
    public Auction? Auction { get; set; }
}
