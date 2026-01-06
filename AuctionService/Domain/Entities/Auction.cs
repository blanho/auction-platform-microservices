#nullable enable
using AuctionService.Domain.Events;
using Common.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Domain.Entities;

public class Auction : BaseEntity
{
    public decimal ReservePrice { get; set; } = 0;
    public decimal? BuyNowPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsBuyNowEnabled => BuyNowPrice.HasValue && BuyNowPrice > 0;
    public bool IsBuyNowAvailable => IsBuyNowEnabled && Status == Status.Live && !SoldAmount.HasValue;
    
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public Guid? WinnerId { get; set; }
    public string? WinnerUsername { get; set; }
    
    public decimal? SoldAmount { get; set; }
    public decimal? CurrentHighBid { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public Status Status { get; set; }
    public bool IsFeatured { get; set; } = false;
    public required Item Item { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<UserAuctionBookmark> Bookmarks { get; set; } = new List<UserAuctionBookmark>();

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
}
