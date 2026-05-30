using Bidding.Application.DTOs;
using Bidding.Application.DTOs.Audit;
using Bidding.Application.Interfaces;
using Bidding.Domain.Constants;
using Bidding.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Features.Bids.PlaceBid;

public class PlaceBidCommandHandler : ICommandHandler<PlaceBidCommand, BidDto>
{
    private readonly IBidService _bidService;
    private readonly IMessageDeduplicationService _deduplicationService;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ILogger<PlaceBidCommandHandler> _logger;

    public PlaceBidCommandHandler(
        IBidService bidService,
        IMessageDeduplicationService deduplicationService,
        IAuditPublisher auditPublisher,
        ILogger<PlaceBidCommandHandler> logger)
    {
        _bidService = bidService;
        _deduplicationService = deduplicationService;
        _auditPublisher = auditPublisher;
        _logger = logger;
    }

    public async Task<Result<BidDto>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing bid for auction {AuctionId} by {Bidder}, Amount: {Amount}",
            request.AuctionId,
            request.BidderUsername,
            request.Amount);

        var deduplicationKey = BuildDeduplicationKey(request);
        if (await _deduplicationService.IsProcessedAsync(deduplicationKey, cancellationToken))
        {
            _logger.LogInformation(
                "Duplicate bid request ignored for auction {AuctionId}, bidder {BidderId}, idempotency key {IdempotencyKey}",
                request.AuctionId,
                request.BidderId,
                request.IdempotencyKey);
            return Result.Failure<BidDto>(BidErrors.DuplicateRequest);
        }

        var bid = await _bidService.PlaceBidAsync(
            new PlaceBidDto
            {
                AuctionId = request.AuctionId,
                Amount = request.Amount
            },
            request.BidderId,
            request.BidderUsername,
            isAutoBid: false,
            cancellationToken);

        var result = ToPlacementResult(bid);
        if (result.IsFailure)
            return result;

        await _auditPublisher.PublishAsync(
            bid.Id,
            BidAuditData.FromDto(bid),
            AuditAction.Created,
            cancellationToken: cancellationToken);

        await _deduplicationService.MarkAsProcessedAsync(
            deduplicationKey,
            TimeSpan.FromSeconds(BidDefaults.DeduplicationWindowSeconds),
            cancellationToken);

        return result;
    }

    private static string BuildDeduplicationKey(PlaceBidCommand request)
    {
        return string.Join(
            ':',
            BidDefaults.DeduplicationKeyPrefix.TrimEnd(':'),
            request.AuctionId,
            request.BidderId,
            request.IdempotencyKey);
    }

    private static Result<BidDto> ToPlacementResult(BidDto bid)
    {
        if (Enum.TryParse<BidStatus>(bid.Status, ignoreCase: true, out var status))
        {
            return status switch
            {
                BidStatus.Accepted or BidStatus.AcceptedBelowReserve => Result.Success(bid),
                BidStatus.TooLow => Result.Failure<BidDto>(
                    BidErrors.BidTooLow(bid.ErrorMessage ?? "Bid amount is below the required minimum.")),
                BidStatus.Rejected => Result.Failure<BidDto>(
                    BidErrors.Rejected(bid.ErrorMessage ?? "Bid was rejected.")),
                _ => Result.Failure<BidDto>(
                    BidErrors.Rejected(bid.ErrorMessage ?? $"Bid finished with unsupported status '{bid.Status}'."))
            };
        }

        return Result.Failure<BidDto>(
            BidErrors.Rejected(bid.ErrorMessage ?? $"Bid finished with unsupported status '{bid.Status}'."));
    }
}
