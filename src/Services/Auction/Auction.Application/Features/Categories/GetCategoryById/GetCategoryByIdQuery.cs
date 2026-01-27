using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Categories.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryDto>;
