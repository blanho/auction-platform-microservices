using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
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
        _logger.LogInformation("Fetching auctions with filters - Status: {Status}, Seller: {Seller}, Page: {Page}",
            request.Status ?? "All", request.Seller ?? "All", request.PageNumber);

        try
        {
            var filter = new AuctionFilterDto
            {
                Status = request.Status,
                Seller = request.Seller,
                Winner = request.Winner,
                SearchTerm = request.SearchTerm,
                Category = request.Category,
                IsFeatured = request.IsFeatured,
                OrderBy = request.OrderBy,
                Descending = request.Descending,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = await _repository.GetPagedAsync(filter, cancellationToken);

            var dtos = result.Items.Select(auction => _mapper.Map<AuctionDto>(auction)).ToList();

            var paginatedResult = new PaginatedResult<AuctionDto>(dtos, result.TotalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} auctions out of {Total}", dtos.Count, result.TotalCount);

            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions: {Error}", ex.Message);
            return Result.Failure<PaginatedResult<AuctionDto>>(AuctionErrors.Auction.FetchFailed(ex.Message));
        }
    }
}

