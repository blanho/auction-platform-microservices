using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Application.Specifications;
using AutoMapper;
using Common.Core.Helpers;
using Common.Repository.Interfaces;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetMyAuctions;

public class GetMyAuctionsQueryHandler : IQueryHandler<GetMyAuctionsQuery, PagedResult<AuctionDto>>
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

    public async Task<Result<PagedResult<AuctionDto>>> Handle(GetMyAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auctions for user {Username} - Status: {Status}, Page: {Page}",
            request.Username, request.Status ?? "All", request.PageNumber);

        try
        {
            var allAuctions = await _repository.GetAllAsync(cancellationToken);

            var query = allAuctions.AsQueryable()
                .Where(a => a.SellerUsername.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

            var filterSpec = new AuctionFilterSpecification(
                status: request.Status,
                searchTerm: request.SearchTerm);

            query = query.Where(filterSpec.Criteria);

            query = query.ApplySorting(request.OrderBy, request.Descending);

            var totalCount = query.Count();

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<AuctionDto>>(items);

            var result = PagedResult<AuctionDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} auctions for user {Username} out of {Total}", 
                dtos.Count, request.Username, totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions for user {Username}: {Error}", request.Username, ex.Message);
            return Result.Failure<PagedResult<AuctionDto>>(Error.Create("Auction.FetchFailed", $"Failed to fetch auctions: {ex.Message}"));
        }
    }
}
