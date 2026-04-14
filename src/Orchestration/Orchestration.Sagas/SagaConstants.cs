namespace Orchestration.Sagas;

public static class SagaConstants
{
    public const int MaxCompensationRetries = 5;
    public const int MaxStepRetries = 3;

    public static readonly TimeSpan AuctionCompletionTimeout = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan BuyNowTimeout = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(30);

    public static class States
    {
        public const string Initial = "Initial";
        public const string OrderCreated = "OrderCreated";
        public const string NotificationsSent = "NotificationsSent";
        public const string Completed = "Completed";
        public const string Compensating = "Compensating";
        public const string Failed = "Failed";
        public const string TimedOut = "TimedOut";
        public const string ManualInterventionRequired = "ManualInterventionRequired";
    }
}
