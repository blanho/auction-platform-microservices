using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.DeleteBrand;

public class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand>
{
    private readonly IBrandRepository _repository;
    private readonly IAppLogger<DeleteBrandCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(
        IBrandRepository repository,
        IAppLogger<DeleteBrandCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting brand {BrandId}", request.Id);

        try
        {
            var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (brand == null)
            {
                return Result.Failure(Error.Create("Brand.NotFound", $"Brand with ID '{request.Id}' was not found"));
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted brand {BrandId}", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete brand {BrandId}: {Error}", request.Id, ex.Message);
            return Result.Failure(Error.Create("Brand.DeleteFailed", $"Failed to delete brand: {ex.Message}"));
        }
    }
}
