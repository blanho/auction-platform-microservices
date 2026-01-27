using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Features.Categories.GetCategoryById;

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository repository,
        IMapper mapper,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching category {CategoryId}", request.Id);

        try
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Failure<CategoryDto>(AuctionErrors.Category.NotFoundById(request.Id));
            }

            var dto = _mapper.Map<CategoryDto>(category);

            _logger.LogInformation("Successfully fetched category {CategoryId}", request.Id);

            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {CategoryId}", request.Id);
            return Result.Failure<CategoryDto>(AuctionErrors.Category.FetchError(ex.Message));
        }
    }
}
