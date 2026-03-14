using Auctions.Application.Errors;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Categories.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository repository,
        ILogger<DeleteCategoryCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category {CategoryId}", request.Id);

        try
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Failure<bool>(AuctionErrors.Category.NotFoundById(request.Id));
            }

            if (category.Items.Any())
            {
                return Result.Failure<bool>(AuctionErrors.Category.HasItems);
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted category {CategoryId}", request.Id);

            return Result<bool>.Success(true);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<bool>(AuctionErrors.Category.NotFoundById(request.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete category {CategoryId}: {Error}", request.Id, ex.Message);
            return Result.Failure<bool>(AuctionErrors.Category.DeleteFailed(ex.Message));
        }
    }
}

