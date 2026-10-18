using System.Linq.Expressions;
using BuildingBlocks.Application.Filtering;

namespace BuildingBlocks.Application.Paging;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.ApplyPaging(QueryParameters.Create(page, pageSize));
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, QueryParameters parameters) =>
        query.Skip(parameters.Skip).Take(parameters.Take);

    public static IQueryable<T> ApplyOffsetPaging<T>(this IQueryable<T> query, int skip, int take) =>
        query.Skip(Math.Max(skip, 0)).Take(QueryParameters.Create(1, take).Take);

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        IReadOnlyList<SortDescriptor>? sorts,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>>? defaultSort = null,
        bool defaultDesc = true,
        Expression<Func<T, object>>? tieBreaker = null)
    {
        IOrderedQueryable<T>? ordered = null;
        var appliedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sort in sorts ?? [])
        {
            if (sort is null || string.IsNullOrWhiteSpace(sort.Field))
                continue;

            var field = sort.Field.Trim();
            var expr = sortMap.FirstOrDefault(entry =>
                string.Equals(entry.Key, field, StringComparison.OrdinalIgnoreCase)).Value;
            if (expr is null || !appliedFields.Add(field))
                continue;

            ordered = ordered == null
                ? OrderByDirection(query, expr, sort.Desc)
                : ThenByDirection(ordered, expr, sort.Desc);
        }

        if (ordered is null && defaultSort is not null)
            ordered = OrderByDirection(query, defaultSort, defaultDesc);

        if (tieBreaker is not null)
            return ordered is null ? query.OrderBy(tieBreaker) : ordered.ThenBy(tieBreaker);

        return ordered ?? query;
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>>? defaultSort = null,
        Expression<Func<T, object>>? tieBreaker = null)
    {
        IReadOnlyList<SortDescriptor>? sorts = parameters.Sorts is { Count: > 0 }
            ? parameters.Sorts
            : string.IsNullOrWhiteSpace(parameters.SortBy)
                ? null
                : [new SortDescriptor { Field = parameters.SortBy, Desc = parameters.SortDescending }];
        return query.ApplySorting(sorts, sortMap, defaultSort, parameters.SortDescending, tieBreaker);
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
