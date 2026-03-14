using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecords;

public class GetNotificationRecordsQueryHandler : IQueryHandler<GetNotificationRecordsQuery, PaginatedResult<NotificationRecordDto>>
{
    private readonly INotificationRecordService _recordService;
    private readonly ILogger<GetNotificationRecordsQueryHandler> _logger;

    public GetNotificationRecordsQueryHandler(
        INotificationRecordService recordService,
        ILogger<GetNotificationRecordsQueryHandler> logger)
    {
        _recordService = recordService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<NotificationRecordDto>>> Handle(GetNotificationRecordsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification records - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        var queryParams = new NotificationRecordFilterDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Filter = new NotificationRecordFilter
            {
                UserId = request.UserId,
                Channel = request.Channel,
                Status = request.Status,
                TemplateKey = request.TemplateKey,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _recordService.GetPagedAsync(queryParams, cancellationToken);

        return Result.Success(result);
    }
}
