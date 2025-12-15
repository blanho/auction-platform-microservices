using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewsForAuction;

public class GetReviewsForAuctionQueryHandler : IQueryHandler<GetReviewsForAuctionQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewsForAuctionQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsForAuctionQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByAuctionIdAsync(request.AuctionId, cancellationToken);
        return Result.Success(_mapper.Map<List<ReviewDto>>(reviews));
    }
}
