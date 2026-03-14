using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobs;

public record GetJobsQuery(
    JobType? Type,
    JobStatus? Status,
    Guid? RequestedBy,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<JobSummaryDto>>;
