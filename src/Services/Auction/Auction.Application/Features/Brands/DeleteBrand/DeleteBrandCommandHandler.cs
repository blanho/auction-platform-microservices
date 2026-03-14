using Auctions.Application.Errors;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Brands.DeleteBrand;

public class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand>
{
    private readonly IBrandRepository _repository;
    private readonly ILogger<DeleteBrandCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(
        IBrandRepository repository,
        ILogger<DeleteBrandCommandHandler> logger,
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
                return Result.Failure(AuctionErrors.Brand.NotFoundById(request.Id));
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted brand {BrandId}", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete brand {BrandId}: {Error}", request.Id, ex.Message);
            return Result.Failure(AuctionErrors.Brand.DeleteFailed(ex.Message));
        }
    }
}

