using Catalog.Application.Filtering;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Catalog.Infrastructure.Tests;

public class BrandQueryTests
{
    [Fact]
    public void FiltersRunBeforeCountingAndPaging()
    {
        var active = Brand.Create("Alpha", "alpha", isFeatured: true);
        var inactive = Brand.Create("ALPine", "alpine", isFeatured: true);
        inactive.Deactivate();
        var ordinary = Brand.Create("Alphabet", "alphabet");
        var deleted = Brand.Create("Alps", "alps", isFeatured: true);
        deleted.MarkAsDeleted(Guid.Empty, DateTimeOffset.UtcNow);
        var query = new[] { inactive, ordinary, deleted, active }.AsQueryable();
        var parameters = new BrandQueryParams
        {
            ActiveOnly = false,
            FeaturedOnly = true,
            Search = " alp ",
            SortBy = "name",
            SortDescending = false,
            Page = 2,
            PageSize = 1
        };
        var filtered = BrandQueries.Filter(query, parameters);
        Assert.Equal(2, filtered.Count());
        Assert.Equal(inactive.Id, Assert.Single(BrandQueries.Page(filtered, parameters)).Id);
        Assert.Equal(active.Id, Assert.Single(BrandQueries.Filter(query, new BrandQueryParams { Search = "alpha", FeaturedOnly = true })).Id);
    }

    [Theory]
    [InlineData("name", false, "Alpha")]
    [InlineData("name", true, "Zulu")]
    [InlineData("slug", false, "Zulu")]
    [InlineData("slug", true, "Alpha")]
    [InlineData("displayOrder", false, "Zulu")]
    [InlineData("displayOrder", true, "Alpha")]
    [InlineData("unknown", false, "Zulu")]
    public void SortIsAppliedBeforePageLimit(string sortBy, bool descending, string expected)
    {
        var brands = new[] { Brand.Create("Alpha", "z", displayOrder: 2), Brand.Create("Zulu", "a", displayOrder: 1) }.AsQueryable();
        var parameters = new BrandQueryParams { SortBy = sortBy, SortDescending = descending, PageSize = 1 };
        Assert.Equal(expected, Assert.Single(BrandQueries.Page(brands, parameters)).Name);
    }

    [Fact]
    public void PostgreSqlTranslationContainsFiltersOrderingAndPaging()
    {
        using var context = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=query_translation_only;Username=test;Password=test").Options);
        var parameters = new BrandQueryParams { Search = "100%", FeaturedOnly = true, SortBy = "name", SortDescending = true, Page = 2, PageSize = 5 };
        var query = BrandQueries.Page(BrandQueries.Filter(context.Brands.AsNoTracking(), parameters), parameters);
        var sql = query.ToQueryString();
        Assert.Contains("WHERE", sql);
        Assert.Contains("IsDeleted", sql);
        Assert.Contains("IsActive", sql);
        Assert.Contains("IsFeatured", sql);
        Assert.Contains("lower(", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("DESC", sql);
        Assert.Contains("LIMIT", sql);
        Assert.Contains("OFFSET", sql);
    }
}
