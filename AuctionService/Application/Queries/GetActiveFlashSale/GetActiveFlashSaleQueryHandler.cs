using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.GetActiveFlashSale;

public class GetActiveFlashSaleQueryHandler : IQueryHandler<GetActiveFlashSaleQuery, ActiveFlashSaleDto?>
{
    private readonly IFlashSaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetActiveFlashSaleQueryHandler> _logger;

    public GetActiveFlashSaleQueryHandler(
        IFlashSaleRepository repository,
        IMapper mapper,
        IAppLogger<GetActiveFlashSaleQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ActiveFlashSaleDto?>> Handle(GetActiveFlashSaleQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching active flash sale");

        try
        {
            var flashSale = await _repository.GetCurrentFlashSaleAsync(cancellationToken);
            if (flashSale == null)
            {
                _logger.LogInformation("No active flash sale found");
                return Result<ActiveFlashSaleDto?>.Success((ActiveFlashSaleDto?)null);
            }

            var now = DateTimeOffset.UtcNow;
            var remainingSeconds = (long)(flashSale.EndTime - now).TotalSeconds;
            if (remainingSeconds < 0) remainingSeconds = 0;

            var dto = new ActiveFlashSaleDto
            {
                Id = flashSale.Id,
                Title = flashSale.Title,
                Description = flashSale.Description,
                BannerUrl = flashSale.BannerUrl,
                EndTime = flashSale.EndTime,
                DiscountPercentage = flashSale.DiscountPercentage,
                RemainingSeconds = remainingSeconds,
                Auctions = flashSale.Items
                    .Where(i => i.Auction != null)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new FlashSaleAuctionDto
                    {
                        Id = i.Auction!.Id,
                        Title = i.Auction.Item?.Title ?? string.Empty,
                        ImageUrl = i.Auction.Item?.Files?.FirstOrDefault(f => f.IsPrimary)?.Url 
                                   ?? i.Auction.Item?.Files?.FirstOrDefault()?.Url,
                        OriginalPrice = i.Auction.ReservePrice,
                        SalePrice = i.SpecialPrice ?? (i.Auction.ReservePrice * (100 - (i.DiscountPercentage ?? flashSale.DiscountPercentage)) / 100),
                        DiscountPercentage = i.DiscountPercentage ?? flashSale.DiscountPercentage,
                        SoldCount = 0,
                        TotalCount = 1
                    })
                    .ToList()
            };

            _logger.LogInformation("Successfully fetched active flash sale {FlashSaleId}", flashSale.Id);

            return Result.Success<ActiveFlashSaleDto?>(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active flash sale");
            return Result.Failure<ActiveFlashSaleDto?>(Error.Create("FlashSale.FetchError", $"Error fetching active flash sale: {ex.Message}"));
        }
    }
}
