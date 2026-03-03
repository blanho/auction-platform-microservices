namespace Auctions.Domain.Constants;

public static class AuctionDefaults
{
    public static class Lock
    {
        public const int ExpirySeconds = 30;

        public const int WaitSeconds = 5;

        public const int RetryDelayMilliseconds = 100;
    }

    public static class Cache
    {
        public const int SingleAuctionTtlMinutes = 10;

        public const int AuctionListTtlMinutes = 1;
    }

    public static class Scheduling
    {
        public const int DefaultRetryDelaySeconds = 5;

        public const int MaxRetryCount = 3;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;

        public const int MaxPageSize = 100;
    }
}
