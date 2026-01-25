using AutoMapper;

namespace Notification.Application.Extensions.Mappings;

public static class NotificationMappingExtensions
{
    public static NotificationDto ToDto(this Domain.Entities.Notification notification, IMapper mapper)
    {
        return mapper.Map<NotificationDto>(notification);
    }

    public static List<NotificationDto> ToDtoList(this IEnumerable<Domain.Entities.Notification> notifications, IMapper mapper)
    {
        return notifications.Select(n => n.ToDto(mapper)).ToList();
    }
}
