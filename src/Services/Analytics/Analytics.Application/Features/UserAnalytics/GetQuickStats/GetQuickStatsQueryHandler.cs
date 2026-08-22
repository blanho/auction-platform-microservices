using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.UserAnalytics.GetQuickStats;

public class GetQuickStatsQueryHandler : IRequestHandler<GetQuickStatsQuery, QuickStatsDto>
{
    private readonly IFactAuctionRepository _auctionRepository;

    public GetQuickStatsQueryHandler(IFactAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<QuickStatsDto> Handle(GetQuickStatsQuery request, CancellationToken cancellationToken)
    {
        var liveAuctions = await _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);

        return new QuickStatsDto
        {
            LiveAuctions = liveAuctions,
            LiveAuctionsChange = null,
            ActiveUsers = 0,
            ActiveUsersChange = null,
            EndingSoon = 0,
            EndingSoonChange = null
        };
    }
}
