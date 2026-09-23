using Catalog.Application.Filtering;
using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Persistence.Repositories;

public static class BrandQueries
{
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
        var ordered = (parameters.SortBy?.ToLowerInvariant(), parameters.SortDescending) switch
        {
            ("name", false) => query.OrderBy(brand => brand.Name),
            ("name", true) => query.OrderByDescending(brand => brand.Name),
            ("slug", false) => query.OrderBy(brand => brand.Slug),
            ("slug", true) => query.OrderByDescending(brand => brand.Slug),
            ("displayorder", true) => query.OrderByDescending(brand => brand.DisplayOrder).ThenBy(brand => brand.Name),
            _ => query.OrderBy(brand => brand.DisplayOrder).ThenBy(brand => brand.Name)
        };
        return ordered.ThenBy(brand => brand.Id).Skip(parameters.Skip).Take(parameters.Take);
    }
}
