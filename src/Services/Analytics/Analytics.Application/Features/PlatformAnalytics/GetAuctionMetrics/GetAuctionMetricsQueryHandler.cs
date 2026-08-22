using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetAuctionMetrics;

public class GetAuctionMetricsQueryHandler : IRequestHandler<GetAuctionMetricsQuery, AuctionMetrics>
{
    private readonly IFactAuctionRepository _auctionRepository;
    public GetAuctionMetricsQueryHandler(IFactAuctionRepository auctionRepository) => _auctionRepository = auctionRepository;

    public async Task<AuctionMetrics> Handle(GetAuctionMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _auctionRepository.GetAuctionMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}
