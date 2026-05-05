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

        public const int DefaultBatchSize = 100;
    }

    public static class Database
    {
        public const int RetryCount = 3;

        public const int MaxRetryDelaySeconds = 30;

        public const int CommandTimeoutSeconds = 30;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;
    }

    public static class Template
    {
        public const int KeyMaxLength = 100;

        public const int NameMaxLength = 200;

        public const int SubjectMaxLength = 500;

        public const int BodyMaxLength = 10000;

        public const int DescriptionMaxLength = 1000;

        public const int SmsBodyMaxLength = 160;

        public const int PushTitleMaxLength = 100;

        public const int PushBodyMaxLength = 500;
    }
}
