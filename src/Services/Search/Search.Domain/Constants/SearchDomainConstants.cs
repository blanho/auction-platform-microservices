namespace Search.Domain.Constants;

public static class AuctionStatuses
{
    public const string Sold = "Sold";
    public const string Finished = "Finished";
}

public static class DateTimeFormats
{
    public const string Iso8601 = "o";
}

public static class SortDirections
{
    public const string Ascending = "asc";
    public const string Descending = "desc";
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
