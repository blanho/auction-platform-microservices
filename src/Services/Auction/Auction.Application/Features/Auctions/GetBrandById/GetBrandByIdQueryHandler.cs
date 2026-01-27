using Auction.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Queries.GetBrandById;

public class GetBrandByIdQueryHandler : IQueryHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBrandByIdQueryHandler> _logger;

    public GetBrandByIdQueryHandler(
        IBrandRepository repository,
        IMapper mapper,
        ILogger<GetBrandByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching brand {BrandId}", request.Id);

        try
        {
            var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (brand == null)
            {
                return Result.Failure<BrandDto>(AuctionErrors.Brand.NotFoundById(request.Id));
            }

            var dto = _mapper.Map<BrandDto>(brand);

            _logger.LogInformation("Successfully fetched brand {BrandId}", request.Id);

            return Result<BrandDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching brand {BrandId}", request.Id);
            return Result.Failure<BrandDto>(AuctionErrors.Brand.FetchError(ex.Message));
        }
    }
}

