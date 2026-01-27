using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordsByUser;

public class GetNotificationRecordsByUserQueryHandler : IQueryHandler<GetNotificationRecordsByUserQuery, List<NotificationRecordDto>>
{
    private readonly INotificationRecordService _recordService;
    private readonly ILogger<GetNotificationRecordsByUserQueryHandler> _logger;

    public GetNotificationRecordsByUserQueryHandler(
        INotificationRecordService recordService,
        ILogger<GetNotificationRecordsByUserQueryHandler> logger)
    {
        _recordService = recordService;
        _logger = logger;
    }

    public async Task<Result<List<NotificationRecordDto>>> Handle(GetNotificationRecordsByUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification records for user: {UserId}, Limit: {Limit}", request.UserId, request.Limit);

        var records = await _recordService.GetByUserIdAsync(request.UserId, request.Limit, cancellationToken);

        return Result.Success(records);
    }
}
