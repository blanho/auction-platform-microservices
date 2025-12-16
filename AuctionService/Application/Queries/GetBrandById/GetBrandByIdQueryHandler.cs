using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetBrandById;

public class GetBrandByIdQueryHandler : IQueryHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetBrandByIdQueryHandler> _logger;

    public GetBrandByIdQueryHandler(
        IBrandRepository repository,
        IMapper mapper,
        IAppLogger<GetBrandByIdQueryHandler> logger)
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
                return Result.Failure<BrandDto>(Error.Create("Brand.NotFound", $"Brand with ID '{request.Id}' was not found"));
            }

            var dto = _mapper.Map<BrandDto>(brand);

            _logger.LogInformation("Successfully fetched brand {BrandId}", request.Id);

            return Result<BrandDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching brand {BrandId}", request.Id);
            return Result.Failure<BrandDto>(Error.Create("Brand.FetchError", $"Error fetching brand: {ex.Message}"));
        }
    }
}
