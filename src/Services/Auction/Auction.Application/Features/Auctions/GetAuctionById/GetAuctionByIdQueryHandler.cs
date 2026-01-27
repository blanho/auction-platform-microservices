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
    private readonly ICacheService _cache;
    private readonly ILogger<GetAuctionByIdQueryHandler> _logger;
    
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "auction:";

    public GetAuctionByIdQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ICacheService cache,
        ILogger<GetAuctionByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<AuctionDto>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auction {AuctionId}", request.Id);

        try
        {
            var cacheKey = $"{CacheKeyPrefix}{request.Id}";
            var cachedDto = await _cache.GetAsync<AuctionDto>(cacheKey, cancellationToken);
            
            if (cachedDto != null)
            {
                _logger.LogDebug("Auction {AuctionId} retrieved from cache", request.Id);
                return Result.Success(cachedDto);
            }

            var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found", request.Id);
                return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.Id));
            }

            var dto = _mapper.Map<AuctionDto>(auction);
            await _cache.SetAsync(cacheKey, dto, CacheDuration, cancellationToken);
            
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auction {AuctionId}: {Error}", request.Id, ex.Message);
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.FetchFailed(ex.Message));
        }
    }
}

