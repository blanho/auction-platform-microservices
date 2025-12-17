using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.Repository.Interfaces;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IQueryHandler<GetAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetAuctionsQueryHandler> _logger;

    public GetAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<GetAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auctions with filters - Status: {Status}, Seller: {Seller}, Page: {Page}",
            request.Status ?? "All", request.Seller ?? "All", request.PageNumber);

        try
        {
            var (items, totalCount) = await _repository.GetPagedAsync(
                status: request.Status,
                seller: request.Seller,
                winner: request.Winner,
                searchTerm: request.SearchTerm,
                category: request.Category,
                isFeatured: request.IsFeatured,
                orderBy: request.OrderBy,
                descending: request.Descending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken);

            var dtos = _mapper.Map<List<AuctionDto>>(items);

            var result = PagedResult<AuctionDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} auctions out of {Total}", dtos.Count, totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions: {Error}", ex.Message);
            return Result.Failure<PagedResult<AuctionDto>>(Error.Create("Auction.FetchFailed", $"Failed to fetch auctions: {ex.Message}"));
        }
    }
}
