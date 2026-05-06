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

        public const int CheckFinishedIntervalSeconds = 5;

        public const int DeactivationIntervalMinutes = 1;

        public const int ActivationIntervalSeconds = 30;

        public const int EndingSoonNotificationIntervalMinutes = 1;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;

        public const int MaxPageSize = 100;
    }
    public static class Batch
    {
        public const int InsertBatchSize = 500;

        public const int SaveBatchSize = 100;

        public const int FetchBatchSize = 200;

        public const int MaxImportRowsPerRequest = 10_000;

        public const int RowsPerImportBatch = 1_000;

        public const int MaxBulkUpdateSize = 5_000;

        public const int MaxCategoriesPerQuery = 100;
    }
    public static class Messaging
    {
        public const int OutboxQueryDelaySeconds = 10;

        public const int ConnectionTimeoutSeconds = 30;

        public const int ContinuationTimeoutSeconds = 20;

        public const int StandardRetryLimit = 3;

        public const int HighThroughputRetryLimit = 5;

        public const int StandardMinIntervalSeconds = 1;

        public const int HighThroughputMinIntervalMs = 200;

        public const int MaxIntervalSeconds = 30;

        public const int IntervalDeltaSeconds = 5;

        public const int RedeliveryFastSeconds = 5;

        public const int RedeliverySlowSeconds = 30;

        public const int RedeliveryMaxMinutes = 2;
    }

    public static class Persistence
    {
        public const int MoneyPrecision = 18;

        public const int MoneyScale = 2;

        public const int CurrencyCodeMaxLength = 3;

        public const int UsernameMaxLength = 256;

        public const int ItemTitleMaxLength = 200;

        public const int ItemDescriptionMaxLength = 4000;

        public const int ItemConditionMaxLength = 50;

        public const int CategoryNameMaxLength = 100;

        public const int CategorySlugMaxLength = 100;

        public const int CategoryIconMaxLength = 50;

        public const int CategoryDescriptionMaxLength = 500;

        public const int BrandNameMaxLength = 100;

        public const int BrandSlugMaxLength = 100;

        public const int BrandDescriptionMaxLength = 1000;

        public const int ReviewTitleMaxLength = 200;

        public const int ReviewCommentMaxLength = 2000;

        public const int ReviewSellerResponseMaxLength = 1000;
    }
}
