namespace Common.Resilience;

public class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public RetryOptions Retry { get; set; } = new();
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
    public TimeoutOptions Timeout { get; set; } = new();
    public BulkheadOptions Bulkhead { get; set; } = new();
}

public class RetryOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public double BaseDelaySeconds { get; set; } = 2.0;
    public double MaxDelaySeconds { get; set; } = 30.0;
    public bool UseJitter { get; set; } = true;
}

public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;
    public int SamplingDurationSeconds { get; set; } = 30;
    public int DurationOfBreakSeconds { get; set; } = 30;
    public int MinimumThroughput { get; set; } = 10;
}

public class TimeoutOptions
{
    public int TimeoutSeconds { get; set; } = 10;
    public int LongRunningTimeoutSeconds { get; set; } = 30;
}

public class BulkheadOptions
{
    public int MaxConcurrentCalls { get; set; } = 100;
    public int MaxQueuedCalls { get; set; } = 50;
}
