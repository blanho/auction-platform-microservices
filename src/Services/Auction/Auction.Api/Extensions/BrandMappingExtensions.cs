#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Brands;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Api.Extensions;

public static class BrandMappingExtensions
{
    public static BrandDto ToDto(this Brand brand, IMapper mapper)
    {
        return mapper.Map<BrandDto>(brand);
    }

    public static List<BrandDto> ToDtoList(this IEnumerable<Brand> brands, IMapper mapper)
    {
        return brands.Select(b => mapper.Map<BrandDto>(b)).ToList();
    }

    public static PaginatedResult<BrandDto> ToPaginatedDto(
        this PaginatedResult<Brand> result,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<BrandDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }
}
