using Auctions.Application.Errors;
using AutoMapper;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Categories.GetCategoryTree;

public class GetCategoryTreeQueryHandler : IQueryHandler<GetCategoryTreeQuery, List<CategoryTreeDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<GetCategoryTreeQueryHandler> _logger;

    public GetCategoryTreeQueryHandler(
        ICategoryRepository repository,
        ILogger<GetCategoryTreeQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<CategoryTreeDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching category tree - ActiveOnly: {ActiveOnly}", request.ActiveOnly);

        try
        {
            var categories = request.ActiveOnly
                ? await _repository.GetActiveCategoriesAsync(cancellationToken)
                : (await _repository.GetCategoriesWithCountAsync(cancellationToken));

            var lookup = categories.ToDictionary(c => c.Id);
            var rootNodes = new List<CategoryTreeDto>();

            var dtoMap = categories.ToDictionary(
                c => c.Id,
                c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Icon = c.Icon,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive
                });

            foreach (var category in categories)
            {
                var dto = dtoMap[category.Id];

                if (category.ParentCategoryId.HasValue && dtoMap.TryGetValue(category.ParentCategoryId.Value, out var parentDto))
                {
                    parentDto.Children.Add(dto);
                }
                else
                {
                    rootNodes.Add(dto);
                }
            }

            SortTreeRecursive(rootNodes);

            _logger.LogInformation("Successfully built category tree with {RootCount} root categories", rootNodes.Count);

            return Result<List<CategoryTreeDto>>.Success(rootNodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category tree");
            return Result.Failure<List<CategoryTreeDto>>(AuctionErrors.Category.FetchError(ex.Message));
        }
    }

    private static void SortTreeRecursive(List<CategoryTreeDto> nodes)
    {
        nodes.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
        foreach (var node in nodes)
        {
            if (node.Children.Count > 0)
                SortTreeRecursive(node.Children);
        }
    }
}
