using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services;

public interface INotificationSenderFactory
{
    INotificationSender? GetSender(ChannelType channel);
}

public class NotificationSenderFactory : INotificationSenderFactory
{
    private readonly Dictionary<ChannelType, INotificationSender> _senders;

    public NotificationSenderFactory(IEnumerable<INotificationSender> senders)
    {
        _senders = senders.ToDictionary(s => s.Channel, s => s);
    }

    public INotificationSender? GetSender(ChannelType channel)
    {
        return _senders.GetValueOrDefault(channel);
    }
}
