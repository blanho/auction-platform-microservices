#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Review : AggregateRoot
{
    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
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

    public static Review Create(
        Guid auctionId,
        Auction? auction,
        Guid? orderId,
        Guid reviewerId,
        string reviewerUsername,
        Guid reviewedUserId,
        string reviewedUsername,
        int rating,
        string? title,
        string? comment)
    {
        var review = new Review
        {
            AuctionId = auctionId,
            Auction = auction,
            OrderId = orderId,
            ReviewerId = reviewerId,
            ReviewerUsername = reviewerUsername,
            ReviewedUserId = reviewedUserId,
            ReviewedUsername = reviewedUsername,
            Rating = rating,
            Title = title,
            Comment = comment
        };
        return review;
    }
    public void AddSellerResponse(string response)
    {
        SellerResponse = response;
        SellerResponseAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSellerResponse()
    {
        SellerResponse = null;
        SellerResponseAt = null;
    }
}
