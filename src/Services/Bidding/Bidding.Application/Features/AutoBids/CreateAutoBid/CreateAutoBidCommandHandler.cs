using Bidding.Application.DTOs.Audit;
using Bidding.Application.Errors;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Bidding.Application.Features.AutoBids.CreateAutoBid;

public class CreateAutoBidCommandHandler : ICommandHandler<CreateAutoBidCommand, CreateAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IBidRepository _bidRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAutoBidCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IAuctionSnapshotRepository _snapshotRepository;

    public CreateAutoBidCommandHandler(
        IAutoBidRepository repository,
        IBidRepository bidRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAutoBidCommandHandler> logger,
        IAuditPublisher auditPublisher,
        IAuctionSnapshotRepository snapshotRepository)
    {
        _repository = repository;
        _bidRepository = bidRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _auditPublisher = auditPublisher;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<Result<CreateAutoBidResult>> Handle(CreateAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auto-bid for auction {AuctionId} by user {UserId} with max {MaxAmount}",
            request.AuctionId, request.UserId, request.MaxAmount);

        var existingAutoBid = await _repository.GetActiveAutoBidAsync(request.AuctionId, request.UserId, cancellationToken);
        if (existingAutoBid != null)
        {
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.AutoBid.AlreadyExists);
        }

        var snapshot = await _snapshotRepository.GetAsync(request.AuctionId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.Auction.NotFound);
        if (snapshot.Status != "Live")
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.Auction.NotActive);
        if (snapshot.EndTime <= DateTimeOffset.UtcNow)
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.Auction.AlreadyEnded);
        if (snapshot.SellerUsername == request.Username)
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.Auction.CannotBidOnOwnAuction);

        var currentHighBid = await _bidRepository.GetHighestBidForAuctionAsync(request.AuctionId, cancellationToken);
        var currentHighAmount = currentHighBid?.Amount ?? 0;

        if (request.MaxAmount <= currentHighAmount)
        {
            return Result.Failure<CreateAutoBidResult>(BiddingErrors.AutoBid.MaxAmountTooLow(currentHighAmount));
        }

        var autoBid = AutoBid.Create(
            request.AuctionId,
            request.UserId,
            request.Username,
            request.MaxAmount);

        await _repository.CreateAsync(autoBid, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            autoBid.Id,
            AutoBidAuditData.FromAutoBid(autoBid),
            AuditAction.Created,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} created successfully for auction {AuctionId}",
            autoBid.Id, request.AuctionId);

        var dto = autoBid.ToDto();
        return Result<CreateAutoBidResult>.Success(CreateAutoBidResult.Succeeded(dto));
    }
}
