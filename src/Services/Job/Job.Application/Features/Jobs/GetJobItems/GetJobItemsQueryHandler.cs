using AutoMapper;
using Jobs.Application.Errors;

namespace Jobs.Application.Features.Jobs.GetJobItems;

public class GetJobItemsQueryHandler(
    IJobRepository jobRepository,
    IJobItemRepository jobItemRepository,
    IMapper mapper)
    : IQueryHandler<GetJobItemsQuery, PaginatedResult<JobItemDto>>
{
    public async Task<Result<PaginatedResult<JobItemDto>>> Handle(
        GetJobItemsQuery request,
        CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<PaginatedResult<JobItemDto>>(
                JobErrors.Job.NotFoundById(request.JobId));

        var paginatedItems = await jobItemRepository.GetPagedItemsByJobIdAsync(
            request.JobId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = mapper.Map<IReadOnlyList<JobItemDto>>(paginatedItems.Items);

        return Result<PaginatedResult<JobItemDto>>.Success(
            new PaginatedResult<JobItemDto>(
                items,
                paginatedItems.TotalCount,
                paginatedItems.Page,
                paginatedItems.PageSize));
    }
}
