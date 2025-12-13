namespace Common.Caching.Keys;

public static class CacheKeys
{
    public const string AuctionPrefix = "auction:";
    public const string UserPrefix = "user:";
    
    public static string Auction(Guid id) => $"{AuctionPrefix}{id}";
    public static string AuctionList(string filter = "all") => $"{AuctionPrefix}list:{filter}";
    public static string User(string userId) => $"{UserPrefix}{userId}";
}
