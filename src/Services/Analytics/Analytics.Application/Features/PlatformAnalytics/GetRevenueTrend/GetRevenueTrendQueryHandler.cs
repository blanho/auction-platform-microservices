using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetRevenueTrend;

public class GetRevenueTrendQueryHandler : IRequestHandler<GetRevenueTrendQuery, List<TrendDataPoint>>
{
    private readonly IFactPaymentRepository _paymentRepository;
    public GetRevenueTrendQueryHandler(IFactPaymentRepository paymentRepository) => _paymentRepository = paymentRepository;

    public async Task<List<TrendDataPoint>> Handle(GetRevenueTrendQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetRevenueTrendAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
