using System.Linq.Expressions;

namespace BuildingBlocks.Application.Paging;

public static class QueryExtensions
{
    private const int MaxPageSize = 100;

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var normalizedPageSize = Math.Min(pageSize, MaxPageSize);
        var normalizedPage = Math.Max(page, 1);

        return query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize);
    }

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
}
