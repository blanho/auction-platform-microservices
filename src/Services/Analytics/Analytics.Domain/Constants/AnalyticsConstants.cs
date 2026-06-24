namespace Analytics.Domain.Constants;

public static class AnalyticsEventTypes
{
    public const string Created = "Created";
    public const string Started = "Started";
    public const string Finished = "Finished";
    public const string BuyNowExecuted = "BuyNowExecuted";
    public const string OrderCreated = "OrderCreated";
    public const string OrderShipped = "OrderShipped";
    public const string OrderDelivered = "OrderDelivered";
    public const string PaymentCompleted = "PaymentCompleted";
    public const string Registered = "Registered";
}

public static class AnalyticsAuctionStatuses
{
    public const string Live = "Live";
    public const string Sold = "Sold";
    public const string Ended = "Ended";
    public const string Created = "Created";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Paid = "Paid";
}

public static class AnalyticsBidStatuses
{
    public const string Accepted = "Accepted";
    public const string AcceptedBelowReserve = "AcceptedBelowReserve";
    public const string Rejected = "Rejected";
    public const string Retracted = "Retracted";
    public const string TooLow = "TooLow";
}

public static class AnalyticsDefaults
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    public const int DefaultDays = 30;
    public const int MaxDays = 365;
    public const int DefaultLimit = 10;
    public const string DefaultPeriod = "week";
    public const string DefaultGranularity = "day";
    
    // String Lengths
    public const int UsernameLength = 50;
    public const int CategoryLength = 100;
    public const int DescriptionLength = 1000;
    public const int ShortDescriptionLength = 200;
    public const int ReasonLength = 500;
    public const int IpAddressLength = 45;
    public const int ActionLength = 100;
    public const int EntityTypeLength = 100;
    public const int StatusLength = 50;
    public const int TypeLength = 50;
    public const int KeyLength = 100;
    public const int ValueLength = 2000;


    public static class Messaging
    {
        public const int OutboxQueryDelaySeconds = 10;
        public const int StandardPrefetch = 16;
        public const int StandardConcurrency = 8;
        public const int BidPrefetch = 128;
        public const int BidConcurrency = 32;
        public const int BidBatchMessageLimit = 100;
        public const int BidBatchTimeLimitSeconds = 1;
        public const int RetryLimit = 5;
        public const int MinIntervalMs = 100;
        public const int MaxIntervalSeconds = 30;
        public const int IntervalDeltaMs = 200;
        public const int RedeliveryFastSeconds = 5;
        public const int RedeliverySlowSeconds = 30;
        public const int RedeliveryMaxMinutes = 2;
    }

    public static class Persistence
    {
        public const int MoneyPrecision = 18;
        public const int MoneyScale = 2;
        public const int DurationPrecision = 10;
        public const int DurationScale = 2;
        public const int TitleMaxLength = 500;
        public const int UsernameMaxLength = 100;
        public const int EmailMaxLength = 256;
        public const int EntityTypeMaxLength = 256;
        public const int CorrelationIdMaxLength = 128;
        public const int IpAddressMaxLength = 64;
        public const int CategoryNameMaxLength = 200;
        public const int StatusMaxLength = 30;
        public const int ConditionMaxLength = 50;
        public const int CurrencyMaxLength = 3;
        public const int ReasonMaxLength = 500;
        public const int LongTextMaxLength = 2000;
        public const int ValueMaxLength = 4000;
        public const int DescriptionMaxLength = 1000;
    }

    public static class Database
    {
        public const int RetryCount = 3;
        public const int MaxRetryDelaySeconds = 15;
        public const int CommandTimeoutSeconds = 30;
    }
}
