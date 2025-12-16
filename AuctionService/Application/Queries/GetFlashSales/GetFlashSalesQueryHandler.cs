using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetFlashSales;

public class GetFlashSalesQueryHandler : IQueryHandler<GetFlashSalesQuery, List<FlashSaleDto>>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetFlashSalesQueryHandler> _logger;

    public GetFlashSalesQueryHandler(
        IFlashSaleRepository repository,
        IMapper mapper,
        IAppLogger<GetFlashSalesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<FlashSaleDto>>> Handle(GetFlashSalesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching flash sales - ActiveOnly: {ActiveOnly}", request.ActiveOnly);

        try
        {
            var flashSales = await _repository.GetAllAsync(!request.ActiveOnly, cancellationToken);

            var dtos = _mapper.Map<List<FlashSaleDto>>(flashSales);

            _logger.LogInformation("Successfully fetched {Count} flash sales", dtos.Count);

            return Result<List<FlashSaleDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flash sales");
            return Result.Failure<List<FlashSaleDto>>(Error.Create("FlashSales.FetchError", $"Error fetching flash sales: {ex.Message}"));
        }
    }
}
