using Auctions.Application.DTOs;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Application.Queries.GetQuickStats;

public class GetQuickStatsQueryHandler : IQueryHandler<GetQuickStatsQuery, QuickStatsDto>
{
    private readonly IAuctionRepository _auctionRepository;

    public GetQuickStatsQueryHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<Result<QuickStatsDto>> Handle(GetQuickStatsQuery request, CancellationToken cancellationToken)
    {
        var liveAuctions = await _auctionRepository.CountLiveAuctionsAsync(cancellationToken);
        var endingSoonAuctions = await _auctionRepository.CountEndingSoonAsync(cancellationToken);

        var stats = new QuickStatsDto
        {
            LiveAuctions = liveAuctions,
            LiveAuctionsChange = null,
            ActiveUsers = 0,
            ActiveUsersChange = null,
            EndingSoon = endingSoonAuctions,
            EndingSoonChange = null
        };

        return Result<QuickStatsDto>.Success(stats);
    }
}

