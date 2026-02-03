namespace BuildingBlocks.Application.BackgroundJobs.Core;

public enum BackgroundJobStatus
{
    Pending = 0,
    Queued = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum BackgroundJobPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public sealed class BackgroundJobState
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Username { get; private init; }
    public string JobType { get; }
    public string Name { get; private init; }
    public BackgroundJobPriority Priority { get; }
    public Dictionary<string, object> Metadata { get; }
    public DateTimeOffset CreatedAt { get; }

    public BackgroundJobStatus Status { get; private set; }
    public int TotalItems { get; private set; }
    public int ProcessedItems { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; }
    public int EstimatedSecondsRemaining { get; private set; }
    public string? ResultUrl { get; private set; }
    public string? ResultFileName { get; private set; }
    public long? ResultFileSizeBytes { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? CheckpointData { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }

    public double ProgressPercentage => TotalItems > 0
        ? Math.Round((double)ProcessedItems / TotalItems * 100, 1)
        : 0;

    public bool CanRetry => RetryCount < MaxRetries && Status == BackgroundJobStatus.Failed;

    public BackgroundJobState(
        string jobType,
        Guid userId,
        Dictionary<string, object>? metadata = null,
        BackgroundJobPriority priority = BackgroundJobPriority.Normal,
        int maxRetries = 3)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Username = string.Empty;
        JobType = jobType;
        Name = jobType;
        Priority = priority;
        Metadata = metadata ?? new Dictionary<string, object>();
        MaxRetries = maxRetries;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = BackgroundJobStatus.Pending;
    }

    public static BackgroundJobState Create(
        Guid userId,
        string username,
        string jobType,
        string name,
        BackgroundJobPriority priority = BackgroundJobPriority.Normal,
        Dictionary<string, object>? metadata = null,
        int maxRetries = 3)
    {
        return new BackgroundJobState(jobType, userId, metadata, priority, maxRetries)
        {
            Username = username,
            Name = name
        };
    }

    public void MarkQueued()
    {
        Status = BackgroundJobStatus.Queued;
    }

    public void MarkRunning()
    {
        Status = BackgroundJobStatus.Running;
        StartedAt ??= DateTimeOffset.UtcNow;
    }

    public void UpdateProgress(int processedItems, int totalItems, int? estimatedSecondsRemaining = null)
    {
        ProcessedItems = processedItems;
        TotalItems = totalItems;
        if (estimatedSecondsRemaining.HasValue)
            EstimatedSecondsRemaining = estimatedSecondsRemaining.Value;
    }

    public void UpdateProgress(int progressPercentage, string? statusMessage)
    {
        TotalItems = 100;
        ProcessedItems = progressPercentage;
    }

    public void SetCheckpoint(string checkpointData)
    {
        CheckpointData = checkpointData;
    }

    public void SetResultFileName(string resultFileName)
    {
        ResultFileName = resultFileName;
    }

    public void MarkCompleted(string? resultUrl = null, string? resultFileName = null, long? resultFileSizeBytes = null)
    {
        Status = BackgroundJobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        ResultUrl = resultUrl;
        ResultFileName = resultFileName;
        ResultFileSizeBytes = resultFileSizeBytes;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = BackgroundJobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;

        if (CanRetry)
        {
            var backoffSeconds = Math.Pow(2, RetryCount) * 10;
            NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(backoffSeconds);
        }
    }

    public void MarkCancelled()
    {
        Status = BackgroundJobStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        Status = BackgroundJobStatus.Pending;
        ErrorMessage = null;
        NextRetryAt = null;
    }

    public TimeSpan CalculateRetryDelay()
    {
        var backoffSeconds = Math.Pow(2, RetryCount) * 10;
        return TimeSpan.FromSeconds(backoffSeconds);
    }
}
