using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordStats;

public class GetNotificationRecordStatsQueryHandler : IQueryHandler<GetNotificationRecordStatsQuery, NotificationRecordStatsDto>
{
    private readonly INotificationRecordService _recordService;
    private readonly ILogger<GetNotificationRecordStatsQueryHandler> _logger;

    public GetNotificationRecordStatsQueryHandler(
        INotificationRecordService recordService,
        ILogger<GetNotificationRecordStatsQueryHandler> logger)
    {
        _recordService = recordService;
        _logger = logger;
    }

    public async Task<Result<NotificationRecordStatsDto>> Handle(GetNotificationRecordStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification record statistics");

        var stats = await _recordService.GetStatsAsync(cancellationToken);

        return Result.Success(stats);
    }
}
