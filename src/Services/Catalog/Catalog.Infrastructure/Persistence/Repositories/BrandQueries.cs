using System.Linq.Expressions;
using Catalog.Application.DTOs;
using Catalog.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Persistence.Repositories;

public static class BrandQueries
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Brand, object>>> SortMap =
        new SortMapBuilder<Brand, BrandDto>()
            .Add(field => field.Name, brand => brand.Name)
            .Add(field => field.Slug, brand => brand.Slug)
            .Add(field => field.DisplayOrder, brand => brand.DisplayOrder)
            .Build();

    public static IQueryable<Brand> Filter(IQueryable<Brand> query, BrandQueryParams parameters)
    {
        query = query.Where(brand => !brand.IsDeleted);
        if (parameters.ActiveOnly)
            query = query.Where(brand => brand.IsActive);
        if (parameters.FeaturedOnly)
            query = query.Where(brand => brand.IsFeatured);
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(brand => brand.Name.ToLower().Contains(search));
        }
        return query;
    }

    public static IQueryable<Brand> Page(IQueryable<Brand> query, BrandQueryParams parameters)
    {
        var sorts = GetSorts(parameters);
        if (sorts.Count == 0)
            sorts.Add(new SortDescriptor { Field = nameof(BrandDto.DisplayOrder), Desc = false });

        if (string.Equals(sorts[0].Field.Trim(), nameof(BrandDto.DisplayOrder), StringComparison.OrdinalIgnoreCase))
            sorts.Add(new SortDescriptor { Field = nameof(BrandDto.Name), Desc = false });

        return query.ApplySorting(sorts, SortMap, tieBreaker: brand => brand.Id).ApplyPaging(parameters);
    }

    private static List<SortDescriptor> GetSorts(BrandQueryParams parameters)
    {
        if (parameters.Sorts is { Count: > 0 })
            return parameters.Sorts
                .Where(sort => sort is not null && !string.IsNullOrWhiteSpace(sort.Field) && SortMap.ContainsKey(sort.Field.Trim()))
                .ToList();

        if (!string.IsNullOrWhiteSpace(parameters.SortBy) && SortMap.ContainsKey(parameters.SortBy.Trim()))
            return [new SortDescriptor { Field = parameters.SortBy, Desc = parameters.SortDescending }];

        return [];
    }
}
