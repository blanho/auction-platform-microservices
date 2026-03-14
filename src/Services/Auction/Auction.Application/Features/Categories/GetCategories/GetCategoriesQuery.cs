using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Categories.GetCategories;

public record GetCategoriesQuery(
    bool ActiveOnly = true,
    bool IncludeCount = false
) : IQuery<List<CategoryDto>>;

