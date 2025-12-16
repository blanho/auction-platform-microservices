using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.UpdateFlashSale;

public class UpdateFlashSaleCommandHandler : ICommandHandler<UpdateFlashSaleCommand, FlashSaleDto>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<UpdateFlashSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFlashSaleCommandHandler(
        IFlashSaleRepository repository,
        IMapper mapper,
        IAppLogger<UpdateFlashSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FlashSaleDto>> Handle(UpdateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating flash sale {FlashSaleId}", request.Id);

        try
        {
            var flashSale = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (flashSale == null)
            {
                return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.NotFound", $"Flash sale with ID '{request.Id}' was not found"));
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
                flashSale.Title = request.Title;

            if (request.Description != null)
                flashSale.Description = request.Description;

            if (request.BannerUrl != null)
                flashSale.BannerUrl = request.BannerUrl;

            if (request.StartTime.HasValue)
                flashSale.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                flashSale.EndTime = request.EndTime.Value;

            if (request.DiscountPercentage.HasValue)
                flashSale.DiscountPercentage = request.DiscountPercentage.Value;

            if (request.IsActive.HasValue)
                flashSale.IsActive = request.IsActive.Value;

            if (request.DisplayOrder.HasValue)
                flashSale.DisplayOrder = request.DisplayOrder.Value;

            if (flashSale.EndTime <= flashSale.StartTime)
            {
                return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.InvalidTimeRange", "End time must be after start time"));
            }

            await _repository.UpdateAsync(flashSale, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated flash sale {FlashSaleId}", request.Id);

            var dto = _mapper.Map<FlashSaleDto>(flashSale);
            return Result<FlashSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update flash sale {FlashSaleId}: {Error}", request.Id, ex.Message);
            return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.UpdateFailed", $"Failed to update flash sale: {ex.Message}"));
        }
    }
}
