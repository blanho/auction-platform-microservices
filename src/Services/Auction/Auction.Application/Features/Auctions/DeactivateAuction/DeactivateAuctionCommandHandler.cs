using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Commands.DeactivateAuction;

public class DeactivateAuctionCommandHandler : ICommandHandler<DeactivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeactivateAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<DeactivateAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuctionDto>> Handle(DeactivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating auction {AuctionId}", request.AuctionId);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);
        
        if (auction == null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to deactivate auction {AuctionId} owned by {OwnerId}", 
                request.UserId, request.AuctionId, auction.SellerId);
            return Result.Failure<AuctionDto>(Error.Create("Auction.Forbidden", "You are not authorized to deactivate this auction"));
        }
        
        if (auction.Status != Status.Live && auction.Status != Status.Scheduled)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        var previousStatus = auction.Status;
        auction.ChangeStatus(Status.Inactive);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated auction {AuctionId} from {PreviousStatus} to Inactive", 
            request.AuctionId, previousStatus);

        return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
    }
}

