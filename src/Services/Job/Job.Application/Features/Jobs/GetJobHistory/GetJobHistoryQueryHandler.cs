using JobExecutionLogEntity = Jobs.Domain.Entities.JobExecutionLog;

namespace Jobs.Application.Features.Jobs.GetJobHistory;

public class GetJobHistoryQueryHandler : IQueryHandler<GetJobHistoryQuery, JobHistoryDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutionLogRepository _logRepository;
    private readonly ILogger<GetJobHistoryQueryHandler> _logger;

    public GetJobHistoryQueryHandler(
        IJobRepository jobRepository,
        IJobExecutionLogRepository logRepository,
        ILogger<GetJobHistoryQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task<Result<JobHistoryDto>> Handle(
        GetJobHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return Result.Failure<JobHistoryDto>(
                Error.Create("Job.NotFound", $"Job with ID '{request.JobId}' was not found."));
        }

        var logsPage = await _logRepository.GetByJobIdAsync(
            request.JobId,
            request.Page,
            request.PageSize,
            request.LogLevel,
            cancellationToken);

        var entries = logsPage.Items.Select(MapToDto).ToList();

        _logger.LogInformation(
            "Retrieved {EntryCount} execution log entries for job {JobId} (page {Page})",
            entries.Count, request.JobId, request.Page);

        return Result<JobHistoryDto>.Success(new JobHistoryDto
        {
            JobId = job.Id,
            Type = job.Type,
            CurrentStatus = job.Status,
            TotalLogEntries = logsPage.TotalCount,
            Entries = entries
        });
    }

    private static JobExecutionLogDto MapToDto(JobExecutionLogEntity log) =>
        new()
        {
            Id = log.Id,
            JobId = log.JobId,
            LogLevel = log.LogLevel,
            Message = log.Message,
            PreviousStatus = log.PreviousStatus,
            NewStatus = log.NewStatus,
            MachineName = log.Context?.MachineName,
            OperationName = log.Context?.OperationName,
            BatchNumber = log.Context?.BatchNumber,
            BatchSize = log.Context?.BatchSize,
            DurationMs = log.Duration?.TotalMilliseconds,
            Timestamp = log.Timestamp
        };
}
