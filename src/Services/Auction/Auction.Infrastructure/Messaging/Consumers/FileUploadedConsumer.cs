using Auctions.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using StorageService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class FileUploadedConsumer : IConsumer<FileUploadedEvent>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FileUploadedConsumer> _logger;

    public FileUploadedConsumer(
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<FileUploadedConsumer> logger)
    {
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileUploadedEvent> context)
    {
        var message = context.Message;

        if (message.OwnerService != "AuctionService")
        {
            return;
        }

        if (string.IsNullOrEmpty(message.OwnerId))
        {
            _logger.LogWarning("FileUploadedEvent received without OwnerId for file {FileId}", message.FileId);
            return;
        }

        if (!Guid.TryParse(message.OwnerId, out var auctionId))
        {
            _logger.LogWarning("Invalid OwnerId format: {OwnerId}", message.OwnerId);
            return;
        }

        _logger.LogInformation("Processing FileUploadedEvent for auction {AuctionId}, file {FileId}", 
            auctionId, message.FileId);

        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for file {FileId}", auctionId, message.FileId);
            return;
        }

        var existingFile = auction.Item.Files.FirstOrDefault(f => f.FileId == message.FileId);
        if (existingFile != null)
        {
            _logger.LogInformation("File {FileId} already linked to auction {AuctionId}", message.FileId, auctionId);
            return;
        }

        var fileType = GetFileType(message.ContentType);
        var isPrimary = !auction.Item.Files.Any(f => f.IsPrimary && f.FileType == fileType);
        var displayOrder = auction.Item.Files.Count(f => f.FileType == fileType);

        auction.Item.Files.Add(new ItemFileInfo
        {
            FileId = message.FileId,
            FileType = fileType,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary
        });

        await _auctionRepository.UpdateAsync(auction, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Linked file {FileId} to auction {AuctionId} as {FileType} (primary: {IsPrimary})", 
            message.FileId, auctionId, fileType, isPrimary);
    }

    private static string GetFileType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return "image";
        if (contentType.StartsWith("video/"))
            return "video";
        if (contentType.StartsWith("application/pdf") || contentType.Contains("document"))
            return "document";
        return "other";
    }
}

