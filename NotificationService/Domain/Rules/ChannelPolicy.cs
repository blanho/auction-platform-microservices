using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Rules;

public static class ChannelPolicy
{
    private static readonly Dictionary<NotificationType, ChannelType> DefaultChannels = new()
    {
        { NotificationType.AuctionCreated, ChannelType.InApp },
        { NotificationType.AuctionUpdated, ChannelType.InApp },
        { NotificationType.AuctionDeleted, ChannelType.InApp },
        { NotificationType.AuctionFinished, ChannelType.InApp | ChannelType.Email },
        { NotificationType.AuctionStarted, ChannelType.InApp },
        { NotificationType.AuctionEndingSoon, ChannelType.InApp | ChannelType.Push },
        { NotificationType.BidPlaced, ChannelType.InApp },
        { NotificationType.BidAccepted, ChannelType.InApp },
        { NotificationType.BidRejected, ChannelType.InApp },
        { NotificationType.OutBid, ChannelType.InApp | ChannelType.Email | ChannelType.Push },
        { NotificationType.AuctionWon, ChannelType.InApp | ChannelType.Email },
        { NotificationType.BuyNowExecuted, ChannelType.InApp | ChannelType.Email },
        { NotificationType.OrderCreated, ChannelType.InApp | ChannelType.Email },
        { NotificationType.OrderShipped, ChannelType.InApp | ChannelType.Email },
        { NotificationType.OrderDelivered, ChannelType.InApp },
        { NotificationType.PaymentCompleted, ChannelType.InApp | ChannelType.Email },
        { NotificationType.ReviewReceived, ChannelType.InApp },
        { NotificationType.WelcomeEmail, ChannelType.Email },
        { NotificationType.PasswordReset, ChannelType.Email },
        { NotificationType.AccountVerification, ChannelType.Email },
        { NotificationType.System, ChannelType.InApp }
    };

    public static ChannelType GetDefaultChannels(NotificationType type)
    {
        return DefaultChannels.TryGetValue(type, out var channels) 
            ? channels 
            : ChannelType.InApp;
    }

    public static IEnumerable<ChannelType> GetIndividualChannels(ChannelType channels)
    {
        if (channels.HasFlag(ChannelType.InApp))
            yield return ChannelType.InApp;
        if (channels.HasFlag(ChannelType.Email))
            yield return ChannelType.Email;
        if (channels.HasFlag(ChannelType.Sms))
            yield return ChannelType.Sms;
        if (channels.HasFlag(ChannelType.Push))
            yield return ChannelType.Push;
    }

    public static ChannelType FilterByRecipientCapability(ChannelType requestedChannels, Recipient recipient)
    {
        var availableChannels = ChannelType.InApp;

        if (requestedChannels.HasFlag(ChannelType.Email) && recipient.CanReceiveEmail)
            availableChannels |= ChannelType.Email;

        if (requestedChannels.HasFlag(ChannelType.Sms) && recipient.CanReceiveSms)
            availableChannels |= ChannelType.Sms;

        if (requestedChannels.HasFlag(ChannelType.Push) && recipient.CanReceivePush)
            availableChannels |= ChannelType.Push;

        return availableChannels;
    }

    public static bool IsHighPriority(NotificationType type)
    {
        return type switch
        {
            NotificationType.AuctionWon => true,
            NotificationType.OutBid => true,
            NotificationType.AuctionEndingSoon => true,
            NotificationType.PasswordReset => true,
            NotificationType.PaymentCompleted => true,
            _ => false
        };
    }
}
