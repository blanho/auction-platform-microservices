using Auctions.Application.DTOs;
using Auctions.Application.Filtering;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Queries.GetReviewsByUser;

public class GetReviewsByUserQueryHandler : IQueryHandler<GetReviewsByUserQuery, PaginatedResult<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewsByUserQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<ReviewDto>>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new ReviewQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new ReviewFilter
            {
                ReviewerUsername = request.Username,
                MinRating = request.MinRating,
                MaxRating = request.MaxRating
            }
        };
        
        var result = await _reviewRepository.GetByReviewerUsernameAsync(queryParams, cancellationToken);
        
        return new PaginatedResult<ReviewDto>(
            result.Items.Select(r => _mapper.Map<ReviewDto>(r)).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}

