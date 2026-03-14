using System.Linq.Expressions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.Filtering;

namespace BuildingBlocks.Application.Paging;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var normalizedPageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);
        var normalizedPage = Math.Max(page, 1);

        return query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize);
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, QueryParameters parameters) =>
        query.ApplyPaging(parameters.Page, parameters.PageSize);

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        IReadOnlyList<SortDescriptor>? sorts,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>>? defaultSort = null,
        bool defaultDesc = true)
    {
        if (sorts == null || sorts.Count == 0)
        {
            return ApplyDefaultSort(query, defaultSort, defaultDesc);
        }

        IOrderedQueryable<T>? ordered = null;

        foreach (var sort in sorts)
        {
            if (!sortMap.TryGetValue(sort.Field.ToLowerInvariant(), out var expr))
                continue;

            ordered = ordered == null
                ? OrderByDirection(query, expr, sort.Desc)
                : ThenByDirection(ordered, expr, sort.Desc);
        }

        return ordered ?? query;
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>>? defaultSort = null)
    {
        if (parameters.Sorts is { Count: > 0 })
            return query.ApplySorting(parameters.Sorts, sortMap, defaultSort, parameters.SortDescending);

        if (string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            return ApplyDefaultSort(query, defaultSort, parameters.SortDescending);
        }

        if (!sortMap.TryGetValue(parameters.SortBy.ToLowerInvariant(), out var sortExpr))
        {
            return ApplyDefaultSort(query, defaultSort, parameters.SortDescending);
        }

        return OrderByDirection(query, sortExpr, parameters.SortDescending);
    }

    private static IQueryable<T> ApplyDefaultSort<T>(
        IQueryable<T> query,
        Expression<Func<T, object>>? defaultSort,
        bool descending)
    {
        if (defaultSort == null)
            return query;

        return descending ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort);
    }

    private static IOrderedQueryable<T> OrderByDirection<T>(
        IQueryable<T> query,
        Expression<Func<T, object>> expr,
        bool descending)
    {
        return descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
    }

    private static IOrderedQueryable<T> ThenByDirection<T>(
        IOrderedQueryable<T> query,
        Expression<Func<T, object>> expr,
        bool descending)
    {
        return descending ? query.ThenByDescending(expr) : query.ThenBy(expr);
    }

    public static IQueryable<T> ApplyFiltering<T>(
        this IQueryable<T> query,
        FilterBuilder<T> filterBuilder) where T : class =>
        filterBuilder.Apply(query);
}
