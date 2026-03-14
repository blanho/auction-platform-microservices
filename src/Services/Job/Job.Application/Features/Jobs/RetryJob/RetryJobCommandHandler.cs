using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using Jobs.Application.DTOs.Audit;
using Jobs.Application.Errors;
using JobStatus = Jobs.Domain.Enums.JobStatus;

namespace Jobs.Application.Features.Jobs.RetryJob;

public class RetryJobCommandHandler(
    IJobRepository jobRepository,
    IJobItemRepository jobItemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RetryJobCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<RetryJobCommand, JobDto>
{
    public async Task<Result<JobDto>> Handle(
        RetryJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdForUpdateAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<JobDto>(JobErrors.Job.NotFoundById(request.JobId));

        if (job.Status is not (JobStatus.Failed or JobStatus.CompletedWithErrors))
            return Result.Failure<JobDto>(JobErrors.Job.CannotRetry(job.Status.ToString()));

        var oldJobData = JobAuditData.FromJob(job);

        logger.LogInformation("Retrying job {JobId} with status {Status}", job.Id, job.Status);

        await jobItemRepository.ResetFailedItemsAsync(job.Id, cancellationToken);
        job.ResetForRetry();

        await jobRepository.UpdateAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditPublisher.PublishAsync(
            job.Id,
            JobAuditData.FromJob(job),
            AuditAction.Updated,
            oldJobData,
            metadata: new Dictionary<string, object>
            {
                ["Action"] = "Retry",
                ["PreviousStatus"] = oldJobData.Status
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("Job {JobId} reset for retry successfully", job.Id);

        var updatedJob = await jobRepository.GetByIdAsync(job.Id, cancellationToken);
        return mapper.Map<JobDto>(updatedJob);
    }
}
