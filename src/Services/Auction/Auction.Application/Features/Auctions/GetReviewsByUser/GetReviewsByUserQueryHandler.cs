using Auctions.Application.DTOs;
using AutoMapper;
namespace Auctions.Application.Queries.GetReviewsByUser;

public class GetReviewsByUserQueryHandler : IQueryHandler<GetReviewsByUserQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewsByUserQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByReviewerUsernameAsync(request.Username, cancellationToken);
        var dtos = reviews.Select(r => _mapper.Map<ReviewDto>(r)).ToList();
        return Result.Success(dtos);
    }
}

