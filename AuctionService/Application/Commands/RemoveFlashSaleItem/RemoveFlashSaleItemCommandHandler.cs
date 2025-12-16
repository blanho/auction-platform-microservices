using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.RemoveFlashSaleItem;

public class RemoveFlashSaleItemCommandHandler : ICommandHandler<RemoveFlashSaleItemCommand>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IAppLogger<RemoveFlashSaleItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFlashSaleItemCommandHandler(
        IFlashSaleRepository repository,
        IAppLogger<RemoveFlashSaleItemCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveFlashSaleItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item {AuctionId} from flash sale {FlashSaleId}", request.AuctionId, request.FlashSaleId);

        try
        {
            var flashSale = await _repository.GetByIdWithItemsAsync(request.FlashSaleId, cancellationToken);
            if (flashSale == null)
            {
                return Result.Failure(Error.Create("FlashSale.NotFound", $"Flash sale with ID '{request.FlashSaleId}' was not found"));
            }

            var item = flashSale.Items.FirstOrDefault(i => i.AuctionId == request.AuctionId);
            if (item == null)
            {
                return Result.Failure(Error.Create("FlashSale.ItemNotFound", "This auction is not in the flash sale"));
            }

            await _repository.RemoveItemAsync(request.FlashSaleId, request.AuctionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed item {AuctionId} from flash sale {FlashSaleId}", request.AuctionId, request.FlashSaleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove item from flash sale: {Error}", ex.Message);
            return Result.Failure(Error.Create("FlashSale.RemoveItemFailed", $"Failed to remove item from flash sale: {ex.Message}"));
        }
    }
}
