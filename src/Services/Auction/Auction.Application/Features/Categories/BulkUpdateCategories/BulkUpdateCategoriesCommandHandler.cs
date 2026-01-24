using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.BulkUpdateCategories;

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

            foreach (var categoryId in request.CategoryIds)
            {
                try
                {
                    var category = await _repository.GetByIdAsync(categoryId, cancellationToken);
                    if (category == null)
                    {
                        _logger.LogWarning("Category {CategoryId} not found during bulk update", categoryId);
                        continue;
                    }
                    
                    category.IsActive = request.IsActive;
                    await _repository.UpdateAsync(category, cancellationToken);
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error updating category {CategoryId} during bulk update", categoryId);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk updated {UpdatedCount} categories", updatedCount);

            return Result<int>.Success(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to bulk update categories: {Error}", ex.Message);
            return Result.Failure<int>(Error.Create("Category.BulkUpdateFailed", $"Failed to bulk update categories: {ex.Message}"));
        }
    }
}

