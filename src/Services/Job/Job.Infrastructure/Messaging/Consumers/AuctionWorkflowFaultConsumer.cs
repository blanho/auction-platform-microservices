using AuctionService.Contracts.Commands;
using JobService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class AuctionWorkflowFaultConsumer :
    IConsumer<Fault<ProcessAuctionImportCommand>>,
    IConsumer<Fault<ProcessAuctionImportBatchCommand>>,
    IConsumer<Fault<ProcessAuctionExportCommand>>,
    IConsumer<Fault<ProcessBulkAuctionUpdateCommand>>
{
    public Task Consume(ConsumeContext<Fault<ProcessAuctionImportCommand>> context) =>
        Report(context, context.Message.Message.CorrelationId, context.Message.Message.ParentJobId,
            context.Message.Message.ParentJobItemId, context.Message.Message.Attempt);
    public Task Consume(ConsumeContext<Fault<ProcessAuctionImportBatchCommand>> context) =>
        Report(context, context.Message.Message.CorrelationId, null, null, 0);
    public Task Consume(ConsumeContext<Fault<ProcessAuctionExportCommand>> context) =>
        Report(context, context.Message.Message.CorrelationId, context.Message.Message.ParentJobId,
            context.Message.Message.ParentJobItemId, context.Message.Message.Attempt);
    public Task Consume(ConsumeContext<Fault<ProcessBulkAuctionUpdateCommand>> context) =>
        Report(context, context.Message.Message.CorrelationId, context.Message.Message.ParentJobId,
            context.Message.Message.ParentJobItemId, context.Message.Message.Attempt);

    private static Task Report(ConsumeContext context, Guid correlation, Guid? jobId, Guid? itemId, int attempt) =>
        jobId.HasValue && itemId.HasValue
            ? context.Publish(new ReportJobItemResultCommand
            {
                JobId = jobId.Value,
                JobItemId = itemId.Value,
                Attempt = attempt,
                IsFinalFailure = true,
                ErrorMessage = "Auction workflow failed after broker retries."
            })
            : context.Publish(new FailJobByCorrelationCommand
            {
                CorrelationId = correlation.ToString(),
                ErrorMessage = "Auction workflow failed after broker retries."
            });
}
