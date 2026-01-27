#nullable enable
using Auctions.Domain.Events;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Domain.Entities;

public class Auction : BaseEntity
{
    private Auction() { }
    public decimal ReservePrice { get; private set; }
    public decimal? BuyNowPrice { get; private set; }
    public string Currency { get; private set; } = "USD";
    public bool IsBuyNowEnabled => BuyNowPrice.HasValue && BuyNowPrice > 0;
    public bool IsBuyNowAvailable => IsBuyNowEnabled && Status == Status.Live && !SoldAmount.HasValue;

    public Guid SellerId { get; private set; }
    public string SellerUsername { get; private set; } = string.Empty;
    public Guid? WinnerId { get; private set; }
    public string? WinnerUsername { get; private set; }

    public decimal? SoldAmount { get; private set; }
    public decimal? CurrentHighBid { get; private set; }
    public DateTimeOffset AuctionEnd { get; private set; }
    public Status Status { get; private set; }
    public bool IsFeatured { get; private set; }
    public Item Item { get; private set; } = null!;

    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<Bookmark> Bookmarks { get; private set; } = new List<Bookmark>();

    public static Auction Create(
        Guid sellerId,
        string sellerUsername,
        Item item,
        decimal reservePrice,
        DateTimeOffset auctionEnd,
        string currency = "USD",
        decimal? buyNowPrice = null,
        bool isFeatured = false)
    {
        if (buyNowPrice.HasValue && buyNowPrice.Value <= reservePrice)
            throw new InvalidOperationException("Buy now price must be greater than reserve price");

        return new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            SellerUsername = sellerUsername,
            Item = item,
            ReservePrice = reservePrice,
            AuctionEnd = auctionEnd,
            Currency = currency,
            BuyNowPrice = buyNowPrice,
            IsFeatured = isFeatured,
            Status = Status.Live,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static Auction CreateScheduled(
        Guid sellerId,
        string sellerUsername,
        Item item,
        decimal reservePrice,
        DateTimeOffset auctionEnd,
        string currency = "USD")
    {

        return new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            SellerUsername = sellerUsername,
            Item = item,
            ReservePrice = reservePrice,
            AuctionEnd = auctionEnd,
            Currency = currency,
            Status = Status.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static Auction CreateForSeeding(
        Guid id,
        string sellerUsername,
        Item item,
        decimal reservePrice,
        DateTimeOffset auctionEnd,
        Status status,
        string currency = "USD",
        bool isFeatured = false,
        DateTimeOffset? createdAt = null)
    {
        return new Auction
        {
            Id = id,
            SellerId = Guid.Empty,
            SellerUsername = sellerUsername,
            Item = item,
            ReservePrice = reservePrice,
            AuctionEnd = auctionEnd,
            Currency = currency,
            Status = status,
            IsFeatured = isFeatured,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    public static Auction CreateSnapshot(Auction source)
    {
        return new Auction
        {
            Id = source.Id,
            SellerId = source.SellerId,
            SellerUsername = source.SellerUsername,
            ReservePrice = source.ReservePrice,
            BuyNowPrice = source.BuyNowPrice,
            Currency = source.Currency,
            AuctionEnd = source.AuctionEnd,
            Status = source.Status,
            IsFeatured = source.IsFeatured,
            Item = Item.CreateSnapshot(source.Item)
        };
    }

    public void UpdateReservePrice(decimal newPrice)
    {
        if (Status != Status.Live || CurrentHighBid.HasValue)
            throw new InvalidOperationException("Cannot change reserve price after bids have been placed");

        ReservePrice = newPrice;
    }

    public void UpdateBuyNowPrice(decimal? newPrice)
    {
        if (Status == Status.Finished)
            throw new InvalidOperationException("Cannot change buy now price on finished auction");

        if (newPrice.HasValue && newPrice.Value <= ReservePrice)
            throw new InvalidOperationException("Buy now price must be greater than reserve price");

        BuyNowPrice = newPrice;
    }

    public void ExtendAuctionEnd(TimeSpan extension, string reason)
    {
        if (Status != Status.Live)
            throw new InvalidOperationException("Can only extend live auctions");

        AuctionEnd = AuctionEnd.Add(extension);
    }

    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new AuctionCreatedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            Title = Item.Title,
            ReservePrice = ReservePrice,
            AuctionEnd = AuctionEnd
        });
    }

    public void ChangeStatus(Status newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        AddDomainEvent(new AuctionStatusChangedDomainEvent
        {
            AuctionId = Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            Title = Item?.Title ?? string.Empty,
            ReservePrice = ReservePrice,
            AuctionEnd = AuctionEnd
        });
    }

    public void Finish(Guid? winnerId, string? winnerUsername, decimal? soldAmount, bool itemSold)
    {
        WinnerId = winnerId;
        WinnerUsername = winnerUsername;
        SoldAmount = soldAmount;
        Status = itemSold ? Status.Finished : Status.ReservedNotMet;

        AddDomainEvent(new AuctionFinishedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            WinnerId = winnerId,
            WinnerUsername = winnerUsername,
            SoldAmount = soldAmount,
            ItemSold = itemSold
        });
    }

    public void ExecuteBuyNow(Guid buyerId, string buyerUsername)
    {
        WinnerId = buyerId;
        WinnerUsername = buyerUsername;
        SoldAmount = BuyNowPrice;
        Status = Status.Finished;

        AddDomainEvent(new BuyNowExecutedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            BuyerId = buyerId,
            BuyerUsername = buyerUsername,
            BuyNowPrice = BuyNowPrice!.Value,
            ItemTitle = Item.Title
        });

        AddDomainEvent(new AuctionFinishedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            WinnerId = buyerId,
            WinnerUsername = buyerUsername,
            SoldAmount = BuyNowPrice,
            ItemSold = true
        });
    }

    public void RaiseUpdatedEvent(IReadOnlyList<string>? modifiedFields = null)
    {
        AddDomainEvent(new AuctionUpdatedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId,
            Title = Item.Title,
            Description = Item.Description,
            Condition = Item.Condition,
            YearManufactured = Item.YearManufactured,
            UpdatedAt = DateTimeOffset.UtcNow,
            ModifiedFields = modifiedFields ?? Array.Empty<string>()
        });
    }

    public void RaiseDeletedEvent()
    {
        AddDomainEvent(new AuctionDeletedDomainEvent
        {
            AuctionId = Id,
            SellerId = SellerId
        });
    }

    public void UpdateHighBid(decimal bidAmount, Guid? bidderId = null, string? bidderUsername = null)
    {
        CurrentHighBid = bidAmount;
        if (bidderId.HasValue)
        {
            WinnerId = bidderId;
            WinnerUsername = bidderUsername;
        }
    }

    public void Cancel(string reason)
    {
        if (Status == Status.Finished)
            throw new InvalidOperationException("Cannot cancel finished auction");

        var oldStatus = Status;
        Status = Status.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new AuctionStatusChangedDomainEvent
        {
            AuctionId = Id,
            OldStatus = oldStatus,
            NewStatus = Status.Cancelled,
            SellerId = SellerId,
            SellerUsername = SellerUsername,
            Title = Item?.Title ?? string.Empty,
            ReservePrice = ReservePrice,
            AuctionEnd = AuctionEnd
        });
    }

    public void UpdateSellerUsername(string newUsername)
    {

        if (SellerUsername != newUsername)
        {
            SellerUsername = newUsername;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateWinnerUsername(string newUsername)
    {

        if (WinnerUsername != newUsername)
        {
            WinnerUsername = newUsername;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
