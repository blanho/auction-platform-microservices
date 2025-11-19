using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _repository;
    private readonly IAppLogger<DeleteCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository repository,
        IAppLogger<DeleteCategoryCommandHandler> logger,
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

            if (category.Items.Any())
            {
                return Result.Failure<bool>(Error.Create("Category.HasItems", "Cannot delete category that has associated items. Please reassign or delete the items first."));
            }

            await _repository.DeleteAsync(request.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted category {CategoryId}", request.Id);

            return Result<bool>.Success(true);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<bool>(Error.Create("Category.NotFound", $"Category with ID '{request.Id}' not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete category {CategoryId}: {Error}", request.Id, ex.Message);
            return Result.Failure<bool>(Error.Create("Category.DeleteFailed", $"Failed to delete category: {ex.Message}"));
        }
    }
}

