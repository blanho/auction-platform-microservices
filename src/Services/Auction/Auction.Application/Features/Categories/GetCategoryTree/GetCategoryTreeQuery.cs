using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Categories.GetCategoryTree;

public record GetCategoryTreeQuery(bool ActiveOnly = true) : IQuery<List<CategoryTreeDto>>;
