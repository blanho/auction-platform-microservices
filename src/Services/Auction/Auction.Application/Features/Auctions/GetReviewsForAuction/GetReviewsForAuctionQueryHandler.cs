using Auctions.Application.DTOs;
using AutoMapper;
namespace Auctions.Application.Queries.GetReviewsForAuction;

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
        var dtos = reviews.Select(r => _mapper.Map<ReviewDto>(r)).ToList();
        return Result.Success(dtos);
    }
}

