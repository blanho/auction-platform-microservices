namespace Common.Utilities.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }

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
