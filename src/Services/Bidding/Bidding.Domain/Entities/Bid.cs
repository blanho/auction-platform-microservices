using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Domain.Entities;

namespace Bidding.Domain.Entities;

public class Bid : AggregateRoot
{
    public Guid AuctionId { get; private set; }
    public Guid BidderId { get; private set; }
    public string BidderUsername { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTimeOffset BidTime { get; private set; }
    public BidStatus Status { get; private set; }

    [Timestamp]
    public uint RowVersion { get; private set; }

    private Bid() { }

    public static Bid Create(
        Guid auctionId,
        Guid bidderId,
        string bidderUsername,
        decimal amount,
        DateTimeOffset bidTime)
    {
        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            BidderId = bidderId,
            BidderUsername = bidderUsername,
            Amount = amount,
            BidTime = bidTime,
            Status = BidStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        bid.AddDomainEvent(new BidPlacedDomainEvent
        {
            BidId = bid.Id,
            AuctionId = auctionId,
            BidderId = bidderId,
            BidderUsername = bidderUsername,
            Amount = amount,
            BidTime = bidTime
        });

        return bid;
    }

    public void Accept(decimal? previousHighestAmount = null, Guid? previousBidderId = null, string? previousBidderUsername = null, bool isAutoBid = false)
    {
        Status = BidStatus.Accepted;
        AddDomainEvent(new BidAcceptedDomainEvent
        {
            BidId = Id,
            AuctionId = AuctionId,
            BidderId = BidderId,
            Amount = Amount
        });

        AddDomainEvent(new HighestBidUpdatedDomainEvent
        {
            AuctionId = AuctionId,
            BidId = Id,
            BidderId = BidderId,
            BidderUsername = BidderUsername,
            NewHighestAmount = Amount,
            PreviousHighestAmount = previousHighestAmount,
            PreviousBidderId = previousBidderId,
            PreviousBidderUsername = previousBidderUsername,
            IsAutoBid = isAutoBid
        });
    }

    public void AcceptBelowReserve()
    {
        Status = BidStatus.AcceptedBelowReserve;

        AddDomainEvent(new BidAcceptedBelowReserveDomainEvent
        {
            BidId = Id,
            AuctionId = AuctionId,
            BidderId = BidderId,
            BidderUsername = BidderUsername,
            Amount = Amount
        });
    }

    public void Reject(string reason)
    {
        Status = BidStatus.Rejected;
        AddDomainEvent(new BidRejectedDomainEvent
        {
            BidId = Id,
            AuctionId = AuctionId,
            BidderId = BidderId,
            BidderUsername = BidderUsername,
            Amount = Amount,
            Reason = reason
        });
    }

    public void Retract(
        string reason,
        bool wasHighestBid,
        Guid? newHighestBidId = null,
        decimal? newHighestAmount = null,
        Guid? newHighestBidderId = null,
        string? newHighestBidderUsername = null)
    {
        Status = BidStatus.Rejected;
        AddDomainEvent(new BidRetractedDomainEvent
        {
            BidId = Id,
            AuctionId = AuctionId,
            BidderId = BidderId,
            BidderUsername = BidderUsername,
            Amount = Amount,
            Reason = reason,
            WasHighestBid = wasHighestBid,
            NewHighestBidId = newHighestBidId,
            NewHighestAmount = newHighestAmount,
            NewHighestBidderId = newHighestBidderId,
            NewHighestBidderUsername = newHighestBidderUsername
        });
    }

    public void MarkAsTooLow()
    {
        Status = BidStatus.TooLow;

        AddDomainEvent(new BidMarkedTooLowDomainEvent
        {
            BidId = Id,
            AuctionId = AuctionId,
            BidderId = BidderId,
            Amount = Amount
        });
    }
}
