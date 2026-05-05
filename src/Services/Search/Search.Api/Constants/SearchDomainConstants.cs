namespace Search.Api.Constants;

public static class BidStatuses
{
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Pending = "Pending";
    public const string Outbid = "Outbid";
}

public static class AuctionStatuses
{
    public const string Active = "Active";
    public const string Sold = "Sold";
    public const string Finished = "Finished";
    public const string Cancelled = "Cancelled";
    public const string Pending = "Pending";
}

public static class DateTimeFormats
{
    public const string Iso8601 = "o";
}

public static class MessagingDefaults
{
    public const int GlobalRetryLimit = 5;
    public const int RetryMinIntervalMs = 200;
    public const int RetryMaxIntervalSeconds = 30;
    public const int RetryIntervalDeltaSeconds = 5;

    public const int PrefetchCountLow = 16;
    public const int PrefetchCountMedium = 32;
    public const int PrefetchCountHigh = 64;

    public const int RedeliveryFastSeconds = 5;
    public const int RedeliverySlowSeconds = 30;
    public const int RedeliveryMaxMinutes = 2;
}

public static class IndexingDefaults
{
    public const int PartialUpdateRetryOnConflict = 3;
    public const int BidUpdateRetryOnConflict = 5;
}
