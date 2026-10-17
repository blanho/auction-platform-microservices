using SearchService.Contracts.Commands;
using AnalyticsService.Contracts.Commands;
using JobService.Contracts.Commands;
using NotificationService.Contracts.Commands;
using PaymentService.Contracts.Commands;
using StorageService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ServiceJobFaultConsumer :
    IConsumer<Fault<GenerateOrderReportCommand>>,
    IConsumer<Fault<SendBulkNotificationCommand>>,
    IConsumer<Fault<ProcessImageCommand>>,
    IConsumer<Fault<FinalizeFileUploadCommand>>,
    IConsumer<Fault<ReindexAuctionsCommand>>,
    IConsumer<Fault<AggregateDailyStatsCommand>>
{
    public Task Consume(ConsumeContext<Fault<ProcessImageCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "Image processing failed after broker retries.");
    }

    public Task Consume(ConsumeContext<Fault<FinalizeFileUploadCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "File upload finalization failed after broker retries.");
    }

    public Task Consume(ConsumeContext<Fault<GenerateOrderReportCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "Report generation failed after broker retries.");
    }

    public Task Consume(ConsumeContext<Fault<SendBulkNotificationCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "Bulk notification failed after broker retries.");
    }

    public Task Consume(ConsumeContext<Fault<ReindexAuctionsCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "Search reindexing failed after broker retries.");
    }

    public Task Consume(ConsumeContext<Fault<AggregateDailyStatsCommand>> context)
    {
        var command = context.Message.Message;
        return Report(context, command.CorrelationId, command.ParentJobId,
            command.ParentJobItemId, command.Attempt, "Analytics aggregation failed after broker retries.");
    }

    private static Task Report(ConsumeContext context, Guid correlation, Guid? jobId,
        Guid? itemId, int attempt, string error) =>
        jobId.HasValue && itemId.HasValue
            ? context.Publish(new ReportJobItemResultCommand
            {
                JobId = jobId.Value,
                JobItemId = itemId.Value,
                Attempt = attempt,
                IsSuccess = false,
                IsFinalFailure = true,
                ErrorMessage = error
            }, context.CancellationToken)
            : context.Publish(new FailJobByCorrelationCommand
            {
                CorrelationId = correlation.ToString(),
                ErrorMessage = error
            }, context.CancellationToken);
}
