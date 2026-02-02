using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
namespace Auctions.Application.Queries.GetAuctionById;

public class GetAuctionByIdQueryHandler : IQueryHandler<GetAuctionByIdQuery, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuctionByIdQueryHandler> _logger;

    public GetAuctionByIdQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<GetAuctionByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuctionDto>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching auction {AuctionId}", request.Id);

        // Repository already handles caching via CachedAuctionRepository decorator
        var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogDebug("Auction {AuctionId} not found", request.Id);
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.Id));
        }

        var dto = _mapper.Map<AuctionDto>(auction);
        return Result.Success(dto);
    }
}

