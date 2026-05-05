using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobs;

public record GetJobsQuery(
    JobType? Type,
    JobStatus? Status,
    Guid? RequestedBy,
    int Page = JobDefaults.Pagination.DefaultPage,
    int PageSize = JobDefaults.Pagination.DefaultJobPageSize) : IQuery<PaginatedResult<JobSummaryDto>>;
