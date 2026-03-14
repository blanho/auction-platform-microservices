using BuildingBlocks.Application.Abstractions;

namespace Bidding.Application.Errors;

public static class BiddingErrors
{
    public static class Bid
    {
        public static Error NotFound => Error.Create("Bid.NotFound", "Bid not found");
        public static Error Unauthorized => Error.Create("Bid.Unauthorized", "You can only manage your own bids");
        public static Error AlreadyRejected => Error.Create("Bid.AlreadyRejected", "Cannot retract a rejected bid");
        public static Error RetractWindowExpired(int minutes) => LocalizableError.Localizable("Bid.RetractWindowExpired", $"Bids can only be retracted within {minutes} minutes of placement", minutes);
        public static Error PlaceFailed(string reason) => LocalizableError.Localizable("Bid.PlaceFailed", $"Bid placement failed: {reason}", reason);
        public static Error TooLow(decimal minimum) => LocalizableError.Localizable("Bid.TooLow", $"Bid must be at least {minimum:C}", minimum);
        public static Error AuctionNotLive => Error.Create("Bid.AuctionNotLive", "This auction is not accepting bids");
        public static Error SelfBidding => Error.Create("Bid.SelfBidding", "You cannot bid on your own auction");
        public static Error DuplicateBid => Error.Create("Bid.DuplicateBid", "Duplicate bid detected, please wait before placing another bid");
    }

    public static class AutoBid
    {
        public static Error NotFound => Error.Create("AutoBid.NotFound", "Auto-bid not found");
        public static Error Unauthorized => Error.Create("AutoBid.Unauthorized", "You can only manage your own auto-bids");
        public static Error AlreadyExists => Error.Create("AutoBid.AlreadyExists", "You already have an active auto-bid for this auction");
        public static Error AlreadyCancelled => Error.Create("AutoBid.AlreadyCancelled", "This auto-bid is already cancelled");
        public static Error Inactive => Error.Create("AutoBid.Inactive", "This auto-bid is no longer active");
        public static Error MaxAmountTooLow(decimal currentBid) => LocalizableError.Localizable("AutoBid.MaxAmountTooLow", $"Max amount must be higher than current bid ({currentBid:C})", currentBid);
        public static Error UpdateFailed(string reason) => LocalizableError.Localizable("AutoBid.UpdateFailed", $"Failed to update auto-bid: {reason}", reason);
        public static Error CreateFailed(string reason) => LocalizableError.Localizable("AutoBid.CreateFailed", $"Failed to create auto-bid: {reason}", reason);
    }

    public static class Auction
    {
        public static Error NotFound => Error.Create("Auction.NotFound", "Auction not found");
        public static Error NotActive => Error.Create("Auction.NotActive", "Auction is not active");
        public static Error AlreadyEnded => Error.Create("Auction.AlreadyEnded", "Auction has already ended");
    }
}
