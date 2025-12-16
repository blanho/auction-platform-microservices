using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.AddFlashSaleItem;

public class AddFlashSaleItemCommandHandler : ICommandHandler<AddFlashSaleItemCommand, FlashSaleItemDto>
{
    private readonly IFlashSaleRepository _flashSaleRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<AddFlashSaleItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AddFlashSaleItemCommandHandler(
        IFlashSaleRepository flashSaleRepository,
        IAuctionRepository auctionRepository,
        IMapper mapper,
        IAppLogger<AddFlashSaleItemCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _flashSaleRepository = flashSaleRepository;
        _auctionRepository = auctionRepository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FlashSaleItemDto>> Handle(AddFlashSaleItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding item {AuctionId} to flash sale {FlashSaleId}", request.AuctionId, request.FlashSaleId);

        try
        {
            var flashSale = await _flashSaleRepository.GetByIdWithItemsAsync(request.FlashSaleId, cancellationToken);
            if (flashSale == null)
            {
                return Result.Failure<FlashSaleItemDto>(Error.Create("FlashSale.NotFound", $"Flash sale with ID '{request.FlashSaleId}' was not found"));
            }

            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null)
            {
                return Result.Failure<FlashSaleItemDto>(Error.Create("Auction.NotFound", $"Auction with ID '{request.AuctionId}' was not found"));
            }

            var existingItem = flashSale.Items.FirstOrDefault(i => i.AuctionId == request.AuctionId);
            if (existingItem != null)
            {
                return Result.Failure<FlashSaleItemDto>(Error.Create("FlashSale.ItemExists", "This auction is already in the flash sale"));
            }

            var item = new FlashSaleItem
            {
                FlashSaleId = request.FlashSaleId,
                AuctionId = request.AuctionId,
                SpecialPrice = request.SpecialPrice,
                DiscountPercentage = request.DiscountPercentage,
                DisplayOrder = request.DisplayOrder
            };

            var createdItem = await _flashSaleRepository.AddItemAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added item {AuctionId} to flash sale {FlashSaleId}", request.AuctionId, request.FlashSaleId);

            var dto = _mapper.Map<FlashSaleItemDto>(createdItem);
            return Result<FlashSaleItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to add item to flash sale: {Error}", ex.Message);
            return Result.Failure<FlashSaleItemDto>(Error.Create("FlashSale.AddItemFailed", $"Failed to add item to flash sale: {ex.Message}"));
        }
    }
}
