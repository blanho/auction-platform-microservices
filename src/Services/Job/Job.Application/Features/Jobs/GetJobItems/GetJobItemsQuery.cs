using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobItems;

public record GetJobItemsQuery(
    Guid JobId,
    JobItemStatus? Status,
    int Page = JobDefaults.Pagination.DefaultPage,
    int PageSize = JobDefaults.Pagination.DefaultItemPageSize) : IQuery<PaginatedResult<JobItemDto>>;
