using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Queries.GetMyAuctions;

public class GetMyAuctionsQueryHandler : IQueryHandler<GetMyAuctionsQuery, PaginatedResult<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyAuctionsQueryHandler> _logger;

    public GetMyAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<GetMyAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AuctionDto>>> Handle(GetMyAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching user auctions - Status: {Status}, Page: {Page}",
            request.Status ?? "All", request.Page);

        var queryParams = new AuctionFilterDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.OrderBy,
            SortDescending = request.Descending,
            Filter = new AuctionFilter
            {
                Status = request.Status,
                Seller = request.Username,
                SearchTerm = request.SearchTerm
            }
        };

        var result = await _repository.GetPagedAsync(queryParams, cancellationToken);

        var dtos = result.Items.Select(a => _mapper.Map<AuctionDto>(a)).ToList();

        var paginatedResult = new PaginatedResult<AuctionDto>(dtos, result.TotalCount, request.Page, request.PageSize);

        _logger.LogDebug("Retrieved {Count} auctions out of {Total}",
            dtos.Count, result.TotalCount);

        return Result.Success(paginatedResult);
    }
}

