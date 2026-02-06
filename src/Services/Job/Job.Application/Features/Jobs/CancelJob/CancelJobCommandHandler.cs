using AutoMapper;
using Jobs.Application.Errors;
using JobStatus = Jobs.Domain.Enums.JobStatus;

namespace Jobs.Application.Features.Jobs.CancelJob;

public class CancelJobCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CancelJobCommandHandler> logger)
    : ICommandHandler<CancelJobCommand, JobDto>
{
    public async Task<Result<JobDto>> Handle(
        CancelJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdForUpdateAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<JobDto>(JobErrors.Job.NotFoundById(request.JobId));

        if (job.Status is not (JobStatus.Initializing or JobStatus.Pending or JobStatus.Processing))
            return Result.Failure<JobDto>(JobErrors.Job.CannotCancel(job.Status.ToString()));

        logger.LogInformation("Cancelling job {JobId} with status {Status}", job.Id, job.Status);

        job.Cancel();

        await jobRepository.UpdateAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Job {JobId} cancelled successfully", job.Id);

        return mapper.Map<JobDto>(job);
    }
}
