using AutoMapper;
using Jobs.Application.Errors;
using Jobs.Domain.Entities;
using Job = Jobs.Domain.Entities.Job;

namespace Jobs.Application.Features.Jobs.CreateJob;

public class CreateJobCommandHandler : ICommandHandler<CreateJobCommand, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateJobCommandHandler> _logger;

    public CreateJobCommandHandler(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<JobDto>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var existingJob = await _jobRepository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existingJob is not null)
        {
            _logger.LogWarning("Job with CorrelationId {CorrelationId} already exists: {JobId}",
                request.CorrelationId, existingJob.Id);
            return Result.Failure<JobDto>(
                JobErrors.Job.DuplicateCorrelationId(request.CorrelationId));
        }

        var job = Job.Create(
            request.Type,
            request.CorrelationId,
            request.PayloadJson,
            request.RequestedBy,
            request.TotalItems,
            request.MaxRetryCount,
            request.Priority);

        await _jobRepository.CreateAsync(job, cancellationToken);

        if (request.Items.Count > 0)
        {
            var items = request.Items.Select(i => job.AddItem(i.PayloadJson, i.SequenceNumber)).ToList();
            await _jobItemRepository.AddRangeAsync(items, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created job {JobId} of type {JobType} with {TotalItems} items",
            job.Id, job.Type, job.TotalItems);

        return Result.Success(_mapper.Map<JobDto>(job));
    }
}
