using AutoMapper;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Notifications.GetUserNotifications;

public class GetUserNotificationsQueryHandler : IQueryHandler<GetUserNotificationsQuery, PaginatedResult<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserNotificationsQueryHandler> _logger;

    public GetUserNotificationsQueryHandler(
        INotificationRepository repository,
        IMapper mapper,
        ILogger<GetUserNotificationsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting notifications, page {Page}", request.Page);

        var result = await _repository.GetPaginatedAsync(
            request.Page,
            request.PageSize,
            request.UserId,
            null,
            null,
            cancellationToken);

        var dtos = result.Items.ToDtoList(_mapper);

        return Result.Success(new PaginatedResult<NotificationDto>(
            dtos,
            result.TotalCount,
            result.Page,
            result.PageSize));
    }
}
