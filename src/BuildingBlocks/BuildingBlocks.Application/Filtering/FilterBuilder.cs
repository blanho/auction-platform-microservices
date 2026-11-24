using System.Linq.Expressions;

namespace BuildingBlocks.Application.Filtering;

public sealed class FilterBuilder<T> where T : class
{
    private readonly List<Expression<Func<T, bool>>> _predicates = [];

    public FilterBuilder<T> When(bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
            _predicates.Add(predicate);
        return this;
    }

    public FilterBuilder<T> WhenNotNull<TValue>(TValue? value, Expression<Func<T, bool>> predicate)
        => When(value is not null, predicate);

    public FilterBuilder<T> WhenHasValue<TValue>(TValue? value, Expression<Func<T, bool>> predicate) where TValue : struct
        => When(value.HasValue, predicate);

    public FilterBuilder<T> WhenNotEmpty(string? value, Expression<Func<T, bool>> predicate)
        => When(!string.IsNullOrWhiteSpace(value), predicate);

    public FilterBuilder<T> WhenNotEmpty<TCollection>(IEnumerable<TCollection>? collection, Expression<Func<T, bool>> predicate)
        => When(collection?.Any() == true, predicate);

    public FilterBuilder<T> WhenInRange<TValue>(TValue? from, TValue? to, Expression<Func<T, bool>> predicate)
        where TValue : struct
        => When(from.HasValue || to.HasValue, predicate);

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        foreach (var predicate in _predicates)
            query = query.Where(predicate);
        return query;
    }

    public IReadOnlyList<Expression<Func<T, bool>>> Build() => _predicates.AsReadOnly();

    public static FilterBuilder<T> Create() => new();
}
