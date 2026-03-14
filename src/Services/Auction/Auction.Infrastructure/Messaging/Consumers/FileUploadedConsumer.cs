using Auctions.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using StorageService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class FileUploadedConsumer : IConsumer<FileUploadedEvent>
{
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FileUploadedConsumer> _logger;

    public FileUploadedConsumer(
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        ILogger<FileUploadedConsumer> logger)
    {
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileUploadedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming FileUploadedEvent for file {FileId}, owner {OwnerId}",
            message.FileId, message.OwnerId);

        if (!message.OwnerId.HasValue)
        {
            _logger.LogDebug("FileUploadedEvent has no OwnerId, skipping");
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
        if (existingFile is not null)
        {
            _logger.LogDebug(
                "File {FileId} already exists on auction {AuctionId}, skipping (idempotent)",
                message.FileId, auction.Id);
            return;
        }

        var fileType = InferFileType(message.ContentType);
        var displayOrder = auction.Item.Files.Count + 1;
        var isPrimary = auction.Item.Files.Count == 0;

        var mediaFile = MediaFile.Create(message.FileId, fileType, displayOrder, isPrimary);
        auction.Item.AddFile(mediaFile);

        await _writeRepository.UpdateAsync(auction, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Added file {FileId} to auction {AuctionId} (type: {FileType}, order: {Order}, primary: {Primary})",
            message.FileId, auction.Id, fileType, displayOrder, isPrimary);
    }

    private static string InferFileType(string contentType)
    {
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return "image";

        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return "video";

        return "document";
    }
}
