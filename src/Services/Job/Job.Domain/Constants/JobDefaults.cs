namespace Jobs.Domain.Constants;

public static class JobDefaults
{
    public const int DefaultMaxRetryCount = 3;

    public static class Worker
    {
        public const int PollingIntervalSeconds = 10;
        public const int StuckJobTimeoutMinutes = 30;
        public const int MaxPollingIntervalMinutes = 2;
        public const int ProcessingBatchSize = 5;
        public const int HealthTimeoutMinutes = 5;
        public const int MaxBackoffMultiplier = 6;
        public const double BackoffBase = 1.5;
        public const int MinConsecutiveEmptyPollsBeforeBackoff = 1;
    }

    public static class Outbox
    {
        public const int QueryDelaySeconds = 10;
    }

    public static class Connection
    {
        public const int RequestTimeoutSeconds = 30;
        public const int ContinuationTimeoutSeconds = 20;
        public const int MaxRetryDelaySeconds = 30;
        public const int RetryCount = 3;
        public const int CommandTimeoutSeconds = 30;
    }

    public static class Messaging
    {
        public const int StandardRetryLimit = 3;
        public const int HighThroughputRetryLimit = 5;
        public const int StandardMinIntervalSeconds = 1;
        public const int HighThroughputMinIntervalMs = 200;
        public const int MaxIntervalSeconds = 30;
        public const int IntervalDeltaSeconds = 5;
        public const int PrefetchCountItemResults = 16;
        public const int PrefetchCountItemBatches = 4;
        public const int ConcurrentMessageLimitItemBatches = 2;
        public const int PrefetchCountBatchResults = 8;
        public const int PrefetchCountBatchProgress = 8;
        public const int RedeliveryFastSeconds = 5;
        public const int RedeliverySlowSeconds = 30;
        public const int RedeliveryMaxMinutes = 2;
        public const int GlobalRetryLimit = 5;
        public const int GlobalMinIntervalMs = 200;
    }

    public static class Dispatcher
    {
        public const int FetchBatchSize = 100;
        public const int PublishBatchSize = 50;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultJobPageSize = 20;
        public const int DefaultItemPageSize = 50;
        public const int DefaultHistoryPageSize = 50;
    }

    public static class Validation
    {
        public const int CorrelationIdMaxLength = 256;
        public const int MaxRetryCountUpperBound = 10;
    }

    public static class Persistence
    {
        public const int BulkInsertBatchSize = 1000;
        public const int CorrelationIdMaxLength = 256;
        public const int ErrorMessageMaxLength = 4000;
        public const int ProgressPercentagePrecision = 5;
        public const int ProgressPercentageScale = 2;
        public const int ProgressDecimalPlaces = 2;
    }
}
