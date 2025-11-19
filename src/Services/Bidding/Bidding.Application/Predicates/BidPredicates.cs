using System.Linq.Expressions;

namespace Bidding.Application.Predicates;

public static class BidPredicates
{
    public static Expression<Func<Bid, bool>> IsActive =>
        b => !b.IsDeleted;

    public static Expression<Func<Bid, bool>> IsAccepted =>
        b => !b.IsDeleted && b.Status == BidStatus.Accepted;

    public static Expression<Func<Bid, bool>> ByAuction(Guid auctionId) =>
        b => !b.IsDeleted && b.AuctionId == auctionId;

    public static Expression<Func<Bid, bool>> AcceptedByAuction(Guid auctionId) =>
        b => !b.IsDeleted && b.AuctionId == auctionId && b.Status == BidStatus.Accepted;

    public static Expression<Func<Bid, bool>> ByBidder(Guid bidderId) =>
        b => !b.IsDeleted && b.BidderId == bidderId;

    public static Expression<Func<Bid, bool>> ByBidderUsername(string username) =>
        b => !b.IsDeleted && b.BidderUsername == username;

    public static Expression<Func<Bid, bool>> PlacedAfter(DateTimeOffset startDate) =>
        b => !b.IsDeleted && b.BidTime >= startDate;

    public static Expression<Func<Bid, bool>> PlacedBetween(DateTimeOffset start, DateTimeOffset end) =>
        b => !b.IsDeleted && b.BidTime >= start && b.BidTime < end;
}

