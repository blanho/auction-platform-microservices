namespace Common.Utilities.Extensions;

/// <summary>
/// Extension methods for collections
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Check if collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Return empty enumerable if null
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Execute action for each item in collection
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }

    /// <summary>
    /// Execute async action for each item in collection sequentially
    /// </summary>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> collection,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in collection)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(item);
        }
    }

    /// <summary>
    /// Execute async action for each item in collection with concurrency limit
    /// </summary>
    public static async Task ForEachAsync<T>(
        this IEnumerable<T> collection,
        Func<T, Task> action,
        int maxConcurrency,
        CancellationToken cancellationToken = default)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = collection.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await action(item);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Split collection into batches of specified size
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        var batch = new List<T>(batchSize);
        foreach (var item in collection)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    /// <summary>
    /// Distinct by selector
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> collection,
        Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var item in collection)
        {
            if (seenKeys.Add(keySelector(item)))
            {
                yield return item;
            }
        }
    }
}
