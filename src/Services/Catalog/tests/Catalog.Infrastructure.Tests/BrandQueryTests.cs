using Catalog.Application.Filtering;
using BuildingBlocks.Application.Paging;
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
    [InlineData("unknown", true, "Zulu")]
    [InlineData(" DISPLAYORDER ", false, "Zulu")]
    public void SortIsAppliedBeforePageLimit(string sortBy, bool descending, string expected)
    {
        var brands = new[] { Brand.Create("Alpha", "z", displayOrder: 2), Brand.Create("Zulu", "a", displayOrder: 1) }.AsQueryable();
        var parameters = new BrandQueryParams { SortBy = sortBy, SortDescending = descending, PageSize = 1 };
        Assert.Equal(expected, Assert.Single(BrandQueries.Page(brands, parameters)).Name);
    }

    [Fact]
    public void DisplayOrderSorting_KeepsNameAscendingAndStableIdTies()
    {
        var first = Brand.Create("Alpha", "first", displayOrder: 2);
        var second = Brand.Create("Alpha", "second", displayOrder: 2);
        var last = Brand.Create("Zulu", "last", displayOrder: 2);
        var lower = Brand.Create("Other", "lower", displayOrder: 1);
        var brands = new[] { last, second, lower, first }.AsQueryable();
        var parameters = new BrandQueryParams { SortBy = "displayOrder", SortDescending = true };

        var actual = BrandQueries.Page(brands, parameters).Select(brand => brand.Id);

        Assert.Equal(new[] { first, second }.OrderBy(brand => brand.Id).Select(brand => brand.Id)
            .Concat(new[] { last.Id, lower.Id }), actual);
    }

    [Fact]
    public void MultiSort_TakesPrecedenceAndIgnoresUnsupportedAndDuplicateFields()
    {
        var first = Brand.Create("Alpha", "a", displayOrder: 1);
        var second = Brand.Create("Zulu", "z", displayOrder: 1);
        var third = Brand.Create("Other", "other", displayOrder: 2);
        var brands = new[] { second, third, first }.AsQueryable();
        var parameters = new BrandQueryParams
        {
            SortBy = "name",
            SortDescending = true,
            Sorts =
            [
                new SortDescriptor { Field = "unsupported" },
                new SortDescriptor { Field = "displayOrder", Desc = false },
                new SortDescriptor { Field = "NAME", Desc = true },
                new SortDescriptor { Field = "name", Desc = false }
            ]
        };

        Assert.Equal(new[] { second.Id, first.Id, third.Id }, BrandQueries.Page(brands, parameters).Select(brand => brand.Id));
        var unsupported = new BrandQueryParams
        {
            SortBy = "name", SortDescending = true,
            Sorts = [new SortDescriptor { Field = "unsupported" }]
        };
        Assert.Equal(new[] { first.Id, second.Id, third.Id }, BrandQueries.Page(brands, unsupported).Select(brand => brand.Id));
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
