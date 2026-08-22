using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetCategoryPerformance;

public class GetCategoryPerformanceQueryHandler : IRequestHandler<GetCategoryPerformanceQuery, List<CategoryBreakdown>>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetCategoryPerformanceQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<List<CategoryBreakdown>> Handle(GetCategoryPerformanceQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetCategoryPerformanceAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
