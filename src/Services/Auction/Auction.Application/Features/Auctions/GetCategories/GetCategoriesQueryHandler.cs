using Auction.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Queries.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;
    private const int MaxCategories = 100;

    public GetCategoriesQueryHandler(
        ICategoryRepository repository,
        IMapper mapper,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching categories - ActiveOnly: {ActiveOnly}, IncludeCount: {IncludeCount}",
            request.ActiveOnly, request.IncludeCount);

        try
        {
            List<Auctions.Domain.Entities.Category> categories;
            
            if (request.IncludeCount)
            {
                categories = await _repository.GetCategoriesWithCountAsync(cancellationToken);
            }
            else if (request.ActiveOnly)
            {
                categories = await _repository.GetActiveCategoriesAsync(cancellationToken);
            }
            else
            {
                var result = await _repository.GetPagedAsync(PaginationDefaults.DefaultPage, MaxCategories, cancellationToken);
                categories = result.Items.ToList();
            }

            var dtos = categories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();

            _logger.LogInformation("Successfully fetched {Count} categories", dtos.Count);

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return Result.Failure<List<CategoryDto>>(AuctionErrors.Category.FetchError(ex.Message));
        }
    }
}

