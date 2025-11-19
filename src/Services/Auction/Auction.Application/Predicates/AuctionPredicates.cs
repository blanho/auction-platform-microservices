using System.Linq.Expressions;
using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Application.Predicates;

public static class AuctionPredicates
{
    public static Expression<Func<Auction, bool>> IsActive =>
        a => !a.IsDeleted;

    public static Expression<Func<Auction, bool>> IsLive =>
        a => !a.IsDeleted && a.Status == Status.Live;

    public static Expression<Func<Auction, bool>> IsSold =>
        a => !a.IsDeleted && a.Status == Status.Finished && a.SoldAmount.HasValue;

    public static Expression<Func<Auction, bool>> IsFinished =>
        a => !a.IsDeleted && a.Status == Status.Finished;

    public static Expression<Func<Auction, bool>> HasStatus(Status status) =>
        a => !a.IsDeleted && a.Status == status;

    public static Expression<Func<Auction, bool>> BySeller(Guid sellerId) =>
        a => !a.IsDeleted && a.SellerId == sellerId;

    public static Expression<Func<Auction, bool>> BySellerUsername(string username) =>
        a => !a.IsDeleted && a.SellerUsername == username;

    public static Expression<Func<Auction, bool>> EndingBetween(DateTimeOffset start, DateTimeOffset end) =>
        a => !a.IsDeleted && a.Status == Status.Live && a.AuctionEnd >= start && a.AuctionEnd < end;

    public static Expression<Func<Auction, bool>> ContainsIds(List<Guid> ids) =>
        a => !a.IsDeleted && ids.Contains(a.Id);
}

