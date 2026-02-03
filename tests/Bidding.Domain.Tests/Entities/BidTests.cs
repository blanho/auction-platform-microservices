using Bidding.Domain.Tests.Builders;
using Bidding.Domain.Entities;
using Bidding.Domain.Enums;
using Bidding.Domain.Events;

namespace Bidding.Domain.Tests.Entities;

public class BidTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateBid()
    {
        var auctionId = Guid.NewGuid();
        var bidderId = Guid.NewGuid();
        var bidderUsername = "test_bidder";
        var amount = 100m;
        var bidTime = DateTimeOffset.UtcNow;

        var bid = BidBuilder.Default()
            .WithAuctionId(auctionId)
            .WithBidderId(bidderId)
            .WithBidderUsername(bidderUsername)
            .WithAmount(amount)
            .WithBidTime(bidTime)
            .Build();

        bid.Should().NotBeNull();
        bid.Id.Should().NotBeEmpty();
        bid.AuctionId.Should().Be(auctionId);
        bid.BidderId.Should().Be(bidderId);
        bid.BidderUsername.Should().Be(bidderUsername);
        bid.Amount.Should().Be(amount);
        bid.BidTime.Should().Be(bidTime);
        bid.Status.Should().Be(BidStatus.Pending);
    }

    [Fact]
    public void Create_ShouldRaiseBidPlacedDomainEvent()
    {
        var bid = BidBuilder.Default().Build();

        bid.DomainEvents.Should().ContainSingle();
        var domainEvent = bid.DomainEvents.First() as BidPlacedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.BidId.Should().Be(bid.Id);
        domainEvent.AuctionId.Should().Be(bid.AuctionId);
        domainEvent.BidderId.Should().Be(bid.BidderId);
        domainEvent.Amount.Should().Be(bid.Amount);
    }

    [Fact]
    public void Accept_ShouldSetStatusToAccepted()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();

        bid.Accept();

        bid.Status.Should().Be(BidStatus.Accepted);
    }

    [Fact]
    public void Accept_ShouldRaiseBidAcceptedAndHighestBidUpdatedEvents()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();

        bid.Accept();

        bid.DomainEvents.Should().HaveCount(2);
        bid.DomainEvents.Should().ContainItemsAssignableTo<BidAcceptedDomainEvent>();
        bid.DomainEvents.Should().ContainItemsAssignableTo<HighestBidUpdatedDomainEvent>();
    }

    [Fact]
    public void Accept_WithPreviousBid_ShouldIncludePreviousBidInfo()
    {
        var bid = BidBuilder.Default()
            .WithAmount(200m)
            .Build();
        bid.ClearDomainEvents();
        var previousAmount = 150m;
        var previousBidderId = Guid.NewGuid();
        var previousBidderUsername = "previous_bidder";

        bid.Accept(previousAmount, previousBidderId, previousBidderUsername);

        var highestBidEvent = bid.DomainEvents
            .OfType<HighestBidUpdatedDomainEvent>()
            .Single();
        highestBidEvent.PreviousHighestAmount.Should().Be(previousAmount);
        highestBidEvent.PreviousBidderId.Should().Be(previousBidderId);
        highestBidEvent.PreviousBidderUsername.Should().Be(previousBidderUsername);
    }

    [Fact]
    public void Accept_WithAutoBidFlag_ShouldSetIsAutoBid()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();

        bid.Accept(isAutoBid: true);

        var highestBidEvent = bid.DomainEvents
            .OfType<HighestBidUpdatedDomainEvent>()
            .Single();
        highestBidEvent.IsAutoBid.Should().BeTrue();
    }

    [Fact]
    public void AcceptBelowReserve_ShouldSetStatusToAcceptedBelowReserve()
    {
        var bid = BidBuilder.Default().Build();

        bid.AcceptBelowReserve();

        bid.Status.Should().Be(BidStatus.AcceptedBelowReserve);
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejected()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();
        var reason = "Bid too low";

        bid.Reject(reason);

        bid.Status.Should().Be(BidStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldRaiseBidRejectedDomainEvent()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();
        var reason = "Bid too low";

        bid.Reject(reason);

        bid.DomainEvents.Should().ContainSingle();
        var domainEvent = bid.DomainEvents.First() as BidRejectedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.BidId.Should().Be(bid.Id);
        domainEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Retract_ShouldSetStatusToRejected()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();

        bid.Retract("Changed mind", wasHighestBid: false);

        bid.Status.Should().Be(BidStatus.Rejected);
    }

    [Fact]
    public void Retract_WhenWasHighestBid_ShouldIncludeNewHighestBidInfo()
    {
        var bid = BidBuilder.Default().Build();
        bid.ClearDomainEvents();
        var newHighestBidId = Guid.NewGuid();
        var newHighestAmount = 90m;
        var newHighestBidderId = Guid.NewGuid();
        var newHighestBidderUsername = "new_highest";

        bid.Retract(
            "Changed mind",
            wasHighestBid: true,
            newHighestBidId,
            newHighestAmount,
            newHighestBidderId,
            newHighestBidderUsername);

        var domainEvent = bid.DomainEvents.First() as BidRetractedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.WasHighestBid.Should().BeTrue();
        domainEvent.NewHighestBidId.Should().Be(newHighestBidId);
        domainEvent.NewHighestAmount.Should().Be(newHighestAmount);
        domainEvent.NewHighestBidderId.Should().Be(newHighestBidderId);
        domainEvent.NewHighestBidderUsername.Should().Be(newHighestBidderUsername);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void Create_WithVariousAmounts_ShouldAcceptAll(decimal amount)
    {
        var bid = BidBuilder.Default()
            .WithAmount(amount)
            .Build();

        bid.Amount.Should().Be(amount);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var bid = BidBuilder.Default().Build();
        bid.DomainEvents.Should().NotBeEmpty();

        bid.ClearDomainEvents();

        bid.DomainEvents.Should().BeEmpty();
    }
}
