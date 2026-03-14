using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobHistory;

public record GetJobHistoryQuery(
    Guid JobId,
    int Page = 1,
    int PageSize = 50,
    JobLogLevel? LogLevel = null) : IQuery<JobHistoryDto>;
