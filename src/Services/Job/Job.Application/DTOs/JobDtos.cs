using Jobs.Domain.Enums;

namespace Jobs.Application.DTOs;

public record JobDto
{
    public Guid Id { get; init; }
    public JobType Type { get; init; }
    public JobStatus Status { get; init; }
    public JobPriority Priority { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int FailedItems { get; init; }
    public decimal ProgressPercentage { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public Guid RequestedBy { get; init; }
}

public record JobSummaryDto
{
    public Guid Id { get; init; }
    public JobType Type { get; init; }
    public JobStatus Status { get; init; }
    public JobPriority Priority { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int FailedItems { get; init; }
    public decimal ProgressPercentage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}

public record JobItemDto
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public int SequenceNumber { get; init; }
    public JobItemStatus Status { get; init; }
    public int RetryCount { get; init; }
    public int MaxRetryCount { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}

public record CreateJobRequestDto
{
    public string Type { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int Priority { get; init; }
    public int MaxRetryCount { get; init; } = 3;
    public List<CreateJobItemRequestDto> Items { get; init; } = [];
}

public record CreateJobItemRequestDto
{
    public string PayloadJson { get; init; } = string.Empty;
    public int SequenceNumber { get; init; }
}

public record JobFilterDto
{
    public string? Type { get; init; }
    public string? Status { get; init; }
    public string? CorrelationId { get; init; }
    public Guid? RequestedBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
