using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Domain.Constants;

namespace Analytics.Application.Features.PlatformAnalytics.GetTopPerformers;

public class GetTopPerformersQueryHandler : IRequestHandler<GetTopPerformersQuery, TopPerformersDto>
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactPaymentRepository _paymentRepository;

    public GetTopPerformersQueryHandler(IFactAuctionRepository auctionRepository, IFactPaymentRepository paymentRepository)
    {
        _auctionRepository = auctionRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<TopPerformersDto> Handle(GetTopPerformersQuery request, CancellationToken cancellationToken)
    {
        var startDate = GetPeriodStartDate(request.Period);
        var topAuctions = await _auctionRepository.GetTopAuctionsAsync(request.Limit, cancellationToken);
        var topSellers = await _paymentRepository.GetTopSellersAsync(request.Limit, startDate, cancellationToken);
        var topBuyers = await _paymentRepository.GetTopBuyersAsync(request.Limit, startDate, cancellationToken);

        return new TopPerformersDto
        {
            TopAuctions = topAuctions,
            TopSellers = topSellers,
            TopBuyers = topBuyers
        };
    }

    private static DateTimeOffset? GetPeriodStartDate(string period)
    {
        var now = DateTimeOffset.UtcNow;
        return period.ToLowerInvariant() switch
        {
            AnalyticsPeriods.Day => now.AddDays(-1),
            AnalyticsPeriods.Week => now.AddDays(-7),
            AnalyticsPeriods.Month => now.AddMonths(-1),
            AnalyticsPeriods.Year => now.AddYears(-1),
            AnalyticsPeriods.All => null,
            _ => now.AddMonths(-1)
        };
    }
}
