using Common.Messaging.Events;
using Common.Storage.Abstractions;
using MassTransit;

namespace UtilityService.Consumers;

/// <summary>
/// Consumes AuctionDeletedEvent to handle file cleanup when an auction is deleted.
/// Implements event-driven file deletion for loose coupling between services.
/// </summary>
public class AuctionDeletedEventConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AuctionDeletedEventConsumer> _logger;

    public AuctionDeletedEventConsumer(
        IFileStorageService fileStorageService,
        ILogger<AuctionDeletedEventConsumer> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        var auctionId = context.Message.Id;
        
        _logger.LogInformation(
            "Received AuctionDeletedEvent for auction {AuctionId}. Starting file cleanup.",
            auctionId);

        try
        {
            // Delete all files associated with this auction
            var deletedCount = await _fileStorageService.DeleteByOwnerEntityAsync(
                ownerService: "auction",
                entityId: auctionId.ToString(),
                context.CancellationToken);

            _logger.LogInformation(
                "File cleanup completed for auction {AuctionId}. Deleted {DeletedCount} files.",
                auctionId,
                deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during file cleanup for auction {AuctionId}",
                auctionId);
            throw;
        }
    }
}
