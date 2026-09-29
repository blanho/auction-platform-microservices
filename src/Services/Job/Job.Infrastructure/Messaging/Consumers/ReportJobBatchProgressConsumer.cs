using Jobs.Infrastructure.Persistence;
using JobService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ReportJobBatchProgressConsumer(JobProgressStore progress) : IConsumer<ReportJobBatchProgressCommand>
{
    public Task Consume(ConsumeContext<ReportJobBatchProgressCommand> context) =>
        progress.RecordAsync(new JobProgressEntry
        {
            CorrelationId = context.Message.CorrelationId,
            BatchId = context.Message.BatchId ?? context.MessageId?.ToString()
                ?? throw new InvalidOperationException("Progress needs a stable message identity."),
            CompletedCount = context.Message.CompletedCount,
            FailedCount = context.Message.FailedCount
        }, context.CancellationToken);
}
