using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Categories.BulkUpdateCategories;

public class BulkUpdateCategoriesCommandHandler : ICommandHandler<BulkUpdateCategoriesCommand, int>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<BulkUpdateCategoriesCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public BulkUpdateCategoriesCommandHandler(
        ICategoryRepository repository,
        ILogger<BulkUpdateCategoriesCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(BulkUpdateCategoriesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bulk updating {Count} categories to IsActive={IsActive}", 
            request.CategoryIds.Count, request.IsActive);

        try
        {
            var updatedCount = 0;
            var categories = await _repository.GetByIdsForUpdateAsync(request.CategoryIds, cancellationToken);
            var categoriesById = categories.ToDictionary(x => x.Id, x => x);

            foreach (var categoryId in request.CategoryIds.Distinct())
            {
                if (!categoriesById.TryGetValue(categoryId, out var category))
                {
                    _logger.LogWarning("Category {CategoryId} not found during bulk update", categoryId);
                    continue;
                }

                category.IsActive = request.IsActive;
                await _repository.UpdateAsync(category, cancellationToken);
                updatedCount++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk updated {UpdatedCount} categories", updatedCount);

            return Result<int>.Success(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk update categories: {Error}", ex.Message);
            return Result.Failure<int>(Error.Create("Category.BulkUpdateFailed", $"Failed to bulk update categories: {ex.Message}"));
        }
    }
}

