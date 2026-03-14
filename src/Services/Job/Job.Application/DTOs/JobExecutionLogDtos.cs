using Jobs.Domain.Enums;

namespace Jobs.Application.DTOs;

public record JobExecutionLogDto
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public JobLogLevel LogLevel { get; init; }
    public string Message { get; init; } = string.Empty;
    public JobStatus? PreviousStatus { get; init; }
    public JobStatus? NewStatus { get; init; }
    public string? MachineName { get; init; }
    public string? OperationName { get; init; }
    public int? BatchNumber { get; init; }
    public int? BatchSize { get; init; }
    public double? DurationMs { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public record JobHistoryDto
{
    public Guid JobId { get; init; }
    public JobType Type { get; init; }
    public JobStatus CurrentStatus { get; init; }
    public int TotalLogEntries { get; init; }
    public IReadOnlyList<JobExecutionLogDto> Entries { get; init; } = [];
}
