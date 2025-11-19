using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetCategories;

public record GetCategoriesQuery(
    bool ActiveOnly = true,
    bool IncludeCount = false
) : IQuery<List<CategoryDto>>;

