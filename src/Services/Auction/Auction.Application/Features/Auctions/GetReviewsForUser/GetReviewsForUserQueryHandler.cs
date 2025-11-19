using Auctions.Application.DTOs;
using AutoMapper;
namespace Auctions.Application.Queries.GetReviewsForUser;

public class GetReviewsForUserQueryHandler : IQueryHandler<GetReviewsForUserQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewsForUserQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsForUserQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByReviewedUsernameAsync(request.Username, cancellationToken);
        return Result.Success(_mapper.Map<List<ReviewDto>>(reviews));
    }
}

