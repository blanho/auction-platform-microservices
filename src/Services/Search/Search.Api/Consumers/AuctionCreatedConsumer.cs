using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Web.Exceptions;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Search.Api.Documents;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IIndexManagementService _indexManagement;
    private readonly IDistributedCache _cache;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuctionCreatedConsumer> _logger;

    public AuctionCreatedConsumer(
        IAuctionIndexService indexService,
        IIndexManagementService indexManagement,
        IDistributedCache cache,
        IDateTimeProvider dateTime,
        ILogger<AuctionCreatedConsumer> logger)
    {
        _indexService = indexService;
        _indexManagement = indexManagement;
        _cache = cache;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing AuctionCreatedEvent for auction {AuctionId}", message.Id);

        var ensureResult = await _indexManagement.EnsureIndexExistsAsync(context.CancellationToken);
        if (ensureResult.IsFailure)
        {
            throw new ConflictException($"Failed to ensure index exists: {ensureResult.Error}");
        }

        var document = CreateAuctionDocument(message);

        var indexResult = await _indexService.IndexAsync(document, context.CancellationToken);
        if (indexResult.IsFailure)
        {
            throw new ConflictException($"Failed to index auction {message.Id}: {indexResult.Error}");
        }

        _logger.LogInformation("Successfully indexed auction {AuctionId}", message.Id);
    }

    private AuctionDocument CreateAuctionDocument(AuctionCreatedEvent message) =>
        new()
        {
            Id = message.Id,
            Title = message.Title,
            Description = message.Description,
            SellerId = message.SellerId,
            SellerUsername = message.SellerUsername,
            StartPrice = message.ReservePrice,
            CurrentPrice = message.CurrentHighBid ?? message.ReservePrice,
            ReservePrice = message.ReservePrice,
            Currency = message.Currency,
            Status = message.Status,
            Condition = message.Condition,
            EndTime = message.AuctionEnd,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt ?? message.CreatedAt,
            BidCount = 0,
            WinnerId = message.WinnerId,
            WinnerUsername = message.WinnerUsername,
            FinalPrice = message.SoldAmount,
            LastSyncedAt = _dateTime.UtcNowOffset
        };
}
