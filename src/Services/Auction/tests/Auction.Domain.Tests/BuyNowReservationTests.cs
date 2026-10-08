using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using BuildingBlocks.Domain.Exceptions;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Domain.Tests;

public class BuyNowReservationTests
{
    private static AuctionEntity NewAuction() => AuctionEntity.Create(new CreateAuctionParams(
        Guid.NewGuid(), "seller", Item.Create("item", "description"), 10,
        DateTimeOffset.UtcNow.AddHours(1), BuyNowPrice: 100));

    [Fact]
    public void OwnedReservationCompletesAndDuplicateCompletionIsIdempotent()
    {
        var auction = NewAuction();
        var correlation = Guid.NewGuid(); var buyer = Guid.NewGuid(); var order = Guid.NewGuid();
        auction.ReserveBuyNow(correlation, buyer);
        auction.ReserveBuyNow(correlation, buyer);
        auction.CompleteReservedBuyNow(correlation, buyer, "buyer", order);
        auction.CompleteReservedBuyNow(correlation, buyer, "buyer", order);
        Assert.Equal(Status.Finished, auction.Status);
        Assert.Equal(buyer, auction.WinnerId);
        Assert.Equal(order, auction.BuyNowOrderId);
        Assert.Throws<DomainInvariantException>(() => auction.ReleaseBuyNow(correlation));
    }

    [Fact]
    public void AnotherBuyerCannotReserveOrCompleteAnOwnedReservation()
    {
        var auction = NewAuction();
        var correlation = Guid.NewGuid(); var buyer = Guid.NewGuid();
        auction.ReserveBuyNow(correlation, buyer);
        Assert.Throws<DomainInvariantException>(() => auction.ReserveBuyNow(Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<DomainInvariantException>(() => auction.CompleteReservedBuyNow(correlation, Guid.NewGuid(), "other", Guid.NewGuid()));
        Assert.Equal(Status.ReservedForBuyNow, auction.Status);
    }

    [Fact]
    public void ReleaseBeforeReserveFencesLateCommands()
    {
        var auction = NewAuction(); var correlation = Guid.NewGuid();
        auction.ReleaseBuyNow(correlation);
        Assert.Throws<DomainInvariantException>(() => auction.ReserveBuyNow(correlation, Guid.NewGuid()));
        Assert.Equal(Status.Live, auction.Status);
    }

    [Fact]
    public void StaleReleaseDoesNotReleaseTheNextBuyersReservation()
    {
        var auction = NewAuction(); var first = Guid.NewGuid(); var next = Guid.NewGuid();
        auction.ReserveBuyNow(first, Guid.NewGuid());
        auction.ReleaseBuyNow(first);
        auction.ReserveBuyNow(next, Guid.NewGuid());
        auction.ReleaseBuyNow(first);
        Assert.Equal(Status.ReservedForBuyNow, auction.Status);
        Assert.Equal(next, auction.BuyNowCorrelationId);
    }

    [Fact]
    public void ReservationFreezesTheQuotedPrice()
    {
        var auction = NewAuction();
        auction.ReserveBuyNow(Guid.NewGuid(), Guid.NewGuid());
        Assert.Throws<InvalidEntityStateException>(() => auction.UpdateBuyNowPrice(200));
        Assert.Equal(100m, auction.BuyNowPrice);
    }

    [Fact]
    public void ReleasePreventsLateCompletion()
    {
        var auction = NewAuction(); var correlation = Guid.NewGuid(); var buyer = Guid.NewGuid();
        auction.ReserveBuyNow(correlation, buyer);
        auction.ReleaseBuyNow(correlation);
        Assert.Throws<DomainInvariantException>(() => auction.CompleteReservedBuyNow(correlation, buyer, "buyer", Guid.NewGuid()));
    }
}
