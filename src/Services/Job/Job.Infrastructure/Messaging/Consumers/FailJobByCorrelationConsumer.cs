using Jobs.Infrastructure.Persistence;
using JobService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class FailJobByCorrelationConsumer(JobProgressStore progress) : IConsumer<FailJobByCorrelationCommand>
{
    public Task Consume(ConsumeContext<FailJobByCorrelationCommand> context) =>
        progress.RecordAsync(new JobProgressEntry
        {
            CorrelationId = context.Message.CorrelationId,
            BatchId = "failure:" + (context.MessageId?.ToString()
                ?? throw new InvalidOperationException("Failure needs a stable message identity.")),
            ErrorMessage = context.Message.ErrorMessage
        }, context.CancellationToken);
}
