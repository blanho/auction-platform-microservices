using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Filtering;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.NotificationRecords.GetNotificationRecordsByUser;

public class GetNotificationRecordsByUserQueryHandler : IQueryHandler<GetNotificationRecordsByUserQuery, PaginatedResult<NotificationRecordDto>>
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

    public async Task<Result<PaginatedResult<NotificationRecordDto>>> Handle(GetNotificationRecordsByUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification records for user: {UserId}, Page: {Page}, PageSize: {PageSize}", 
            request.UserId, request.Page, request.PageSize);

        var queryParams = new NotificationRecordQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new NotificationRecordFilterCriteria
            {
                UserId = request.UserId,
                Channel = request.Channel,
                Status = request.Status,
                TemplateKey = request.TemplateKey,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _recordService.GetByUserIdAsync(queryParams, cancellationToken);

        return Result.Success(result);
    }
}
