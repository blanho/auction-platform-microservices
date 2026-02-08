using System.Collections.Concurrent;

namespace Search.Api.Services;

public class InMemoryRecentSearchService : IRecentSearchService
{
    private readonly ConcurrentDictionary<string, List<string>> _recentSearches = new();
    private readonly ConcurrentDictionary<string, int> _searchCounts = new();
    private const int MaxRecentSearches = 10;
    private const int MaxPopularSearches = 10;

    public List<string> GetRecentSearches(string userId)
    {
        return _recentSearches.TryGetValue(userId, out var searches)
            ? searches.ToList()
            : new List<string>();
    }

    public void AddRecentSearch(string userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        _recentSearches.AddOrUpdate(
            userId,
            _ => new List<string> { normalizedQuery },
            (_, existing) =>
            {
                existing.Remove(normalizedQuery);
                existing.Insert(0, normalizedQuery);
                if (existing.Count > MaxRecentSearches)
                {
                    existing.RemoveAt(existing.Count - 1);
                }
                return existing;
            });

        _searchCounts.AddOrUpdate(normalizedQuery, 1, (_, count) => count + 1);
    }

    public void ClearRecentSearches(string userId)
    {
        _recentSearches.TryRemove(userId, out _);
    }

    public List<string> GetPopularSearches()
    {
        return _searchCounts
            .OrderByDescending(x => x.Value)
            .Take(MaxPopularSearches)
            .Select(x => x.Key)
            .ToList();
    }
}
