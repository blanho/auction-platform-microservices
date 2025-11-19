namespace BuildingBlocks.Application.Filtering;

public static class FilterExtensions
{
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        params IFilter<T>[] filters)
    {
        foreach (var filter in filters)
            query = filter.Apply(query);

        return query;
    }

    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IEnumerable<IFilter<T>> filters)
    {
        foreach (var filter in filters)
            query = filter.Apply(query);

        return query;
    }
}
