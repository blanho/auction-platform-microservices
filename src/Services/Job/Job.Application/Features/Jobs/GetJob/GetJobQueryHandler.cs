using AutoMapper;
using Jobs.Application.Errors;

namespace Jobs.Application.Features.Jobs.GetJob;

public class GetJobQueryHandler : IQueryHandler<GetJobQuery, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public GetJobQueryHandler(IJobRepository jobRepository, IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<Result<JobDto>> Handle(GetJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<JobDto>(JobErrors.Job.NotFoundById(request.JobId));

        return Result.Success(_mapper.Map<JobDto>(job));
    }
}
