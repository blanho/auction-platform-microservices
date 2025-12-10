using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetCategories;

public record GetCategoriesQuery(
    bool ActiveOnly = true,
    bool IncludeCount = false
) : IQuery<List<CategoryDto>>;
