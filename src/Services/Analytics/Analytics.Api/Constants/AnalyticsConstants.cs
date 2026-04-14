namespace Analytics.Api.Constants;

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
