using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Domain.Models;
using Search.Application.Interfaces;

namespace Search.Application.Features.Auctions.Queries.Autocomplete;

public record AutocompleteQuery(string Prefix, int MaxSuggestions = 10) : IRequest<Result<IReadOnlyList<AutocompleteSuggestion>>>;

public class AutocompleteQueryHandler : IRequestHandler<AutocompleteQuery, Result<IReadOnlyList<AutocompleteSuggestion>>>
{
    private readonly IAuctionSearchService _searchService;

    public AutocompleteQueryHandler(IAuctionSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<IReadOnlyList<AutocompleteSuggestion>>> Handle(AutocompleteQuery request, CancellationToken cancellationToken)
    {
        var result = await _searchService.AutocompleteAsync(request.Prefix, request.MaxSuggestions, cancellationToken);
        return Result.Success(result);
    }
}
