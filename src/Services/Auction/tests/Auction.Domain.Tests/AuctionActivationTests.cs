using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Domain.Events;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Domain.Tests;

public class AuctionActivationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(Status.Scheduled, 1, true)]
    [InlineData(Status.Inactive, 1, true)]
    [InlineData(Status.Scheduled, 0, false)]
    [InlineData(Status.Inactive, 0, false)]
    [InlineData(Status.Scheduled, -1, false)]
    [InlineData(Status.Live, 1, false)]
    [InlineData(Status.ReservedForBuyNow, 1, false)]
    [InlineData(Status.Finished, 1, false)]
    [InlineData(Status.ReservedNotMet, 1, false)]
    [InlineData(Status.Cancelled, 1, false)]
    public void ActivationRequiresAnEligibleStateAndAFutureEnd(Status status, int remainingTicks, bool expected)
    {
        var auction = Create(status, Now.AddTicks(remainingTicks));

        var changed = auction.TryActivate(Now);

        Assert.Equal(expected, changed);
        Assert.Equal(expected ? Status.Live : status, auction.Status);
        AssertTransition(auction, status, Status.Live, expected);
    }

    [Theory]
    [InlineData(Status.Live, true)]
    [InlineData(Status.Scheduled, true)]
    [InlineData(Status.Inactive, false)]
    [InlineData(Status.ReservedForBuyNow, false)]
    [InlineData(Status.Finished, false)]
    [InlineData(Status.ReservedNotMet, false)]
    [InlineData(Status.Cancelled, false)]
    public void DeactivationRequiresAnEligibleStateEvenAfterTheEndDate(Status status, bool expected)
    {
        var auction = Create(status, Now.AddDays(-1));

        var changed = auction.TryDeactivate();

        Assert.Equal(expected, changed);
        Assert.Equal(expected ? Status.Inactive : status, auction.Status);
        AssertTransition(auction, status, Status.Inactive, expected);
    }

    private static void AssertTransition(AuctionEntity auction, Status previous, Status target, bool changed)
    {
        if (!changed)
        {
            Assert.Empty(auction.DomainEvents);
            return;
        }

        var notification = Assert.IsType<AuctionStatusChangedDomainEvent>(Assert.Single(auction.DomainEvents));
        Assert.Equal(previous, notification.OldStatus);
        Assert.Equal(target, notification.NewStatus);
        Assert.Equal(auction.Id, notification.AuctionId);
        Assert.Equal(auction.Item.Title, notification.Title);
        Assert.Equal(auction.AuctionEnd, notification.AuctionEnd);
    }

    private static AuctionEntity Create(Status status, DateTimeOffset end)
    {
        var seller = Guid.NewGuid();
        var item = Item.Create("Item", "Description");
        var auction = status == Status.Scheduled
            ? AuctionEntity.CreateScheduled(seller, "seller", item, 10, end)
            : AuctionEntity.Create(new CreateAuctionParams(seller, "seller", item, 10, end, BuyNowPrice: 100));

        switch (status)
        {
            case Status.Inactive: auction.ChangeStatus(Status.Inactive); break;
            case Status.ReservedForBuyNow: auction.ReserveBuyNow(Guid.NewGuid(), Guid.NewGuid()); break;
            case Status.Finished: auction.Finish(Guid.NewGuid(), "buyer", 20, true); break;
            case Status.ReservedNotMet: auction.Finish(null, null, null, false); break;
            case Status.Cancelled: auction.Cancel("cancelled"); break;
        }

        auction.ClearDomainEvents();
        return auction;
    }
}
