using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Features.Categories.ImportCategories;

public class ImportCategoriesCommandHandler : ICommandHandler<ImportCategoriesCommand, ImportCategoriesResult>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<ImportCategoriesCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ImportCategoriesCommandHandler(
        ICategoryRepository repository,
        ILogger<ImportCategoriesCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportCategoriesResult>> Handle(ImportCategoriesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Importing {Count} categories", request.Categories.Count);

        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        try
        {
            foreach (var dto in request.Categories)
            {
                try
                {
                    var slugExists = await _repository.SlugExistsAsync(dto.Slug, null, cancellationToken);
                    if (slugExists)
                    {
                        errors.Add($"Category with slug '{dto.Slug}' already exists");
                        failureCount++;
                        continue;
                    }

                    var category = new Category
                    {
                        Name = dto.Name,
                        Slug = dto.Slug,
                        Icon = dto.Icon,
                        Description = dto.Description,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = dto.IsActive,
                        ParentCategoryId = dto.ParentCategoryId
                    };

                    await _repository.CreateAsync(category, cancellationToken);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to import category '{dto.Name}': {ex.Message}");
                    failureCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Import completed: {SuccessCount} succeeded, {FailureCount} failed", 
                successCount, failureCount);

            return Result<ImportCategoriesResult>.Success(new ImportCategoriesResult(successCount, failureCount, errors));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to import categories: {Error}", ex.Message);
            return Result.Failure<ImportCategoriesResult>(AuctionErrors.Category.ImportFailed(ex.Message));
        }
    }
}

