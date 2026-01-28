using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Consumers;

public class BidPlacedBatchConsumer : IConsumer<Batch<BidPlacedEvent>>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidPlacedBatchConsumer> _logger;

    public BidPlacedBatchConsumer(
        AnalyticsDbContext context,
        ILogger<BidPlacedBatchConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Batch<BidPlacedEvent>> context)
    {
        var batch = context.Message;
        var batchSize = batch.Length;

        _logger.LogInformation("Processing batch of {BatchSize} BidPlaced events", batchSize);

        var eventIds = batch.Select(m => m.Message.Id).ToList();

        var existingIds = await _context.FactBids
            .Where(f => eventIds.Contains(f.EventId))
            .Select(f => f.EventId)
            .ToHashSetAsync(context.CancellationToken);

        var newFacts = new List<FactBid>();
        var duplicateCount = 0;

        foreach (var message in batch)
        {
            var @event = message.Message;

            if (existingIds.Contains(@event.Id))
            {
                duplicateCount++;
                continue;
            }

            var fact = new FactBid
            {
                EventId = @event.Id,
                EventTime = @event.BidTime,
                IngestedAt = DateTimeOffset.UtcNow,

                AuctionId = @event.AuctionId,
                BidderId = @event.BidderId,
                DateKey = DateOnly.FromDateTime(@event.BidTime.UtcDateTime),

                BidAmount = @event.BidAmount,

                BidderUsername = @event.Bidder,

                BidStatus = @event.BidStatus,

                EventVersion = (short)@event.Version
            };

            newFacts.Add(fact);
        }

        if (newFacts.Count > 0)
        {
            await _context.FactBids.AddRangeAsync(newFacts, context.CancellationToken);
            await _context.SaveChangesAsync(context.CancellationToken);
        }

        _logger.LogInformation(
            "Batch complete: {NewCount} new facts recorded, {DuplicateCount} duplicates skipped",
            newFacts.Count, duplicateCount);
    }
}
