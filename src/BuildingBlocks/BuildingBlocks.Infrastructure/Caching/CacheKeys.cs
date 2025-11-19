namespace BuildingBlocks.Infrastructure.Caching;

public static class CacheKeys
{
    public const string AuctionPrefix = "auction:";
    public const string UserPrefix = "user:";
    public const string StatsPrefix = "stats:";

    public static string Auction(Guid id) => $"{AuctionPrefix}{id}";

    public static string AuctionList(string filter = "all") => $"{AuctionPrefix}list:{filter}";

    public static string AuctionsByIds(IEnumerable<Guid> ids) =>
        $"{AuctionPrefix}batch:{string.Join(",", ids.OrderBy(x => x).Take(10))}";

    public static string LiveAuctionCount() => $"{StatsPrefix}live-count";
    public static string EndingSoonCount() => $"{StatsPrefix}ending-soon-count";
    public static string TotalAuctionCount() => $"{StatsPrefix}total-count";
    public static string AuctionCountByStatus(string status) => $"{StatsPrefix}count:{status}";

    public static string TotalRevenue() => $"{StatsPrefix}total-revenue";
    public static string CategoryStats() => $"{StatsPrefix}category-stats";
    public static string TrendingItems(int limit) => $"{AuctionPrefix}trending:{limit}";
    public static string TopByRevenue(int limit) => $"{AuctionPrefix}top-revenue:{limit}";

    public static string User(string userId) => $"{UserPrefix}{userId}";
    public static string UserBookmarks(Guid userId) => $"{UserPrefix}{userId}:bookmarks";
}
