using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Application.Specifications;
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
            var allAuctions = await _repository.GetAllAsync(cancellationToken);

            var query = allAuctions.AsQueryable();

            var filterSpec = new AuctionFilterSpecification(
                request.Status,
                request.Seller,
                request.Winner,
                request.SearchTerm,
                request.Category,
                request.IsFeatured);

            query = query.Where(filterSpec.Criteria);

            query = query.ApplySorting(request.OrderBy, request.Descending);

            var totalCount = query.Count();

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

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
