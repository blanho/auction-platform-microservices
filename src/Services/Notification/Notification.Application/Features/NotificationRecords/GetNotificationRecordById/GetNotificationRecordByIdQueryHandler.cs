using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordById;

public class GetNotificationRecordByIdQueryHandler : IQueryHandler<GetNotificationRecordByIdQuery, NotificationRecordDto?>
{
    private readonly INotificationRecordService _recordService;
    private readonly ILogger<GetNotificationRecordByIdQueryHandler> _logger;

    public GetNotificationRecordByIdQueryHandler(
        INotificationRecordService recordService,
        ILogger<GetNotificationRecordByIdQueryHandler> logger)
    {
        _recordService = recordService;
        _logger = logger;
    }

    public async Task<Result<NotificationRecordDto?>> Handle(GetNotificationRecordByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification record by ID: {RecordId}", request.Id);

        var record = await _recordService.GetByIdAsync(request.Id, cancellationToken);

        return Result.Success(record);
    }
}
