using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.CreateFlashSale;

public class CreateFlashSaleCommandHandler : ICommandHandler<CreateFlashSaleCommand, FlashSaleDto>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<CreateFlashSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFlashSaleCommandHandler(
        IFlashSaleRepository repository,
        IMapper mapper,
        IAppLogger<CreateFlashSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FlashSaleDto>> Handle(CreateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating flash sale {Title}", request.Title);

        try
        {
            if (request.EndTime <= request.StartTime)
            {
                return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.InvalidTimeRange", "End time must be after start time"));
            }

            var flashSale = new FlashSale
            {
                Title = request.Title,
                Description = request.Description,
                BannerUrl = request.BannerUrl,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                DiscountPercentage = request.DiscountPercentage,
                DisplayOrder = request.DisplayOrder,
                IsActive = true
            };

            var createdFlashSale = await _repository.AddAsync(flashSale, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created flash sale {FlashSaleId} with title {Title}", createdFlashSale.Id, request.Title);

            var dto = _mapper.Map<FlashSaleDto>(createdFlashSale);
            return Result<FlashSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create flash sale {Title}: {Error}", request.Title, ex.Message);
            return Result.Failure<FlashSaleDto>(Error.Create("FlashSale.CreateFailed", $"Failed to create flash sale: {ex.Message}"));
        }
    }
}
