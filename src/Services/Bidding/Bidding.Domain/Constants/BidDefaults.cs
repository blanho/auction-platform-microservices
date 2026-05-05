namespace Bidding.Domain.Constants;

public static class BidDefaults
{
    public const int DefaultDaysForStats = 30;
    public const int DefaultTopBiddersLimit = 10;

    public const int AntiSnipeThresholdMinutes = 2;
    public const int AntiSnipeExtensionMinutes = 2;

    public const int BidLockTimeoutSeconds = 10;
    public const int AutoBidLockExpirySeconds = 30;
    public const int AutoBidLockWaitSeconds = 10;

    public const int DeduplicationWindowSeconds = 5;
    public const string DeduplicationKeyPrefix = "bid:dedup:";

    public const int BidRateLimitPerSecond = 5;
    public const int AutoBidRateLimitPerMinute = 10;
    public const int ApiRateLimitPerMinute = 100;

    public const int MaxAutoBidRecursionDepth = 10;
    public const int MaxActiveBidsPerUser = 100;

    public const int RetractWindowMinutes = 5;

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public const decimal MaxBidAmount = 10_000_000;

    public const int MaxDaysForStats = 365;
    public const int MaxTopBiddersLimit = 100;

    public static class Database
    {
        public const int RetryCount = 3;

        public const int MaxRetryDelaySeconds = 30;

        public const int CommandTimeoutSeconds = 30;
    }

    public static class Grpc
    {
        public const int PooledConnectionIdleTimeoutMinutes = 5;

        public const int KeepAlivePingDelaySeconds = 60;

        public const int KeepAlivePingTimeoutSeconds = 30;

        public const int ConnectTimeoutSeconds = 5;

        public const int MaxRetryAttempts = 3;

        public const int InitialBackoffMs = 500;

        public const int MaxBackoffSeconds = 5;

        public const double BackoffMultiplier = 2.0;
    }

    public static class RateLimit
    {
        public const int BiddingPermitLimit = 10;

        public const int BiddingWindowSeconds = 10;

        public const int BiddingSegmentsPerWindow = 2;

        public const int GlobalPermitLimit = 100;

        public const int GlobalWindowMinutes = 1;

        public const int GlobalSegmentsPerWindow = 4;

        public const int DefaultRetryAfterSeconds = 10;
    }
}
