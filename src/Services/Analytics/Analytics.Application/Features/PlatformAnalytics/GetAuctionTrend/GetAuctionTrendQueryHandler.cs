using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetAuctionTrend;

public class GetAuctionTrendQueryHandler : IRequestHandler<GetAuctionTrendQuery, List<TrendDataPoint>>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetAuctionTrendQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<List<TrendDataPoint>> Handle(GetAuctionTrendQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetAuctionTrendAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
