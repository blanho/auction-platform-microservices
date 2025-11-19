using AuctionService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Storage.Application.Interfaces;

namespace Storage.Api.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly Storage.Application.Interfaces.IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionDeletedConsumer> _logger;

    public AuctionDeletedConsumer(
        Storage.Application.Interfaces.IUnitOfWork unitOfWork,
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
            file.MarkRemoved();
            _unitOfWork.StoredFiles.Update(file);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully marked {Count} files as deleted for auction {AuctionId}",
            fileList.Count, auctionId);
    }
}
