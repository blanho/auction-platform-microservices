using Auctions.Application.DTOs;
using Auctions.Application.Errors;
using AutoMapper;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.ActivateAuction;

public class ActivateAuctionCommandHandler : ICommandHandler<ActivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateAuctionCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<ActivateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuctionDto>> Handle(ActivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating auction {AuctionId}", request.AuctionId);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);
        
        if (auction == null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.Status != Status.Inactive && auction.Status != Status.Scheduled)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        if (auction.AuctionEnd <= _dateTime.UtcNow)
        {
            return Result.Failure<AuctionDto>(Error.Create("Auction.EndDatePassed", 
                "Cannot activate auction. The auction end date has already passed."));
        }

        var previousStatus = auction.Status;
        auction.ChangeStatus(Status.Live);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated auction {AuctionId} from {PreviousStatus} to Live", 
            request.AuctionId, previousStatus);

        return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
    }
}

