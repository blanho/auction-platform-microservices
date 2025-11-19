namespace Bidding.Application.Constants;

public static class BidDefaults
{
    public const int MaxBidDedupeDurationMinutes = 5;
    public const int MaxActiveBidsPerUser = 100;
    public const int DeduplicationWindowSeconds = 60;
    public const string DeduplicationKeyPrefix = "bid:dedup:";
    public const int BidLockTimeoutSeconds = 10;
    public const int AntiSnipeThresholdMinutes = 2;
    public const int AntiSnipeExtensionMinutes = 2;
    public const int AutoBidLockExpirySeconds = 30;
    public const int AutoBidLockWaitSeconds = 5;
}
