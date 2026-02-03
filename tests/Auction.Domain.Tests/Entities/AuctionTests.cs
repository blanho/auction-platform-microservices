using Auction.Domain.Tests.Builders;
using Auctions.Domain.Events;
using BuildingBlocks.Domain.Enums;

namespace Auction.Domain.Tests.Entities;

public class AuctionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAuction()
    {
        var sellerId = Guid.NewGuid();
        var sellerUsername = "test_seller";
        var item = ItemBuilder.Default().Build();
        var reservePrice = 100m;
        var auctionEnd = DateTimeOffset.UtcNow.AddDays(7);

        var auction = AuctionBuilder.Default()
            .WithSellerId(sellerId)
            .WithSellerUsername(sellerUsername)
            .WithItem(item)
            .WithReservePrice(reservePrice)
            .WithAuctionEnd(auctionEnd)
            .Build();

        auction.Should().NotBeNull();
        auction.Id.Should().NotBeEmpty();
        auction.SellerId.Should().Be(sellerId);
        auction.SellerUsername.Should().Be(sellerUsername);
        auction.ReservePrice.Should().Be(reservePrice);
        auction.AuctionEnd.Should().Be(auctionEnd);
        auction.Status.Should().Be(Status.Live);
        auction.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithBuyNowPrice_ShouldEnableBuyNow()
    {
        var buyNowPrice = 500m;

        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(buyNowPrice)
            .Build();

        auction.BuyNowPrice.Should().Be(buyNowPrice);
        auction.IsBuyNowEnabled.Should().BeTrue();
        auction.IsBuyNowAvailable.Should().BeTrue();
    }

    [Fact]
    public void Create_WithBuyNowPriceLessThanReserve_ShouldThrowException()
    {
        var action = () => AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(50m)
            .Build();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Buy now price must be greater than reserve price*");
    }

    [Fact]
    public void Create_WithBuyNowPriceEqualToReserve_ShouldThrowException()
    {
        var action = () => AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(100m)
            .Build();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Buy now price must be greater than reserve price*");
    }

    [Fact]
    public void CreateScheduled_ShouldHaveScheduledStatus()
    {
        var auction = AuctionBuilder.Default().BuildScheduled();

        auction.Status.Should().Be(Status.Scheduled);
    }

    [Fact]
    public void UpdateReservePrice_WhenNoBids_ShouldUpdatePrice()
    {
        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .Build();

        auction.UpdateReservePrice(150m);

        auction.ReservePrice.Should().Be(150m);
    }

    [Fact]
    public void UpdateBuyNowPrice_WhenAuctionIsLive_ShouldUpdatePrice()
    {
        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(200m)
            .Build();

        auction.UpdateBuyNowPrice(300m);

        auction.BuyNowPrice.Should().Be(300m);
    }

    [Fact]
    public void UpdateBuyNowPrice_WithPriceBelowReserve_ShouldThrowException()
    {
        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(200m)
            .Build();

        var action = () => auction.UpdateBuyNowPrice(50m);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Buy now price must be greater than reserve price*");
    }

    [Fact]
    public void ExtendAuctionEnd_WhenLive_ShouldExtendEndTime()
    {
        var originalEnd = DateTimeOffset.UtcNow.AddDays(7);
        var auction = AuctionBuilder.Default()
            .WithAuctionEnd(originalEnd)
            .Build();
        var extension = TimeSpan.FromHours(24);

        auction.ExtendAuctionEnd(extension);

        auction.AuctionEnd.Should().Be(originalEnd.Add(extension));
    }

    [Fact]
    public void RaiseCreatedEvent_ShouldAddDomainEvent()
    {
        var auction = AuctionBuilder.Default().Build();

        auction.RaiseCreatedEvent();

        auction.DomainEvents.Should().ContainSingle();
        auction.DomainEvents.First().Should().BeOfType<AuctionCreatedDomainEvent>();
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndRaiseEvent()
    {
        var auction = AuctionBuilder.Default().Build();

        auction.ChangeStatus(Status.Finished);

        auction.Status.Should().Be(Status.Finished);
        auction.DomainEvents.Should().ContainSingle();
        var domainEvent = auction.DomainEvents.First() as AuctionStatusChangedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.OldStatus.Should().Be(Status.Live);
        domainEvent.NewStatus.Should().Be(Status.Finished);
    }

    [Fact]
    public void Finish_WithWinner_ShouldSetWinnerAndRaiseEvent()
    {
        var auction = AuctionBuilder.Default().Build();
        var winnerId = Guid.NewGuid();
        var winnerUsername = "winner";
        var soldAmount = 150m;

        auction.Finish(winnerId, winnerUsername, soldAmount, itemSold: true);

        auction.WinnerId.Should().Be(winnerId);
        auction.WinnerUsername.Should().Be(winnerUsername);
        auction.SoldAmount.Should().Be(soldAmount);
        auction.Status.Should().Be(Status.Finished);
        auction.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AuctionFinishedDomainEvent>();
    }

    [Fact]
    public void Finish_WithoutSale_ShouldSetReserveNotMetStatus()
    {
        var auction = AuctionBuilder.Default().Build();

        auction.Finish(null, null, null, itemSold: false);

        auction.Status.Should().Be(Status.ReservedNotMet);
    }

    [Fact]
    public void ExecuteBuyNow_ShouldSetWinnerAndFinishAuction()
    {
        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(500m)
            .Build();
        var buyerId = Guid.NewGuid();
        var buyerUsername = "buyer";

        auction.ExecuteBuyNow(buyerId, buyerUsername);

        auction.WinnerId.Should().Be(buyerId);
        auction.WinnerUsername.Should().Be(buyerUsername);
        auction.SoldAmount.Should().Be(500m);
        auction.Status.Should().Be(Status.Finished);
        auction.DomainEvents.Should().HaveCount(2);
        auction.DomainEvents.Should().ContainItemsAssignableTo<BuyNowExecutedDomainEvent>();
        auction.DomainEvents.Should().ContainItemsAssignableTo<AuctionFinishedDomainEvent>();
    }

    [Fact]
    public void UpdateHighBid_ShouldUpdateCurrentHighBidAndWinner()
    {
        var auction = AuctionBuilder.Default().Build();
        var bidderId = Guid.NewGuid();
        var bidderUsername = "bidder";
        var bidAmount = 200m;

        auction.UpdateHighBid(bidAmount, bidderId, bidderUsername);

        auction.CurrentHighBid.Should().Be(bidAmount);
        auction.WinnerId.Should().Be(bidderId);
        auction.WinnerUsername.Should().Be(bidderUsername);
    }

    [Fact]
    public void Cancel_WhenLive_ShouldSetCancelledStatus()
    {
        var auction = AuctionBuilder.Default().Build();

        auction.Cancel("Test cancellation");

        auction.Status.Should().Be(Status.Cancelled);
        auction.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AuctionStatusChangedDomainEvent>();
    }

    [Fact]
    public void IsBuyNowAvailable_WhenSold_ShouldReturnFalse()
    {
        var auction = AuctionBuilder.Default()
            .WithReservePrice(100m)
            .WithBuyNowPrice(500m)
            .Build();

        auction.ExecuteBuyNow(Guid.NewGuid(), "buyer");

        auction.IsBuyNowAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Create_WithDifferentCurrencies_ShouldSetCurrency(string currency)
    {
        var auction = AuctionBuilder.Default()
            .WithCurrency(currency)
            .Build();

        auction.Currency.Should().Be(currency);
    }

    [Fact]
    public void Create_WithFeatured_ShouldSetIsFeatured()
    {
        var auction = AuctionBuilder.Default()
            .WithFeatured(true)
            .Build();

        auction.IsFeatured.Should().BeTrue();
    }
}
