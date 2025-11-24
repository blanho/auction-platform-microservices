using SearchService.Application.DTOs;

namespace SearchService.Application.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken);
        Task<List<SearchItemDto>> GetAllItemsAsync(CancellationToken cancellationToken);
        Task<SearchItemDto> GetItemByIdAsync(Guid id, CancellationToken cancellationToken);
        Task ReindexAllAsync(CancellationToken cancellationToken);
    }
}