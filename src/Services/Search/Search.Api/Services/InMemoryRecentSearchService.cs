using System.Collections.Concurrent;
using Search.Api.Constants;

namespace Search.Api.Services;

public class InMemoryRecentSearchService : IRecentSearchService
{
    private readonly ConcurrentDictionary<string, List<string>> _recentSearches = new();
    private readonly ConcurrentDictionary<string, int> _searchCounts = new();

    public List<string> GetRecentSearches(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<string>();

        return _recentSearches.TryGetValue(userId, out var searches)
            ? searches.ToList()
            : new List<string>();
    }

    public void AddRecentSearch(string userId, string query)
    {
        if (IsInvalidInput(userId, query))
            return;

        var normalizedQuery = NormalizeSearchQuery(query);

        UpdateUserRecentSearches(userId, normalizedQuery);
        IncrementSearchCount(normalizedQuery);
    }

    public void ClearRecentSearches(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        _recentSearches.TryRemove(userId, out _);
    }

    public List<string> GetPopularSearches()
    {
        return _searchCounts
            .OrderByDescending(x => x.Value)
            .Take(SearchDefaults.MaxPopularSearches)
            .Select(x => x.Key)
            .ToList();
    }

    private static bool IsInvalidInput(string userId, string query) =>
        string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(query);

    private static string NormalizeSearchQuery(string query) =>
        query.Trim().ToLowerInvariant();

    private void UpdateUserRecentSearches(string userId, string normalizedQuery)
    {
        _recentSearches.AddOrUpdate(
            userId,
            _ => new List<string> { normalizedQuery },
            (_, existingSearches) => UpdateSearchList(existingSearches, normalizedQuery));
    }

    private static List<string> UpdateSearchList(List<string> searches, string query)
    {
        searches.Remove(query);
        searches.Insert(0, query);

        if (searches.Count > SearchDefaults.MaxRecentSearchesPerUser)
        {
            searches.RemoveAt(searches.Count - 1);
        }

        return searches;
    }

    private void IncrementSearchCount(string normalizedQuery)
    {
        _searchCounts.AddOrUpdate(normalizedQuery, 1, (_, count) => count + 1);
    }
}
