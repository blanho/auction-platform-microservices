namespace Orchestration.Sagas;

public static class SagaConstants
{
    public const int MaxCompensationRetries = 5;
    public const int MaxStepRetries = 3;

    public static readonly TimeSpan AuctionCompletionTimeout = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan BuyNowTimeout = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(30);

    public static class Workflows
    {
        public const string BuyNow = "BuyNow";
        public const string AuctionCompletion = "AuctionCompletion";
    }

    public static class RecoverySteps
    {
        public const string ReleaseReservation = "ReleaseReservation";
        public const string CancelOrder = "CancelOrder";
        public const string ConfirmOrder = "ConfirmOrder";
        public const string CreateOrder = "CreateOrder";
        public const string Notifications = "Notifications";
    }

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
