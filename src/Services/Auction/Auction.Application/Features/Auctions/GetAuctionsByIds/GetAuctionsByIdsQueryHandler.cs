using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Repository;

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
        var idList = request.Ids.ToList();
        _logger.LogDebug("Fetching {Count} auctions by IDs", idList.Count);

        if (idList.Count == 0)
        {
            return Result.Success(Enumerable.Empty<AuctionDto>());
        }

        var auctions = await _repository.GetByIdsAsync(idList, cancellationToken);
        var dtos = auctions.Select(a => _mapper.Map<AuctionDto>(a)).ToList();
        return Result.Success<IEnumerable<AuctionDto>>(dtos);
    }
}

