#nullable enable
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Auctions.Domain.Entities;

public class Review : AggregateRoot
{
    private const int MinRating = 1;
    private const int MaxRating = 5;

    public Guid AuctionId { get; private set; }
    public Auction? Auction { get; private set; }
    public Guid? OrderId { get; private set; }

    public Guid ReviewerId { get; private set; }
    public string ReviewerUsername { get; private set; } = string.Empty;

    public Guid ReviewedUserId { get; private set; }
    public string ReviewedUsername { get; private set; } = string.Empty;

    public int Rating { get; private set; }

    public string? Title { get; private set; }
    public string? Comment { get; private set; }
    public string? SellerResponse { get; private set; }
    public DateTimeOffset? SellerResponseAt { get; private set; }

    private Review() { }

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
        if (rating is < MinRating or > MaxRating)
            throw new DomainInvariantException($"Rating must be between {MinRating} and {MaxRating}");

        return new Review
        {
            Id = Guid.NewGuid(),
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
    }

    public void AddSellerResponse(string response, DateTimeOffset? respondedAt = null)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new DomainInvariantException("Seller response cannot be empty");

        SellerResponse = response;
        SellerResponseAt = respondedAt ?? DateTimeOffset.UtcNow;
    }

    public void RemoveSellerResponse()
    {
        SellerResponse = null;
        SellerResponseAt = null;
    }

    public void UpdateRating(int rating)
    {
        if (rating is < MinRating or > MaxRating)
            throw new DomainInvariantException($"Rating must be between {MinRating} and {MaxRating}");

        Rating = rating;
    }
}
