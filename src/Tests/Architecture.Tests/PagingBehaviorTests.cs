using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Web.Helpers;
using Xunit;

namespace Architecture.Tests;

public sealed class PagingBehaviorTests
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Row, object>>> SortMap =
        new Dictionary<string, Expression<Func<Row, object>>>
        {
            ["CreatedAt"] = row => row.CreatedAt,
            ["Name"] = row => row.Name
        };

    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(-1, -20, 1, 10)]
    [InlineData(2, 500, 2, 100)]
    [InlineData(3, 20, 3, 20)]
    [InlineData(int.MaxValue, 100, 21474837, 100)]
    [InlineData(int.MaxValue, 1, int.MaxValue, 1)]
    public void PagingInputsAndMetadata_UseTheSameNormalization(int page, int size, int expectedPage, int expectedSize)
    {
        var parameters = QueryParameters.Create(page, size);
        var reversedInitialization = new QueryParameters { PageSize = size, Page = page };
        var empty = PaginatedResult<int>.Empty(page, size);

        Assert.Equal(expectedPage, parameters.Page);
        Assert.Equal(expectedSize, parameters.PageSize);
        Assert.Equal(parameters.Page, reversedInitialization.Page);
        Assert.Equal(parameters.Skip, reversedInitialization.Skip);
        Assert.Equal((long)(expectedPage - 1) * expectedSize, parameters.Skip);
        Assert.Equal(parameters.Page, empty.Page);
        Assert.Equal(parameters.PageSize, empty.PageSize);

        var data = Enumerable.Range(1, 250).AsQueryable();
        Assert.Equal(data.ApplyPaging(parameters), data.ApplyPaging(page, size));
    }

    [Fact]
    public void ExtremePage_DoesNotWrapToFirstPage()
    {
        var result = Enumerable.Range(1, 20).AsQueryable().ApplyPaging(int.MaxValue, 100).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void EmptyResult_UsesCommonDefaults()
    {
        var result = PaginatedResult<int>.Empty();
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Theory]
    [InlineData(-1, 0, 1, 10)]
    [InlineData(5, 500, 6, 100)]
    public void OffsetAdapter_IsBounded(int skip, int take, int first, int count)
    {
        var result = Enumerable.Range(1, 200).AsQueryable().ApplyOffsetPaging(skip, take).ToArray();
        Assert.Equal(first, result[0]);
        Assert.Equal(count, result.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("unknown")]
    [InlineData(" ")]
    public void MissingOrUnknownSort_UsesDefaultAndUniqueTieBreaker(string? field)
    {
        var input = Rows();
        var result = input.ApplySorting(new QueryParameters { SortBy = field }, SortMap,
            row => row.CreatedAt, tieBreaker: row => row.Id).ToArray();
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(row => row.Id));
    }

    [Fact]
    public void UnknownSortList_UsesDefaultOrdering()
    {
        var result = Rows().ApplySorting(new QueryParameters
        {
            Sorts = [new SortDescriptor { Field = "unknown" }]
        }, SortMap, row => row.CreatedAt, tieBreaker: row => row.Id);
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(row => row.Id));
    }

    [Fact]
    public void SortNames_AreCaseInsensitiveAndMultiSortPrecedesSingleSort()
    {
        var result = Rows().ApplySorting(new QueryParameters
        {
            SortBy = "CreatedAt",
            Sorts =
            [
                new SortDescriptor { Field = " nAmE " },
                new SortDescriptor { Field = "NAME", Desc = true },
                new SortDescriptor { Field = "unknown" },
                new SortDescriptor { Field = "CREATEDAT", Desc = false }
            ]
        }, SortMap, row => row.CreatedAt, tieBreaker: row => row.Id);
        Assert.Equal(new[] { 3, 1, 2 }, result.Select(row => row.Id));
    }

    [Fact]
    public void EqualSortValues_ProduceDisjointConsecutivePages()
    {
        var first = Rows().ApplySorting(new QueryParameters { SortBy = "createdat" }, SortMap,
            row => row.CreatedAt, tieBreaker: row => row.Id).ApplyPaging(1, 1).Single();
        var second = Rows().ApplySorting(new QueryParameters { SortBy = "createdat" }, SortMap,
            row => row.CreatedAt, tieBreaker: row => row.Id).ApplyPaging(2, 1).Single();
        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void MalformedSorts_AreValidatedAndCannotRemoveFallbackOrdering()
    {
        var parameters = new QueryParameters
        {
            SortBy = new string('x', 101),
            Sorts = [null!, new SortDescriptor { Field = "" }, new SortDescriptor { Field = new string('x', 101) }]
        };
        var errors = new QueryParametersValidator().Validate(parameters).Errors;
        Assert.Contains(errors, failure => failure.PropertyName == "SortBy");
        Assert.Contains(errors, failure => failure.PropertyName == "Sorts[0]");
        Assert.Contains(errors, failure => failure.PropertyName == "Sorts[1].Field");
        Assert.Contains(errors, failure => failure.PropertyName == "Sorts[2].Field");
        Assert.Equal(new[] { 1, 2, 3 }, Rows().ApplySorting(parameters, SortMap,
            row => row.CreatedAt, tieBreaker: row => row.Id).Select(row => row.Id));
    }

    [Fact]
    public void SortValidation_UsesJapaneseFieldLabels()
    {
        using var culture = new RequestCultureScope("ja-JP");
        var errors = new QueryParametersValidator().Validate(new QueryParameters
        {
            Sorts = [null!, new SortDescriptor { Field = "" }]
        }).Errors;
        var localized = errors.Select(error => ValidationErrorLocalizer.LocalizeMessage(error.ErrorMessage, null)).ToList();
        Assert.Contains(localized, message => message.Contains("並び順") && !message.Contains("Sort"));
        Assert.Contains(localized, message => message.Contains("並び替え項目") && !message.Contains("SortBy"));
    }

    private static IQueryable<Row> Rows() => new[]
    {
        new Row(2, "B", 2), new Row(3, "A", 1), new Row(1, "B", 2)
    }.AsQueryable();

    private sealed record Row(int Id, string Name, int CreatedAt);
}
