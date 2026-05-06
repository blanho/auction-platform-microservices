using System.Diagnostics;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Domain.Constants;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using JobService.Contracts.Commands;
using JobService.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Auction = Auctions.Domain.Entities.Auction;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BulkUpdateAuctionsConsumer : IConsumer<ProcessBulkAuctionUpdateCommand>
{
    private readonly IAuctionQueryRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<BulkUpdateAuctionsConsumer> _logger;

    public BulkUpdateAuctionsConsumer(
        IAuctionQueryRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        AuctionDbContext dbContext,
        ILogger<BulkUpdateAuctionsConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _dbContext = dbContext;
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
        var pendingChanges = 0;

        var idBatches = ChunkList(message.AuctionIds, AuctionDefaults.Batch.FetchBatchSize);

        foreach (var idBatch in idBatches)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var auctions = await _readRepository.GetByIdsAsync(idBatch, context.CancellationToken);
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
                    if (TryApplyStatusChange(auction, message.Activate))
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
                catch (Exception ex)
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
                _dbContext.ChangeTracker.Clear();

                await context.Publish(new ReportJobBatchProgressCommand
                {
                    CorrelationId = correlationId,
                    CompletedCount = succeededCount,
                    FailedCount = failedCount
                });

                pendingChanges = 0;
            }
        }

        if (pendingChanges > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _dbContext.ChangeTracker.Clear();
        }

        await context.Publish(new ReportJobBatchProgressCommand
        {
            CorrelationId = correlationId,
            CompletedCount = succeededCount,
            FailedCount = failedCount
        });

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

    private static List<List<T>> ChunkList<T>(IReadOnlyList<T> source, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            chunks.Add(source.Skip(i).Take(Math.Min(chunkSize, source.Count - i)).ToList());
        }
        return chunks;
    }
}
