using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Domain.Enums;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Auction = Auctions.Domain.Entities.Auction;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BulkUpdateAuctionsConsumer : IConsumer<ProcessBulkAuctionUpdateCommand>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<BulkUpdateAuctionsConsumer> _logger;

    public BulkUpdateAuctionsConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<BulkUpdateAuctionsConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessBulkAuctionUpdateCommand> context)
    {
        var message = context.Message;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = message.CorrelationId.ToString();

        _logger.LogInformation(
            "Processing bulk auction update {CorrelationId}: {Count} auctions, Activate={Activate}",
            correlationId, message.AuctionIds.Count, message.Activate);

        await context.Publish(new RequestJobCommand
        {
            JobType = nameof(JobType.BulkAuctionUpdate),
            CorrelationId = correlationId,
            RequestedBy = message.RequestedBy,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                message.Activate,
                message.Reason,
                AuctionCount = message.AuctionIds.Count
            }),
            TotalItems = message.AuctionIds.Count,
            MaxRetryCount = 0
        });

        var succeededCount = 0;
        var failedCount = 0;
        var batchSucceeded = 0;
        var batchFailed = 0;
        const int progressBatchSize = 100;

        foreach (var auctionId in message.AuctionIds)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var auction = await _readRepository.GetByIdAsync(auctionId, context.CancellationToken);
                if (auction is null)
                {
                    _logger.LogWarning(
                        "Auction {AuctionId} not found during bulk update {CorrelationId}",
                        auctionId, correlationId);
                    failedCount++;
                    batchFailed++;
                }
                else if (TryApplyStatusChange(auction, message.Activate))
                {
                    await _writeRepository.UpdateAsync(auction, context.CancellationToken);
                    succeededCount++;
                    batchSucceeded++;
                }
                else
                {
                    failedCount++;
                    batchFailed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to update auction {AuctionId} in bulk update {CorrelationId}",
                    auctionId, correlationId);
                failedCount++;
                batchFailed++;
            }

            if (batchSucceeded + batchFailed >= progressBatchSize)
            {
                await context.Publish(new ReportJobBatchProgressCommand
                {
                    CorrelationId = correlationId,
                    CompletedCount = batchSucceeded,
                    FailedCount = batchFailed
                });
                batchSucceeded = 0;
                batchFailed = 0;
            }
        }

        if (batchSucceeded + batchFailed > 0)
        {
            await context.Publish(new ReportJobBatchProgressCommand
            {
                CorrelationId = correlationId,
                CompletedCount = batchSucceeded,
                FailedCount = batchFailed
            });
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Bulk update {CorrelationId} completed: {Succeeded}/{Total} succeeded in {Duration}ms",
            correlationId, succeededCount, message.AuctionIds.Count, stopwatch.ElapsedMilliseconds);

        await context.Publish(new BulkAuctionUpdateCompletedEvent
        {
            CorrelationId = message.CorrelationId,
            RequestedBy = message.RequestedBy,
            TotalRequested = message.AuctionIds.Count,
            SucceededCount = succeededCount,
            FailedCount = failedCount,
            Activated = message.Activate,
            Reason = message.Reason,
            Duration = stopwatch.Elapsed,
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    private bool TryApplyStatusChange(Auction auction, bool activate)
    {
        if (activate)
        {
            if ((auction.Status == Status.Inactive || auction.Status == Status.Scheduled) &&
                auction.AuctionEnd > _dateTime.UtcNow)
            {
                auction.ChangeStatus(Status.Live);
                return true;
            }
        }
        else
        {
            if (auction.Status == Status.Live || auction.Status == Status.Scheduled)
            {
                auction.ChangeStatus(Status.Inactive);
                return true;
            }
        }

        return false;
    }
}
