using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetRealTimeStats;

public class GetRealTimeStatsQueryHandler : IRequestHandler<GetRealTimeStatsQuery, RealTimeStatsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;

    public GetRealTimeStatsQueryHandler(IFactAuctionRepository auctionRepository, IFactBidRepository bidRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
    }

    public async Task<RealTimeStatsDto> Handle(GetRealTimeStatsQuery request, CancellationToken cancellationToken)
    {
        var liveAuctionsTask = _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);
        var bidsLastHourTask = _bidRepository.GetBidsInLastHourAsync(cancellationToken);

        await Task.WhenAll(liveAuctionsTask, bidsLastHourTask);

        return new RealTimeStatsDto
        {
            ActiveAuctions = await liveAuctionsTask,
            BidsLastHour = await bidsLastHourTask
        };
    }
}
