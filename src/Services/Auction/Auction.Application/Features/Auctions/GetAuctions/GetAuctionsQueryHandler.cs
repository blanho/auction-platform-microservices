using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IQueryHandler<GetAuctionsQuery, PaginatedResult<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuctionsQueryHandler> _logger;

    public GetAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<GetAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching auctions with filters - Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status ?? "All", request.Page, request.PageSize);

        var queryParams = new AuctionFilterDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.OrderBy,
            SortDescending = request.Descending,
            Filter = new AuctionFilter
            {
                Status = request.Status,
                Seller = request.Seller,
                Winner = request.Winner,
                SearchTerm = request.SearchTerm,
                Category = request.Category,
                IsFeatured = request.IsFeatured
            }
        };

        var result = await _repository.GetPagedAsync(queryParams, cancellationToken);

        var dtos = result.Items.Select(auction => _mapper.Map<AuctionDto>(auction)).ToList();

        var paginatedResult = new PaginatedResult<AuctionDto>(dtos, result.TotalCount, request.Page, request.PageSize);

        _logger.LogDebug("Retrieved {Count} auctions out of {Total}", dtos.Count, result.TotalCount);

        return Result.Success(paginatedResult);
    }
}

