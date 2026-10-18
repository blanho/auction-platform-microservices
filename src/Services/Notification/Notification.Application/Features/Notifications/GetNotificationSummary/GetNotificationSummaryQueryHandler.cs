using AutoMapper;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Notifications.GetNotificationSummary;

public class GetNotificationSummaryQueryHandler : IQueryHandler<GetNotificationSummaryQuery, NotificationSummaryDto>
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetNotificationSummaryQueryHandler> _logger;

    public GetNotificationSummaryQueryHandler(
        INotificationRepository repository,
        IMapper mapper,
        ILogger<GetNotificationSummaryQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<NotificationSummaryDto>> Handle(GetNotificationSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification summary for user {UserId}", request.UserId);

        var notifications = await _repository.GetPaginatedAsync(PaginationDefaults.DefaultPage,
            NotificationDefaults.Pagination.RecentNotificationsCount, request.UserId,
            cancellationToken: cancellationToken);
        var unreadCount = await _repository.GetUnreadCountByUserIdAsync(request.UserId, cancellationToken);

        var summary = new NotificationSummaryDto
        {
            UnreadCount = unreadCount,
            TotalCount = notifications.TotalCount,
            RecentNotifications = notifications.Items.ToDtoList(_mapper)
        };

        return Result.Success(summary);
    }
}
