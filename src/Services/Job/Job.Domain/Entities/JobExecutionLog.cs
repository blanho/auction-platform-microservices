using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using Jobs.Domain.Enums;

namespace Jobs.Domain.Entities;

public class JobExecutionLog : BaseEntity
{
    public Guid JobId { get; private set; }
    public Job Job { get; private set; } = null!;
    public JobLogLevel LogLevel { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public JobStatus? PreviousStatus { get; private set; }
    public JobStatus? NewStatus { get; private set; }
    public ValueObjects.ExecutionContext? Context { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    private JobExecutionLog() { }

    public static JobExecutionLog CreateStateTransition(
        Guid jobId,
        JobStatus previousStatus,
        JobStatus newStatus,
        string message,
        ValueObjects.ExecutionContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainInvariantException("Log message is required.");

        return new JobExecutionLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            LogLevel = JobLogLevel.StateTransition,
            Message = message,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Context = context,
            Timestamp = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static JobExecutionLog CreateInformation(
        Guid jobId,
        string message,
        ValueObjects.ExecutionContext? context = null,
        TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainInvariantException("Log message is required.");

        return new JobExecutionLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            LogLevel = JobLogLevel.Information,
            Message = message,
            Context = context,
            Duration = duration,
            Timestamp = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static JobExecutionLog CreateWarning(
        Guid jobId,
        string message,
        ValueObjects.ExecutionContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainInvariantException("Log message is required.");

        return new JobExecutionLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            LogLevel = JobLogLevel.Warning,
            Message = message,
            Context = context,
            Timestamp = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static JobExecutionLog CreateError(
        Guid jobId,
        string message,
        ValueObjects.ExecutionContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainInvariantException("Log message is required.");

        return new JobExecutionLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            LogLevel = JobLogLevel.Error,
            Message = message,
            Context = context,
            Timestamp = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
