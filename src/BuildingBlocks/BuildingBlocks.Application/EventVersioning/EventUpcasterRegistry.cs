namespace BuildingBlocks.Application.EventVersioning;

public interface IEventUpcaster<in TSource, out TTarget>
    where TSource : class
    where TTarget : class
{
    TTarget Upcast(TSource source);
}

public interface IEventUpcasterRegistry
{
    T Upcast<T>(object sourceEvent) where T : class;
    bool CanUpcast(Type sourceType, Type targetType);
}

public sealed class EventUpcasterRegistry : IEventUpcasterRegistry
{
    private readonly Dictionary<(Type Source, Type Target), Func<object, object>> _upcasters = new();

    public void Register<TSource, TTarget>(IEventUpcaster<TSource, TTarget> upcaster)
        where TSource : class
        where TTarget : class
    {
        _upcasters[(typeof(TSource), typeof(TTarget))] = source => upcaster.Upcast((TSource)source);
    }

    public void Register<TSource, TTarget>(Func<TSource, TTarget> upcastFunc)
        where TSource : class
        where TTarget : class
    {
        _upcasters[(typeof(TSource), typeof(TTarget))] = source => upcastFunc((TSource)source)!;
    }

    public T Upcast<T>(object sourceEvent) where T : class
    {
        var sourceType = sourceEvent.GetType();
        var targetType = typeof(T);

        if (sourceType == targetType)
            return (T)sourceEvent;

        if (_upcasters.TryGetValue((sourceType, targetType), out var upcaster))
            return (T)upcaster(sourceEvent);

        throw new InvalidOperationException(
            $"No upcaster registered from {sourceType.Name} to {targetType.Name}");
    }

    public bool CanUpcast(Type sourceType, Type targetType)
    {
        return sourceType == targetType || _upcasters.ContainsKey((sourceType, targetType));
    }
}
