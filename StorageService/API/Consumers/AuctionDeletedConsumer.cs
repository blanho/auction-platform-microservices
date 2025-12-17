using Common.Messaging.Events;
using MassTransit;
using StorageService.Application.Interfaces;
using StorageService.Domain.Enums;

namespace StorageService.API.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionDeletedConsumer> _logger;

    public AuctionDeletedConsumer(
        IUnitOfWork unitOfWork,
        ILogger<AuctionDeletedConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        var message = context.Message;
        var auctionId = message.Id.ToString();

        _logger.LogInformation("Processing AuctionDeletedEvent for auction {AuctionId}", auctionId);

        var files = await _unitOfWork.StoredFiles.GetByOwnerAsync("AuctionService", auctionId);
        var fileList = files.ToList();

        if (fileList.Count == 0)
        {
            _logger.LogInformation("No files found for auction {AuctionId}", auctionId);
            return;
        }

        _logger.LogInformation("Marking {Count} files as deleted for auction {AuctionId}", fileList.Count, auctionId);

        foreach (var file in fileList)
        {
            file.Status = FileStatus.Deleted;
            file.DeletedAt = DateTimeOffset.UtcNow;
            _unitOfWork.StoredFiles.Update(file);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully marked {Count} files as deleted for auction {AuctionId}", 
            fileList.Count, auctionId);
    }
}
