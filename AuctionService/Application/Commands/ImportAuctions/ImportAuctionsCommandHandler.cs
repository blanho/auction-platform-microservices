using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Audit.Abstractions;
using Common.Audit.Enums;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandHandler : ICommandHandler<ImportAuctionsCommand, ImportAuctionsResultDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<ImportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public ImportAuctionsCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<ImportAuctionsCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
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

                var auction = CreateAuctionEntity(dto, request.Seller);
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

            var auditEntities = createdAuctions
                .Select(auction => (auction.Id, auction))
                .ToList();
            
            await _auditPublisher.PublishBatchAsync(
                auditEntities,
                AuditAction.Created,
                cancellationToken: cancellationToken);

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
        
        if (string.IsNullOrWhiteSpace(dto.Make))
            return $"Row {rowNumber}: Make is required";
        
        if (string.IsNullOrWhiteSpace(dto.Model))
            return $"Row {rowNumber}: Model is required";
        
        if (dto.Year < 1900 || dto.Year > _dateTime.UtcNow.Year + 2)
            return $"Row {rowNumber}: Year must be between 1900 and {_dateTime.UtcNow.Year + 2}";
        
        if (dto.ReservePrice < 0)
            return $"Row {rowNumber}: Reserve price cannot be negative";
        
        if (dto.AuctionEnd <= _dateTime.UtcNow)
            return $"Row {rowNumber}: Auction end date must be in the future";

        return null;
    }

    private Auction CreateAuctionEntity(ImportAuctionDto dto, string seller)
    {
        return new Auction
        {
            Id = Guid.NewGuid(),
            ReversePrice = dto.ReservePrice,
            Seller = seller,
            AuctionEnd = dto.AuctionEnd,
            Status = Status.Scheduled,
            Item = new Item
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Color = dto.Color ?? string.Empty,
                Mileage = dto.Mileage
            }
        };
    }
}
