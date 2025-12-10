using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        ICategoryRepository repository,
        IMapper mapper,
        IAppLogger<GetCategoriesQueryHandler> logger)
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
            var categories = request.IncludeCount
                ? await _repository.GetCategoriesWithCountAsync(cancellationToken)
                : request.ActiveOnly
                    ? await _repository.GetActiveCategoriesAsync(cancellationToken)
                    : await _repository.GetAllAsync(cancellationToken);

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            _logger.LogInformation("Successfully fetched {Count} categories", dtos.Count);

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return Result.Failure<List<CategoryDto>>(Error.Create("Categories.FetchError", $"Error fetching categories: {ex.Message}"));
        }
    }
}
