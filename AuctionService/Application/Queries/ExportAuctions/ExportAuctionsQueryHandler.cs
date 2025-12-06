using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Domain.Enums;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Queries.ExportAuctions;

public class ExportAuctionsQueryHandler : IQueryHandler<ExportAuctionsQuery, List<ExportAuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<ExportAuctionsQueryHandler> _logger;

    public ExportAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<ExportAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ExportAuctionDto>>> Handle(ExportAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting auctions with filters - Status: {Status}, Seller: {Seller}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.Status ?? "All", request.Seller ?? "All", request.StartDate?.ToString() ?? "None", request.EndDate?.ToString() ?? "None");

        try
        {
            Status? statusFilter = null;
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<Status>(request.Status, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }

            var auctions = await _repository.GetAuctionsForExportAsync(
                statusFilter,
                request.Seller,
                request.StartDate,
                request.EndDate,
                cancellationToken);

            var exportDtos = auctions.Select(a => new ExportAuctionDto
            {
                Id = a.Id,
                Title = a.Item.Title,
                Description = a.Item.Description,
                Make = a.Item.Make,
                Model = a.Item.Model,
                Year = a.Item.Year,
                Color = a.Item.Color,
                Mileage = a.Item.Mileage,
                ReservePrice = a.ReversePrice,
                Seller = a.Seller,
                Winner = a.Winner,
                SoldAmount = a.SoldAmount,
                CurrentHighBid = a.CurrentHighBid,
                CreatedAt = a.CreatedAt,
                AuctionEnd = a.AuctionEnd,
                Status = a.Status.ToString()
            }).ToList();

            _logger.LogInformation("Exported {Count} auctions", exportDtos.Count);

            return Result<List<ExportAuctionDto>>.Success(exportDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to export auctions: {Error}", ex.Message);
            var error = Error.Create("Auction.ExportFailed", ex.Message);
            return Result.Failure<List<ExportAuctionDto>>(error);
        }
    }
}
