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
        public const int DefaultPage = 1;

        public const int DefaultPageSize = 20;

        public const int RecentNotificationsCount = 10;
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

    public static class Column
    {
        public const int UserIdMaxLength = 450;

        public const int UsernameMaxLength = 256;

        public const int TitleMaxLength = 200;

        public const int MessageMaxLength = 2000;

        public const int LinkMaxLength = 500;

        public const int NotificationTypeMaxLength = 50;

        public const int ReferenceIdMaxLength = 100;

        public const int ChannelMaxLength = 50;

        public const int ErrorMessageMaxLength = 1000;

        public const int ExternalIdMaxLength = 100;
    }

    public static class Phone
    {
        public const int MinDigitCount = 10;

        public const int MaxDigitCount = 15;

        public const int NorthAmericanDigitCount = 10;

        public const int NorthAmericanWithCountryCodeDigitCount = 11;

        public const int MinMaskableLength = 6;
    }

    public static class Validation
    {
        public const int MinUserIdLength = 3;
    }

    public static class Idempotency
    {
        public const int DefaultTtlHours = 24;

        public const int LockTimeoutMinutes = 5;

        public const int RateLimitWindowSeconds = 60;

        public const int RateLimitMaxCount = 10;
    }

    public static class Push
    {
        public const int BadgeCount = 1;
    }

    public static class Transport
    {
        public const int OutboxQueryDelaySeconds = 1;

        public const int OutboxQueryTimeoutSeconds = 30;

        public const int HeartbeatSeconds = 30;

        public const int GlobalRetryCount = 3;

        public const int GlobalRetryInitialSeconds = 1;

        public const int GlobalRetryDeltaSeconds = 2;

        public const int BulkConsumerPrefetchCount = 1;

        public const int BulkConsumerConcurrencyLimit = 2;

        public const int AuctionEndingSoonConcurrencyLimit = 1;
    }

    public static class Redelivery
    {
        public const int StandardInitialSeconds = 5;

        public const int StandardSecondSeconds = 30;

        public const int StandardThirdMinutes = 5;

        public const int StandardFourthMinutes = 30;

        public const int StandardFifthHours = 1;

        public const int BulkFirstMinutes = 1;

        public const int BulkSecondMinutes = 5;

        public const int BulkThirdMinutes = 15;
    }

    public static class Message
    {
        public const int DefaultTruncateLength = 100;

        public const int RelativeTimeMinutesPerHour = 60;

        public const int RelativeTimeMinutesPerDay = 1440;

        public const int RelativeTimeMinutesPerWeek = 10080;
    }
}
