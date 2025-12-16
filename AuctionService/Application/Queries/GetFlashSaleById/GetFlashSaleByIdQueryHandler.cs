using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetFlashSaleById;

public class GetFlashSaleByIdQueryHandler : IQueryHandler<GetFlashSaleByIdQuery, FlashSaleDto>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetFlashSaleByIdQueryHandler> _logger;

    public GetFlashSaleByIdQueryHandler(
        IFlashSaleRepository repository,
        IMapper mapper,
        IAppLogger<GetFlashSaleByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<FlashSaleDto>> Handle(GetFlashSaleByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching flash sale {FlashSaleId}", request.Id);

        try
        {
            var flashSale = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
            if (flashSale == null)
            {
                return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.NotFound", $"Flash sale with ID '{request.Id}' was not found"));
            }

            var dto = _mapper.Map<FlashSaleDto>(flashSale);

            _logger.LogInformation("Successfully fetched flash sale {FlashSaleId}", request.Id);

            return Result<FlashSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flash sale {FlashSaleId}", request.Id);
            return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.FetchError", $"Error fetching flash sale: {ex.Message}"));
        }
    }
}
