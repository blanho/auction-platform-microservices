using JobService.Contracts.Commands;
using NotificationService.Contracts.Commands;
using PaymentService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ServiceJobFaultConsumer :
    IConsumer<Fault<GenerateOrderReportCommand>>,
    IConsumer<Fault<SendBulkNotificationCommand>>
{
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
