using System.Text.Json;
using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Domain.Constants;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BulkUpdateAuctionsConsumer : IConsumer<ProcessBulkAuctionUpdateCommand>
{
    private readonly IAuctionWorkflowStore _workflow;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<BulkUpdateAuctionsConsumer> _logger;

    public BulkUpdateAuctionsConsumer(
        IAuctionWorkflowStore workflow,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<BulkUpdateAuctionsConsumer> logger)
    {
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
        _workflow = workflow;
    }

    public async Task Consume(ConsumeContext<ProcessBulkAuctionUpdateCommand> context)
    {
        var message = context.Message;
        var receiptKey = $"BulkUpdateAuctionsConsumer:{message.CorrelationId}";
        if (await _workflow.ExistsAsync(receiptKey, context.CancellationToken))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing bulk auction update {CorrelationId}: {Count} auctions, Activate={Activate}",
            correlationId, message.AuctionIds.Count, message.Activate);

        await PublishJobRequestAsync(context, correlationId);

        var succeededCount = 0;
        var failedCount = 0;
        var pendingChanges = 0;

        foreach (var idBatch in message.AuctionIds.Chunk(AuctionDefaults.Batch.FetchBatchSize))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = await _writeRepository.GetByIdsForUpdateAsync(idBatch, context.CancellationToken);
            var auctionLookup = auctions.ToDictionary(a => a.Id);

            foreach (var auctionId in idBatch)
            {
                if (!auctionLookup.TryGetValue(auctionId, out var auction))
                {
                    failedCount++;
                    continue;
                }

                try
                {
                    var statusChanged = message.Activate
                        ? auction.TryActivate(_dateTime.UtcNow)
                        : auction.TryDeactivate();
                    if (statusChanged)
                    {
                        await _writeRepository.UpdateAsync(auction, context.CancellationToken);
                        succeededCount++;
                        pendingChanges++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
                catch (BuildingBlocks.Domain.Exceptions.DomainInvariantException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to update auction {AuctionId} in bulk update {CorrelationId}",
                        auctionId, correlationId);
                    failedCount++;
                }
            }

            if (pendingChanges >= AuctionDefaults.Batch.SaveBatchSize)
            {
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                pendingChanges = 0;
            }
        }

        if (pendingChanges > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        await PublishProgressAsync(context, correlationId, succeededCount, failedCount);

        stopwatch.Stop();

        _logger.LogInformation(
            "Bulk update {CorrelationId} completed: {Succeeded}/{Total} succeeded in {Duration}ms",
            correlationId, succeededCount, message.AuctionIds.Count, stopwatch.ElapsedMilliseconds);

        await PublishCompletionAsync(context, receiptKey, succeededCount, failedCount, stopwatch.Elapsed);
    }

    private static async Task PublishJobRequestAsync(
        ConsumeContext<ProcessBulkAuctionUpdateCommand> context,
        string correlationId)
    {
        var message = context.Message;
        if (message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.BulkAuctionUpdate),
            CorrelationId = correlationId,
            RequestedBy = message.RequestedBy,
            PayloadJson = JsonSerializer.Serialize(new
            {
                message.Activate,
                message.Reason,
                AuctionCount = message.AuctionIds.Count
            }),
            TotalItems = message.AuctionIds.Count,
            MaxRetryCount = 0
        });
    }

    private async Task PublishCompletionAsync(
        ConsumeContext<ProcessBulkAuctionUpdateCommand> context,
        string receiptKey,
        int succeededCount,
        int failedCount,
        TimeSpan duration)
    {
        var message = context.Message;
        await _workflow.CompleteAsync(receiptKey, message.CorrelationId, succeededCount, failedCount,
            [], 1, context.CancellationToken);
        if (message.ParentJobId.HasValue && message.ParentJobItemId.HasValue)
        {
            await context.Publish(new ReportJobItemResultCommand
            {
                JobId = message.ParentJobId.Value,
                JobItemId = message.ParentJobItemId.Value,
                Attempt = message.Attempt,
                IsSuccess = failedCount == 0,
                IsFinalFailure = true,
                ErrorMessage = failedCount == 0 ? null : "Auction workflow completed with errors."
            });
        }

        await context.Publish(new BulkAuctionUpdateCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            TotalRequested = message.AuctionIds.Count,
            SucceededCount = succeededCount,
            FailedCount = failedCount,
            Activated = message.Activate,
            Reason = message.Reason,
            Duration = duration,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    private static async Task PublishProgressAsync(
        ConsumeContext<ProcessBulkAuctionUpdateCommand> context,
        string correlationId,
        int completedCount,
        int failedCount)
    {
        if (context.Message.ParentJobId is not null)
        {
            return;
        }

        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            BatchId = "bulk-update:complete",
            CompletedCount = completedCount,
            FailedCount = failedCount
        });
    }
}
