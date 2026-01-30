using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
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
        _logger.LogInformation("Fetching auctions for user {Username} - Status: {Status}, Page: {Page}",
            request.Username, request.Status ?? "All", request.Page);

        try
        {
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

            _logger.LogInformation("Retrieved {Count} auctions for user {Username} out of {Total}", 
                dtos.Count, request.Username, result.TotalCount);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions for user {Username}: {Error}", request.Username, ex.Message);
            return Result.Failure<PaginatedResult<AuctionDto>>(AuctionErrors.Auction.FetchFailed(ex.Message));
        }
    }
}

