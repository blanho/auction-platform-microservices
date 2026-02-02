using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandHandler : ICommandHandler<ImportAuctionsCommand, ImportAuctionsResultDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ImportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public ImportAuctionsCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<ImportAuctionsCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportAuctionsResultDto>> Handle(ImportAuctionsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Importing {Count} auctions for seller {Seller}", 
            request.Auctions.Count, request.Seller);

        var result = new ImportAuctionsResultDto
        {
            TotalRows = request.Auctions.Count
        };

        var createdAuctions = new List<Auction>();

        for (int i = 0; i < request.Auctions.Count; i++)
        {
            var dto = request.Auctions[i];
            var rowNumber = i + 1;

            try
            {
                var validationError = ValidateImportDto(dto, rowNumber);
                if (validationError != null)
                {
                    result.Results.Add(new ImportAuctionResultDto
                    {
                        RowNumber = rowNumber,
                        Success = false,
                        Error = validationError
                    });
                    result.FailureCount++;
                    continue;
                }

                var auction = CreateAuctionEntity(dto, request.Seller, request.SellerId);
                var createdAuction = await _repository.CreateAsync(auction, cancellationToken);
                createdAuctions.Add(createdAuction);

                result.Results.Add(new ImportAuctionResultDto
                {
                    RowNumber = rowNumber,
                    Success = true,
                    AuctionId = createdAuction.Id
                });
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to import auction at row {RowNumber}: {Error}", rowNumber, ex.Message);
                result.Results.Add(new ImportAuctionResultDto
                {
                    RowNumber = rowNumber,
                    Success = false,
                    Error = ex.Message
                });
                result.FailureCount++;
            }
        }

        if (createdAuctions.Count > 0)
        {
            var auctionCreatedEvents = createdAuctions
                .Select(auction => _mapper.Map<AuctionCreatedEvent>(auction))
                .ToList();
            
            await _eventPublisher.PublishBatchAsync(auctionCreatedEvents, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Import completed: {SuccessCount} succeeded, {FailureCount} failed out of {TotalRows}", 
            result.SuccessCount, result.FailureCount, result.TotalRows);

        return Result<ImportAuctionsResultDto>.Success(result);
    }

    private string? ValidateImportDto(ImportAuctionDto dto, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return $"Row {rowNumber}: Title is required";
        
        if (dto.YearManufactured.HasValue && (dto.YearManufactured < 1900 || dto.YearManufactured > _dateTime.UtcNow.Year + 2))
            return $"Row {rowNumber}: Year manufactured must be between 1900 and {_dateTime.UtcNow.Year + 2}";
        
        if (dto.ReservePrice < 0)
            return $"Row {rowNumber}: Reserve price cannot be negative";
        
        if (dto.AuctionEnd <= _dateTime.UtcNow)
            return $"Row {rowNumber}: Auction end date must be in the future";

        return null;
    }

    private Auction CreateAuctionEntity(ImportAuctionDto dto, string seller, Guid sellerId)
    {
        var item = Item.Create(
            title: dto.Title,
            description: dto.Description ?? string.Empty,
            condition: dto.Condition,
            yearManufactured: dto.YearManufactured);

        if (dto.Attributes != null)
        {
            foreach (var attr in dto.Attributes)
            {
                item.SetAttribute(attr.Key, attr.Value);
            }
        }

        return Auction.CreateScheduled(
            sellerId: sellerId,
            sellerUsername: seller,
            item: item,
            reservePrice: dto.ReservePrice,
            auctionEnd: dto.AuctionEnd,
            currency: dto.Currency);
    }
}

