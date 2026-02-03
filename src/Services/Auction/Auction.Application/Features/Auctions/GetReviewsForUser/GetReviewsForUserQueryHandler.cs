using Auctions.Application.DTOs;
using Auctions.Application.Filtering;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Queries.GetReviewsForUser;

public class GetReviewsForUserQueryHandler : IQueryHandler<GetReviewsForUserQuery, PaginatedResult<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewsForUserQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<ReviewDto>>> Handle(GetReviewsForUserQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new ReviewQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new ReviewFilter
            {
                ReviewedUsername = request.Username,
                MinRating = request.MinRating,
                MaxRating = request.MaxRating
            }
        };
        
        var result = await _reviewRepository.GetByReviewedUsernameAsync(queryParams, cancellationToken);
        
        return new PaginatedResult<ReviewDto>(
            result.Items.Select(r => _mapper.Map<ReviewDto>(r)).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}

