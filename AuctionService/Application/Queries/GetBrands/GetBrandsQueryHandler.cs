using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetBrands;

public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetBrandsQueryHandler> _logger;

    public GetBrandsQueryHandler(
        IBrandRepository repository,
        IMapper mapper,
        IAppLogger<GetBrandsQueryHandler> logger)
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

            var dtos = _mapper.Map<List<BrandDto>>(brands);

            _logger.LogInformation("Successfully fetched {Count} brands", dtos.Count);

            return Result<List<BrandDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching brands");
            return Result.Failure<List<BrandDto>>(Error.Create("Brands.FetchError", $"Error fetching brands: {ex.Message}"));
        }
    }
}
