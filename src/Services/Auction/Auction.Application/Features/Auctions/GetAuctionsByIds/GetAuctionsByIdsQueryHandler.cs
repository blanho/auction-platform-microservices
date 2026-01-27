using Auction.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Queries.GetAuctionsByIds;

public class GetAuctionsByIdsQueryHandler : IQueryHandler<GetAuctionsByIdsQuery, IEnumerable<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuctionsByIdsQueryHandler> _logger;

    public GetAuctionsByIdsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<GetAuctionsByIdsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<AuctionDto>>> Handle(GetAuctionsByIdsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching {Count} auctions by IDs", request.Ids.Count());

        try
        {
            var idList = request.Ids.ToList();
            if (idList.Count == 0)
            {
                return Result.Success(Enumerable.Empty<AuctionDto>());
            }

            var auctions = await _repository.GetByIdsAsync(idList, cancellationToken);
            var dtos = auctions.Select(a => _mapper.Map<AuctionDto>(a)).ToList();
            return Result.Success<IEnumerable<AuctionDto>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions by IDs: {Error}", ex.Message);
            return Result.Failure<IEnumerable<AuctionDto>>(AuctionErrors.Auction.FetchFailed(ex.Message));
        }
    }
}

