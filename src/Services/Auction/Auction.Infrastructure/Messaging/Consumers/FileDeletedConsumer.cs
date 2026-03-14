using MassTransit;
using Microsoft.Extensions.Logging;
using StorageService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class FileDeletedConsumer : IConsumer<FileDeletedEvent>
{
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FileDeletedConsumer> _logger;

    public FileDeletedConsumer(
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        ILogger<FileDeletedConsumer> logger)
    {
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming FileDeletedEvent for file {FileId}, owner {OwnerId}",
            message.FileId, message.OwnerId);

        if (!message.OwnerId.HasValue)
        {
            _logger.LogDebug("FileDeletedEvent has no OwnerId, skipping");
            return;
        }

        var auction = await _writeRepository.GetByIdForUpdateAsync(message.OwnerId.Value, context.CancellationToken);
        if (auction is null)
        {
            _logger.LogDebug(
                "Auction {AuctionId} not found for file {FileId}, may belong to another service",
                message.OwnerId.Value, message.FileId);
            return;
        }

        var existingFile = auction.Item.Files.FirstOrDefault(f => f.FileId == message.FileId);
        if (existingFile is null)
        {
            _logger.LogDebug(
                "File {FileId} not found on auction {AuctionId}, skipping (idempotent)",
                message.FileId, auction.Id);
            return;
        }

        auction.Item.RemoveFile(message.FileId);

        await _writeRepository.UpdateAsync(auction, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Removed file {FileId} from auction {AuctionId}",
            message.FileId, auction.Id);
    }
}
