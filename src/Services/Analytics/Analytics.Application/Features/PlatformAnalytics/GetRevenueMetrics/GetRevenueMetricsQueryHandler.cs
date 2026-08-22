using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetRevenueMetrics;

public class GetRevenueMetricsQueryHandler : IRequestHandler<GetRevenueMetricsQuery, RevenueMetrics>
{
    private readonly IFactPaymentRepository _paymentRepository;
    public GetRevenueMetricsQueryHandler(IFactPaymentRepository paymentRepository) => _paymentRepository = paymentRepository;

    public async Task<RevenueMetrics> Handle(GetRevenueMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetRevenueMetricsAsync(request.Query.StartDate, request.Query.EndDate, cancellationToken);
    }
}
