#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Categories;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Api.Extensions;

public static class CategoryMappingExtensions
{
    public static CategoryDto ToDto(this Category category, IMapper mapper)
    {
        return mapper.Map<CategoryDto>(category);
    }

    public static List<CategoryDto> ToDtoList(this IEnumerable<Category> categories, IMapper mapper)
    {
        return categories.Select(c => mapper.Map<CategoryDto>(c)).ToList();
    }

    public static PaginatedResult<CategoryDto> ToPaginatedDto(
        this PaginatedResult<Category> result,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<CategoryDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }
}
