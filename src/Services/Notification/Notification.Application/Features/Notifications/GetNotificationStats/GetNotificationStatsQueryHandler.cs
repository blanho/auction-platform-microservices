using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Notifications.GetNotificationStats;

public class GetNotificationStatsQueryHandler : IQueryHandler<GetNotificationStatsQuery, NotificationStatsDto>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<GetNotificationStatsQueryHandler> _logger;

    public GetNotificationStatsQueryHandler(
        INotificationRepository repository,
        ILogger<GetNotificationStatsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<NotificationStatsDto>> Handle(GetNotificationStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting notification stats");

        var stats = await _repository.GetStatsAsync(cancellationToken);

        var dto = new NotificationStatsDto
        {
            TotalNotifications = stats.TotalCount,
            UnreadNotifications = stats.UnreadCount,
            TodayCount = stats.TodayCount,
            ByType = stats.ByType
        };

        return Result.Success(dto);
    }
}
