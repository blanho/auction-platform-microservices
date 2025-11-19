using Auctions.Application.DTOs;
using Auctions.Application.Specifications;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
namespace Auctions.Application.Queries.GetMyAuctions;

public class GetMyAuctionsQueryHandler : IQueryHandler<GetMyAuctionsQuery, PaginatedResult<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetMyAuctionsQueryHandler> _logger;

    public GetMyAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<GetMyAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<AuctionDto>>> Handle(GetMyAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auctions for user {Username} - Status: {Status}, Page: {Page}",
            request.Username, request.Status ?? "All", request.PageNumber);

        try
        {
            var (auctions, totalCount) = await _repository.GetPagedAsync(
                status: request.Status,
                seller: request.Username,
                searchTerm: request.SearchTerm,
                orderBy: request.OrderBy,
                descending: request.Descending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken);

            var dtos = _mapper.Map<List<AuctionDto>>(auctions);

            var result = new PaginatedResult<AuctionDto>(dtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} auctions for user {Username} out of {Total}", 
                dtos.Count, request.Username, totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions for user {Username}: {Error}", request.Username, ex.Message);
            return Result.Failure<PaginatedResult<AuctionDto>>(Error.Create("Auction.FetchFailed", $"Failed to fetch auctions: {ex.Message}"));
        }
    }
}

