using Auction.Application.Errors;
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository repository,
        IMapper mapper,
        ILogger<CreateCategoryCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category {Name}", request.Name);

        try
        {
            var slugExists = await _repository.SlugExistsAsync(request.Slug, null, cancellationToken);
            if (slugExists)
            {
                return Result.Failure<CategoryDto>(AuctionErrors.Category.SlugExists(request.Slug));
            }

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                Icon = request.Icon,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                ParentCategoryId = request.ParentCategoryId
            };

            var createdCategory = await _repository.CreateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created category {CategoryId} with name {Name}", createdCategory.Id, request.Name);

            var dto = _mapper.Map<CategoryDto>(createdCategory);
            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create category {Name}: {Error}", request.Name, ex.Message);
            return Result.Failure<CategoryDto>(AuctionErrors.Category.CreateFailed(ex.Message));
        }
    }
}

