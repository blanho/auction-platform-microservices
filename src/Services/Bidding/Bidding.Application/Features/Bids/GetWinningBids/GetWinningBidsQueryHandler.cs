using Bidding.Application.Interfaces;
using Bidding.Domain.Constants;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.Bids.GetWinningBids;

public class GetWinningBidsQueryHandler : IQueryHandler<GetWinningBidsQuery, PaginatedResult<WinningBidDto>>
{
    private readonly IBidRepository _repository;
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<GetWinningBidsQueryHandler> _logger;

    public GetWinningBidsQueryHandler(
        IBidRepository repository,
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<GetWinningBidsQueryHandler> logger)
    {
        _repository = repository;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<WinningBidDto>>> Handle(GetWinningBidsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting winning bids for user {UserId}, page {Page}", request.UserId, request.Page);

        var queryParams = new WinningBidQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new WinningBidFilter
            {
                AuctionId = request.AuctionId,
                IsPaid = request.IsPaid,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _repository.GetWinningBidsForUserAsync(
            request.UserId,
            queryParams,
            cancellationToken);

        var enrichedBids = new List<WinningBidDto>();
        foreach (var bid in result.Items)
        {
            var snapshot = await _snapshotRepository.GetAsync(bid.AuctionId, cancellationToken);
            enrichedBids.Add(new WinningBidDto
            {
                BidId = bid.Id,
                AuctionId = bid.AuctionId,
                AuctionTitle = snapshot?.Title ?? BidDefaults.WinningBids.DefaultAuctionTitle,
                WinningAmount = bid.Amount,
                WonAt = bid.BidTime,
                PaymentStatus = BidDefaults.WinningBids.DefaultPaymentStatus,
                IsPaid = false
            });
        }

        return Result<PaginatedResult<WinningBidDto>>.Success(new PaginatedResult<WinningBidDto>(
            enrichedBids,
            result.TotalCount,
            result.Page,
            result.PageSize
        ));
    }
}
