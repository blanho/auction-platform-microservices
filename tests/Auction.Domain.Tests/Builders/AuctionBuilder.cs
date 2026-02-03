using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auction.Domain.Tests.Builders;

public class AuctionBuilder
{
    private Guid _sellerId = Guid.NewGuid();
    private string _sellerUsername = "test_seller";
    private Item _item = ItemBuilder.Default().Build();
    private decimal _reservePrice = 100m;
    private DateTimeOffset _auctionEnd = DateTimeOffset.UtcNow.AddDays(7);
    private string _currency = "USD";
    private decimal? _buyNowPrice = null;
    private bool _isFeatured = false;

    public static AuctionBuilder Default() => new();

    public AuctionBuilder WithSellerId(Guid sellerId)
    {
        _sellerId = sellerId;
        return this;
    }

    public AuctionBuilder WithSellerUsername(string username)
    {
        _sellerUsername = username;
        return this;
    }

    public AuctionBuilder WithItem(Item item)
    {
        _item = item;
        return this;
    }

    public AuctionBuilder WithReservePrice(decimal price)
    {
        _reservePrice = price;
        return this;
    }

    public AuctionBuilder WithAuctionEnd(DateTimeOffset end)
    {
        _auctionEnd = end;
        return this;
    }

    public AuctionBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public AuctionBuilder WithBuyNowPrice(decimal? price)
    {
        _buyNowPrice = price;
        return this;
    }

    public AuctionBuilder WithFeatured(bool isFeatured = true)
    {
        _isFeatured = isFeatured;
        return this;
    }

    public Auctions.Domain.Entities.Auction Build()
    {
        return Auctions.Domain.Entities.Auction.Create(
            _sellerId,
            _sellerUsername,
            _item,
            _reservePrice,
            _auctionEnd,
            _currency,
            _buyNowPrice,
            _isFeatured);
    }

    public Auctions.Domain.Entities.Auction BuildScheduled()
    {
        return Auctions.Domain.Entities.Auction.CreateScheduled(
            _sellerId,
            _sellerUsername,
            _item,
            _reservePrice,
            _auctionEnd,
            _currency);
    }
}
