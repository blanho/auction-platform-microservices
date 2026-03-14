using BuildingBlocks.Application.Abstractions;

namespace Bidding.Application.Features.Bids.PlaceBid;

public static class BidErrors
{
    public static readonly Error LockAcquisitionFailed = Error.Create(
        "Bid.LockFailed",
        "Another bid is being processed. Please try again.");

    public static readonly Error AuctionNotLive = Error.Create(
        "Bid.AuctionNotLive",
        "This auction is not currently accepting bids.");

    public static readonly Error AuctionEnded = Error.Create(
        "Bid.AuctionEnded",
        "This auction has ended.");

    public static readonly Error CannotBidOnOwnAuction = Error.Create(
        "Bid.OwnAuction",
        "You cannot bid on your own auction.");

    public static Error AuctionNotFound(Guid auctionId) => Error.Create(
        "Bid.AuctionNotFound",
        $"Auction {auctionId} was not found.");

    public static Error BidTooLow(decimal minimumBid, decimal increment) => Error.Create(
        "Bid.TooLow",
        $"Bid must be at least ${minimumBid:N2}. Minimum increment is ${increment:N2}.");
}
