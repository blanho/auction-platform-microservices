namespace Storage.Domain.Constants;

public static class StorageDefaults
{
    public static class Cleanup
    {
        public const int BatchSize = 100;

        public const int SoftDeleteRetentionDays = 30;

        public const int UnassociatedFileThresholdHours = 24;
    }

    public static class Messaging
    {
        public const int OutboxQueryDelaySeconds = 1;

        public const int OutboxQueryTimeoutSeconds = 30;

        public const int HeartbeatSeconds = 30;

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

    public static class Database
    {
        public const int RetryCount = 3;

        public const int MaxRetryDelaySeconds = 30;

        public const int CommandTimeoutSeconds = 30;
    }

    public static class Persistence
    {
        public const int FileNameMaxLength = 255;

        public const int StoredFileNameMaxLength = 500;

        public const int ContentTypeMaxLength = 100;

        public const int UrlMaxLength = 2048;

        public const int SubFolderMaxLength = 255;

        public const int ChecksumMaxLength = 128;
    }

    public static class Validation
    {
        public const int FileNameMaxLength = 255;
    }
}
