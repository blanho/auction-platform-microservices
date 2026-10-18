using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Application.Paging;

public sealed class SortMapBuilder<TEntity, TContract>
{
    private readonly Dictionary<string, Expression<Func<TEntity, object>>> _fields =
        new(StringComparer.OrdinalIgnoreCase);

    public SortMapBuilder<TEntity, TContract> Add<TField>(
        Expression<Func<TContract, TField>> field,
        Expression<Func<TEntity, TField>> selector)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(selector);

        if (field.Body is not MemberExpression { Member: PropertyInfo property, Expression: ParameterExpression parameter } ||
            parameter != field.Parameters[0])
            throw new ArgumentException("A direct contract property is required.", nameof(field));

        var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
        var expression = Expression.Lambda<Func<TEntity, object>>(
            Expression.Convert(selector.Body, typeof(object)), selector.Parameters);

        if (!_fields.TryAdd(name, expression))
            throw new ArgumentException("Sort fields must have unique public names.", nameof(field));

        return this;
    }

    public IReadOnlyDictionary<string, Expression<Func<TEntity, object>>> Build() =>
        _fields.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
