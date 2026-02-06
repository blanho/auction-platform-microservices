using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.CreateJob;

public record CreateJobCommand(
    JobType Type,
    string CorrelationId,
    string PayloadJson,
    Guid RequestedBy,
    int TotalItems,
    int MaxRetryCount,
    JobPriority Priority,
    List<CreateJobItemRequestDto> Items) : ICommand<JobDto>;
