using Bidding.Domain.Entities;

namespace Bidding.Domain.Tests.Builders;

public class BidBuilder
{
    private Guid _auctionId = Guid.NewGuid();
    private Guid _bidderId = Guid.NewGuid();
    private string _bidderUsername = "test_bidder";
    private decimal _amount = 100m;
    private DateTimeOffset _bidTime = DateTimeOffset.UtcNow;

    public static BidBuilder Default() => new();

    public BidBuilder WithAuctionId(Guid auctionId)
    {
        _auctionId = auctionId;
        return this;
    }

    public BidBuilder WithBidderId(Guid bidderId)
    {
        _bidderId = bidderId;
        return this;
    }

    public BidBuilder WithBidderUsername(string username)
    {
        _bidderUsername = username;
        return this;
    }

    public BidBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public BidBuilder WithBidTime(DateTimeOffset bidTime)
    {
        _bidTime = bidTime;
        return this;
    }

    public Bid Build()
    {
        return Bid.Create(
            _auctionId,
            _bidderId,
            _bidderUsername,
            _amount,
            _bidTime);
    }
}
