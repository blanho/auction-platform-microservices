using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuctionService.Contracts.Commands;
using Jobs.Infrastructure.Persistence;
using Jobs.Domain.Enums;
using JobService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ProcessJobItemConsumer(JobDbContext db) : IConsumer<ProcessJobItemCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task Consume(ConsumeContext<ProcessJobItemCommand> context)
    {
        var message = context.Message;
        var item = await db.JobItems.SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == message.JobItemId, context.CancellationToken);
        var job = await db.Jobs.SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == message.JobId, context.CancellationToken);
        if (item is null || job is null || item.JobId != job.Id || item.Status != JobItemStatus.Processing ||
            job.Status != JobStatus.Processing || item.RetryCount != message.Attempt) return;

        var correlation = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes($"{item.Id}:{item.RetryCount}"))[..16]);
        try
        {
            switch (job.Type)
            {
                case JobType.AuctionImport:
                    var import = Deserialize<ProcessAuctionImportCommand>(item.PayloadJson);
                    await context.Publish(import with
                    {
                        CorrelationId = correlation,
                        SellerId = job.RequestedBy,
                        ParentJobId = job.Id,
                        ParentJobItemId = item.Id,
                        Attempt = item.RetryCount
                    });
                    break;
                case JobType.BulkAuctionUpdate:
                    var update = Deserialize<ProcessBulkAuctionUpdateCommand>(item.PayloadJson);
                    await context.Publish(update with
                    {
                        CorrelationId = correlation,
                        RequestedBy = job.RequestedBy,
                        ParentJobId = job.Id,
                        ParentJobItemId = item.Id,
                        Attempt = item.RetryCount
                    });
                    break;
                case JobType.DataExport:
                    var export = Deserialize<ProcessAuctionExportCommand>(item.PayloadJson);
                    await context.Publish(export with
                    {
                        CorrelationId = correlation,
                        RequestedBy = job.RequestedBy,
                        ParentJobId = job.Id,
                        ParentJobItemId = item.Id,
                        Attempt = item.RetryCount
                    });
                    break;
                default:
                    await FailAsync($"No item processor is registered for {job.Type}.");
                    break;
            }
        }
        catch (JsonException)
        {
            await FailAsync("The job item payload is not a valid command.");
        }

        Task FailAsync(string error) => context.Publish(new ReportJobItemResultCommand
        {
            JobId = job.Id,
            JobItemId = item.Id,
            Attempt = item.RetryCount,
            IsSuccess = false,
            IsFinalFailure = true,
            ErrorMessage = error
        });
    }

    private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new JsonException("A command payload is required.");
}
