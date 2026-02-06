using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using Jobs.Domain.Enums;
using Jobs.Domain.Events;

namespace Jobs.Domain.Entities;

public class Job : BaseEntity
{
    public JobType Type { get; private set; }
    public JobStatus Status { get; private set; }
    public JobPriority Priority { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public int TotalItems { get; private set; }
    public int CompletedItems { get; private set; }
    public int FailedItems { get; private set; }
    public int MaxRetryCount { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public Guid RequestedBy { get; private set; }

    private readonly List<JobItem> _items = new();
    public IReadOnlyCollection<JobItem> Items => _items.AsReadOnly();

    private Job() { }

    public static Job Create(
        JobType type,
        string correlationId,
        string payloadJson,
        Guid requestedBy,
        int totalItems,
        int maxRetryCount = 3,
        JobPriority priority = JobPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new DomainInvariantException("CorrelationId is required for job creation.");

        if (totalItems <= 0)
            throw new DomainInvariantException("TotalItems must be greater than zero.");

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = JobStatus.Pending,
            Priority = priority,
            CorrelationId = correlationId,
            PayloadJson = payloadJson,
            TotalItems = totalItems,
            CompletedItems = 0,
            FailedItems = 0,
            MaxRetryCount = maxRetryCount,
            ProgressPercentage = 0,
            RequestedBy = requestedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };

        job.AddDomainEvent(new JobCreatedDomainEvent(
            job.Id, job.Type, job.CorrelationId, job.TotalItems, job.RequestedBy));

        return job;
    }

    public static Job CreateStreaming(
        JobType type,
        string correlationId,
        string payloadJson,
        Guid requestedBy,
        int maxRetryCount = 3,
        JobPriority priority = JobPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new DomainInvariantException("CorrelationId is required for job creation.");

        return new Job
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = JobStatus.Initializing,
            Priority = priority,
            CorrelationId = correlationId,
            PayloadJson = payloadJson,
            TotalItems = 0,
            CompletedItems = 0,
            FailedItems = 0,
            MaxRetryCount = maxRetryCount,
            ProgressPercentage = 0,
            RequestedBy = requestedBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public JobItem AddItem(string payloadJson, int sequenceNumber)
    {
        EnsureNotTerminal();

        var item = JobItem.Create(Id, payloadJson, sequenceNumber, MaxRetryCount);
        _items.Add(item);
        return item;
    }

    public void AddItems(IEnumerable<(string PayloadJson, int SequenceNumber)> itemPayloads)
    {
        EnsureNotTerminal();

        foreach (var (payloadJson, sequenceNumber) in itemPayloads)
        {
            var item = JobItem.Create(Id, payloadJson, sequenceNumber, MaxRetryCount);
            _items.Add(item);
        }
    }

    public void Start()
    {
        if (Status != JobStatus.Pending)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Job can only be started from Pending status.");

        Status = JobStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementTotalItems(int count)
    {
        if (Status != JobStatus.Initializing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Can only increment total items during initialization.");

        if (count <= 0)
            throw new DomainInvariantException("Increment count must be greater than zero.");

        TotalItems += count;
    }

    public void FinalizeInitialization()
    {
        if (Status != JobStatus.Initializing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Can only finalize a job that is being initialized.");

        if (TotalItems <= 0)
            throw new DomainInvariantException("Cannot finalize a job with zero items.");

        Status = JobStatus.Pending;

        AddDomainEvent(new JobCreatedDomainEvent(
            Id, Type, CorrelationId, TotalItems, RequestedBy));
    }

    public void RecordItemCompleted()
    {
        if (Status != JobStatus.Processing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot record item completion on a non-processing job.");

        CompletedItems++;
        RecalculateProgress();
        CheckForCompletion();
    }

    public void RecordItemFailed()
    {
        if (Status != JobStatus.Processing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot record item failure on a non-processing job.");

        FailedItems++;
        RecalculateProgress();
        CheckForCompletion();
    }

    public void RecordBatchCompleted(int count)
    {
        if (Status != JobStatus.Processing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot record batch completion on a non-processing job.");

        if (count <= 0)
            throw new DomainInvariantException("Batch count must be greater than zero.");

        CompletedItems += count;
        RecalculateProgress();
        CheckForCompletion();
    }

    public void RecordBatchFailed(int count)
    {
        if (Status != JobStatus.Processing)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot record batch failure on a non-processing job.");

        if (count <= 0)
            throw new DomainInvariantException("Batch count must be greater than zero.");

        FailedItems += count;
        RecalculateProgress();
        CheckForCompletion();
    }

    public void Fail(string errorMessage)
    {
        if (IsTerminal())
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot fail a job that is already in a terminal state.");

        Status = JobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new JobFailedDomainEvent(Id, Type, CorrelationId, errorMessage));
    }

    public void Cancel()
    {
        if (IsTerminal())
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot cancel a job that is already in a terminal state.");

        Status = JobStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        if (Status != JobStatus.Failed && Status != JobStatus.CompletedWithErrors)
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Can only retry a failed or partially completed job.");

        Status = JobStatus.Pending;
        ErrorMessage = null;
        StartedAt = null;
        CompletedAt = null;
        FailedItems = 0;
        CompletedItems = 0;
        ProgressPercentage = 0;
    }

    private void RecalculateProgress()
    {
        if (TotalItems <= 0)
        {
            ProgressPercentage = 0;
            return;
        }

        var processedItems = CompletedItems + FailedItems;
        ProgressPercentage = Math.Round((decimal)processedItems / TotalItems * 100, 2);
    }

    private void CheckForCompletion()
    {
        var processedItems = CompletedItems + FailedItems;
        if (processedItems < TotalItems)
            return;

        CompletedAt = DateTimeOffset.UtcNow;

        if (FailedItems == 0)
        {
            Status = JobStatus.Completed;
        }
        else if (CompletedItems > 0)
        {
            Status = JobStatus.CompletedWithErrors;
        }
        else
        {
            Status = JobStatus.Failed;
            ErrorMessage = $"All {FailedItems} items failed.";
        }

        AddDomainEvent(new JobCompletedDomainEvent(
            Id, Type, CorrelationId, TotalItems, CompletedItems, FailedItems));
    }

    private bool IsTerminal() =>
        Status is JobStatus.Completed or JobStatus.CompletedWithErrors or JobStatus.Failed or JobStatus.Cancelled;

    private void EnsureNotTerminal()
    {
        if (IsTerminal())
            throw new InvalidEntityStateException(nameof(Job), Status.ToString(),
                "Cannot modify a job in a terminal state.");
    }
}
