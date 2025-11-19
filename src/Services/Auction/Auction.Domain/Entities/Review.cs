#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Review : BaseEntity
{
    private int _rating;

    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
    public Guid? OrderId { get; set; }

    public Guid ReviewerId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;

    public Guid ReviewedUserId { get; set; }
    public string ReviewedUsername { get; set; } = string.Empty;

    public int Rating
    {
        get => _rating;
        set
        {
            if (value < 1 || value > 5)
                throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5");
            _rating = value;
        }
    }

    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? SellerResponse { get; set; }
    public DateTimeOffset? SellerResponseAt { get; set; }
    public void AddSellerResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Seller response cannot be empty", nameof(response));

        if (response.Length > 1000)
            throw new ArgumentException("Seller response cannot exceed 1000 characters", nameof(response));

        SellerResponse = response;
        SellerResponseAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSellerResponse()
    {
        SellerResponse = null;
        SellerResponseAt = null;
    }
}
