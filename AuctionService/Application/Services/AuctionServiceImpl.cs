using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Exceptions;
using Common.Core.Interfaces;
using Common.Repository.Interfaces;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;

namespace AuctionService.Application.Services;

public class AuctionServiceImpl : IAuctionService
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<AuctionServiceImpl> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public AuctionServiceImpl(
        IAuctionRepository repository, 
        IMapper mapper,
        IAppLogger<AuctionServiceImpl> logger,
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

    public async Task<List<AuctionDto>> GetAllAuctionsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all auctions at {Timestamp}", _dateTime.UtcNow);

        
        var auctions = await _repository.GetAllAsync(cancellationToken);
        var result = auctions.Select(a => _mapper.Map<AuctionDto>(a)).ToList();
        
        _logger.LogInformation("Retrieved {Count} auctions (check cache decorator logs for cache hit/miss)", result.Count);
        return result;
    }

    public async Task<AuctionDto> GetAuctionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auction {AuctionId}", id);

        
        var auction = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", id);
            throw new NotFoundException($"Auction with ID {id} was not found");
        }
        
        return _mapper.Map<AuctionDto>(auction);
    }

    public async Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto dto, string seller, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", seller, _dateTime.UtcNow);

        var auction = _mapper.Map<Domain.Entities.Auction>(dto);
        auction.Seller = seller;
        var createdAuction = await _repository.CreateAsync(auction, cancellationToken);
        
        var auctionCreatedEvent = _mapper.Map<AuctionCreatedEvent>(createdAuction);
        await _eventPublisher.PublishAsync(auctionCreatedEvent, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created auction {AuctionId} for seller {Seller} and queued event in outbox", createdAuction.Id, seller);
        return _mapper.Map<AuctionDto>(createdAuction);
    }

    public async Task<bool> UpdateAuctionAsync(Guid id, UpdateAuctionDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auction {AuctionId} at {Timestamp}", id, _dateTime.UtcNow);

        var auction = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for update", id);
            throw new NotFoundException($"Auction with ID {id} was not found");
        }

        auction.Item.Make = dto.Make ?? auction.Item.Make;
        auction.Item.Model = dto.Model ?? auction.Item.Model;
        auction.Item.Color = dto.Color ?? auction.Item.Color;
        auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = dto.Year ?? auction.Item.Year;

        await _repository.UpdateAsync(auction, cancellationToken);
        
        await _eventPublisher.PublishAsync(new AuctionUpdatedEvent
        {
            Id = id,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Color = dto.Color,
            Mileage = dto.Mileage
        }, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Updated auction {AuctionId} and queued event in outbox", id);
        return true;
    }

    public async Task<bool> DeleteAuctionAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting auction {AuctionId} at {Timestamp}", id, _dateTime.UtcNow);

        var exists = await _repository.ExistsAsync(id, cancellationToken);
        
        if (!exists)
        {
            _logger.LogWarning("Auction {AuctionId} not found for deletion", id);
            throw new NotFoundException($"Auction with ID {id} was not found");
        }

        await _repository.DeleteAsync(id, cancellationToken);

        await _eventPublisher.PublishAsync(new AuctionDeletedEvent
        {
            Id = id
        }, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Deleted auction {AuctionId} and queued event in outbox", id);
        return true;
    }
}
