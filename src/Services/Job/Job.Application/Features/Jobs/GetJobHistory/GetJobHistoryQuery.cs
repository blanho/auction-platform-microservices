using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobHistory;

public record GetJobHistoryQuery(
    Guid JobId,
    int Page = JobDefaults.Pagination.DefaultPage,
    int PageSize = JobDefaults.Pagination.DefaultHistoryPageSize,
    JobLogLevel? LogLevel = null) : IQuery<JobHistoryDto>;
