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
            return defaultSort != null
                ? (defaultDesc ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort))
                : query;
        }

        IOrderedQueryable<T>? ordered = null;

        foreach (var sort in sorts)
        {
            if (!sortMap.TryGetValue(sort.Field.ToLowerInvariant(), out var expr))
                continue;

            ordered = ordered == null
                ? (sort.Desc ? query.OrderByDescending(expr) : query.OrderBy(expr))
                : (sort.Desc ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr));
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
            return defaultSort != null
                ? (parameters.SortDescending ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort))
                : query;
        }

        if (!sortMap.TryGetValue(parameters.SortBy.ToLowerInvariant(), out var sortExpr))
        {
            return defaultSort != null
                ? (parameters.SortDescending ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort))
                : query;
        }

        return parameters.SortDescending
            ? query.OrderByDescending(sortExpr)
            : query.OrderBy(sortExpr);
    }

    public static IQueryable<T> ApplyFiltering<T>(
        this IQueryable<T> query,
        FilterBuilder<T> filterBuilder) where T : class =>
        filterBuilder.Apply(query);
}
