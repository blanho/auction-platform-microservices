namespace Notification.Domain.Constants;

public static class NotificationDefaults
{
    public static class Messaging
    {
        public const int RetryLimit = 5;

        public const int MinIntervalSeconds = 5;

        public const int MaxIntervalMinutes = 5;

        public const int IntervalDeltaSeconds = 10;
    }

    public static class LightRetry
    {
        public const int RetryLimit = 3;

        public const int MinIntervalSeconds = 2;

        public const int MaxIntervalMinutes = 1;

        public const int IntervalDeltaSeconds = 5;
    }

    public static class BulkRetry
    {
        public const int RetryLimit = 3;

        public const int MinIntervalSeconds = 10;

        public const int MaxIntervalMinutes = 5;

        public const int IntervalDeltaSeconds = 30;
    }

    public static class Bulk
    {
        public const int MaxRecipients = 10000;

        public const int MinBatchSize = 10;

        public const int MaxBatchSize = 500;
    }
}
