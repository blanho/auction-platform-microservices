using AuctionService.Domain.Entities;
using System.Linq.Expressions;

namespace AuctionService.Application.Specifications;

public static class AuctionSortingExtensions
{
    private static readonly Dictionary<string, Expression<Func<Auction, object>>> SortExpressions = new()
    {
        ["title"] = a => a.Item.Title,
        ["make"] = a => a.Item.Make,
        ["model"] = a => a.Item.Model,
        ["year"] = a => a.Item.Year,
        ["auctionend"] = a => a.AuctionEnd,
        ["createdat"] = a => a.CreatedAt,
        ["currenthighbid"] = a => a.CurrentHighBid ?? 0,
        ["reserveprice"] = a => a.ReversePrice,
        ["status"] = a => a.Status
    };

    public static IQueryable<Auction> ApplySorting(this IQueryable<Auction> query, string? orderBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return descending 
                ? query.OrderByDescending(a => a.AuctionEnd) 
                : query.OrderBy(a => a.AuctionEnd);

        var key = orderBy.ToLowerInvariant();
        
        if (!SortExpressions.TryGetValue(key, out var sortExpression))
            return descending 
                ? query.OrderByDescending(a => a.AuctionEnd) 
                : query.OrderBy(a => a.AuctionEnd);

        return descending 
            ? query.OrderByDescending(sortExpression) 
            : query.OrderBy(sortExpression);
    }

    public static IEnumerable<string> GetAvailableSortFields()
    {
        return SortExpressions.Keys;
    }
}
