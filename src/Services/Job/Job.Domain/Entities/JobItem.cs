using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using Jobs.Domain.Enums;

namespace Jobs.Domain.Entities;

public class JobItem : BaseEntity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "S1144", Justification = "EF Core refreshes this PostgreSQL xmin concurrency token after materialization and writes.")]
    public uint RowVersion { get; private set; }
    public Guid JobId { get; private set; }
    public Job Job { get; private set; } = null!;
    public int SequenceNumber { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public JobItemStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public string[] ErrorArguments { get; private set; } = [];
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private JobItem() { }

    public static JobItem Create(
        Guid jobId,
        string payloadJson,
        int sequenceNumber,
        int maxRetryCount)
    {
        return new JobItem
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            PayloadJson = payloadJson,
            SequenceNumber = sequenceNumber,
            Status = JobItemStatus.Pending,
            RetryCount = 0,
            MaxRetryCount = maxRetryCount,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkProcessing()
    {
        if (Status != JobItemStatus.Pending)
            throw new InvalidEntityStateException(nameof(JobItem), Status.ToString(),
                "Item can only start processing from Pending status.");

        Status = JobItemStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status != JobItemStatus.Processing)
            throw new InvalidEntityStateException(nameof(JobItem), Status.ToString(),
                "Item can only be completed from Processing status.");

        Status = JobItemStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
        ErrorCode = null;
        ErrorArguments = [];
    }

    public void MarkFailed(string errorMessage, bool terminal = false,
        string? errorCode = null, string[]? errorArguments = null)
    {
        if (Status != JobItemStatus.Processing)
            throw new InvalidEntityStateException(nameof(JobItem), Status.ToString(),
                "Item can only be failed from Processing status.");

        ErrorMessage = errorMessage;
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode;
        ErrorArguments = ErrorCode is null ? [] : errorArguments?.ToArray() ?? [];
        RetryCount++;

        if (terminal || RetryCount >= MaxRetryCount)
        {
            Status = JobItemStatus.Failed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = JobItemStatus.Pending;
            StartedAt = null;
        }
    }

    public void Skip(string reason)
    {
        Status = JobItemStatus.Skipped;
        ErrorMessage = reason;
        ErrorCode = null;
        ErrorArguments = [];
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        if (Status != JobItemStatus.Failed)
            throw new InvalidEntityStateException(nameof(JobItem), Status.ToString(),
                "Item can only be reset from Failed status.");

        Status = JobItemStatus.Pending;
        ErrorMessage = null;
        ErrorCode = null;
        ErrorArguments = [];
        StartedAt = null;
        CompletedAt = null;
    }

    public bool CanRetry => RetryCount < MaxRetryCount && Status == JobItemStatus.Pending;

    public bool IsTerminal =>
        Status is JobItemStatus.Completed or JobItemStatus.Failed or JobItemStatus.Skipped;
}
