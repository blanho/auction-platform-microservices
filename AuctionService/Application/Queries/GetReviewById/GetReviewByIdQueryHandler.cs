using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewById;

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
            return Result.Failure<ReviewDto>(Error.Create("Review.NotFound", "Review not found"));
        }
    }
}
