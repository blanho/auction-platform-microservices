namespace Jobs.Domain.Constants;

public static class JobDefaults
{
    public static class Worker
    {
        public const int PollingIntervalSeconds = 10;

        public const int StuckJobTimeoutMinutes = 30;
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
    }
}
