using Auction.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
namespace Auctions.Application.Queries.GetReviewById;

public class GetReviewByIdQueryHandler : IQueryHandler<GetReviewByIdQuery, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewByIdQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var review = await _reviewRepository.GetByIdAsync(request.Id, cancellationToken);
            return Result.Success(_mapper.Map<ReviewDto>(review));
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<ReviewDto>(AuctionErrors.Review.NotFound);
        }
    }
}

