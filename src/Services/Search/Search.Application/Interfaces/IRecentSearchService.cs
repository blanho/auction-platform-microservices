namespace Search.Application.Interfaces;

public interface IRecentSearchService
{
    List<string> GetRecentSearches(string userId);
    void AddRecentSearch(string userId, string query);
    void ClearRecentSearches(string userId);
    List<string> GetPopularSearches();
}
