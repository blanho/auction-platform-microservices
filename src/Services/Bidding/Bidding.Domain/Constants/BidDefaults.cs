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
}
