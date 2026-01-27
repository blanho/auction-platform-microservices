using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Features.Brands.GetBrands;

public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBrandsQueryHandler> _logger;

    public GetBrandsQueryHandler(
        IBrandRepository repository,
        IMapper mapper,
        ILogger<GetBrandsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<BrandDto>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching brands - ActiveOnly: {ActiveOnly}, FeaturedOnly: {FeaturedOnly}",
            request.ActiveOnly, request.FeaturedOnly);

        try
        {
            var brands = request.FeaturedOnly
                ? await _repository.GetFeaturedBrandsAsync(request.Count ?? 10, cancellationToken)
                : await _repository.GetAllAsync(!request.ActiveOnly, cancellationToken);

            var dtos = brands.Select(b => _mapper.Map<BrandDto>(b)).ToList();

            _logger.LogInformation("Successfully fetched {Count} brands", dtos.Count);

            return Result<List<BrandDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching brands");
            return Result.Failure<List<BrandDto>>(AuctionErrors.Brand.FetchError(ex.Message));
        }
    }
}

