using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.DeleteFlashSale;

public class DeleteFlashSaleCommandHandler : ICommandHandler<DeleteFlashSaleCommand>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IAppLogger<DeleteFlashSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFlashSaleCommandHandler(
        IFlashSaleRepository repository,
        IAppLogger<DeleteFlashSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting flash sale {FlashSaleId}", request.Id);

        try
        {
            var flashSale = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (flashSale == null)
            {
                return Result.Failure(Error.Create("FlashSale.NotFound", $"Flash sale with ID '{request.Id}' was not found"));
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted flash sale {FlashSaleId}", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete flash sale {FlashSaleId}: {Error}", request.Id, ex.Message);
            return Result.Failure(Error.Create("FlashSale.DeleteFailed", $"Failed to delete flash sale: {ex.Message}"));
        }
    }
}
