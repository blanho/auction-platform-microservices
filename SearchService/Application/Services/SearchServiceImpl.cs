using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using AutoMapper;
using Common.Core.Exceptions;
using Common.Core.Interfaces;
using Common.Repository.Interfaces;
using System.Diagnostics;

namespace SearchService.Application.Services;

public class SearchServiceImpl : ISearchService
{
    private readonly ISearchItemRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<SearchServiceImpl> _logger;
    private readonly IDateTimeProvider _dateTime;

    public SearchServiceImpl(
        ISearchItemRepository repository, 
        IMapper mapper,
        IAppLogger<SearchServiceImpl> logger,
        IDateTimeProvider dateTime)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Performing search with query: {Query}, Category: {Category}, Page: {Page}", 
            request.Query, request.Category, request.Page);

        var skip = (request.Page - 1) * request.PageSize;
        
        var items = await _repository.SearchAsync(
            request.Query, 
            request.Category, 
            request.MinPrice, 
            request.MaxPrice, 
            request.Status, 
            request.Source, 
            skip, 
            request.PageSize, 
            cancellationToken);

        var totalCount = await _repository.GetSearchCountAsync(
            request.Query, 
            request.Category, 
            request.MinPrice, 
            request.MaxPrice, 
            request.Status, 
            request.Source, 
            cancellationToken);

        stopwatch.Stop();

        var result = new SearchResultDto
        {
            Items = items.Select(i => _mapper.Map<SearchItemDto>(i)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page * request.PageSize < totalCount,
            HasPreviousPage = request.Page > 1,
            Query = request.Query,
            SearchTime = stopwatch.Elapsed
        };

        _logger.LogInformation("Search completed in {ElapsedMs}ms, found {TotalCount} items", 
            stopwatch.ElapsedMilliseconds, totalCount);

        return result;
    }

    public async Task<List<SearchItemDto>> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all search items at {Timestamp}", _dateTime.UtcNow);

        var items = await _repository.GetAllAsync(cancellationToken);
        var result = items.Select(i => _mapper.Map<SearchItemDto>(i)).ToList();
        
        _logger.LogInformation("Retrieved {Count} search items", result.Count);
        return result;
    }

    public async Task<SearchItemDto> GetItemByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching search item {ItemId}", id);

        var item = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (item == null)
        {
            _logger.LogWarning("Search item {ItemId} not found", id);
            throw new NotFoundException($"Search item with ID {id} was not found");
        }
        
        return _mapper.Map<SearchItemDto>(item);
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting reindex of all search items at {Timestamp}", _dateTime.UtcNow);

        var items = await _repository.GetAllAsync(cancellationToken);
        
        // Update metadata for all items
        foreach (var item in items)
        {
            if (item.Metadata == null)
            {
                item.Metadata = new Domain.Entities.SearchMetadata
                {
                    SearchItemId = item.Id
                };
            }

            item.Metadata.LastIndexed = _dateTime.UtcNow;
            item.Metadata.IndexedAt = _dateTime.UtcNow;
            item.Metadata.SearchVector = $"{item.Make} {item.Model} {item.Color} {item.Year}".ToLowerInvariant();
            
            await _repository.UpdateAsync(item, cancellationToken);
        }
        
        _logger.LogInformation("Completed reindex of {Count} search items", items.Count);
    }
}