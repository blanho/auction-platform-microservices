using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetBidMetrics;

public class GetBidMetricsQueryHandler : IRequestHandler<GetBidMetricsQuery, BidMetrics>
{
    private readonly IFactBidRepository _bidRepository;
    public GetBidMetricsQueryHandler(IFactBidRepository bidRepository) => _bidRepository = bidRepository;

    public async Task<BidMetrics> Handle(GetBidMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _bidRepository.GetBidMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}
