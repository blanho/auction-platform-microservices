using AutoMapper;

namespace Notification.Application.Extensions.Mappings;

public static class NotificationMappingExtensions
{
    public static NotificationDto ToDto(this NotificationEntity notification, IMapper mapper)
    {
        return mapper.Map<NotificationDto>(notification);
    }

    public static List<NotificationDto> ToDtoList(this IEnumerable<NotificationEntity> notifications, IMapper mapper)
    {
        return notifications.Select(n => n.ToDto(mapper)).ToList();
    }
}
