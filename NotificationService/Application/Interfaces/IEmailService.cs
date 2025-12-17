using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface IEmailService
{
    Task<SendEmailResultDto> SendEmailAsync(EmailDto email, CancellationToken cancellationToken = default);
    Task<SendEmailResultDto> SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> data, CancellationToken cancellationToken = default);
    Task SendAuctionWonEmailAsync(string to, string auctionTitle, decimal winningBid, Guid auctionId, CancellationToken cancellationToken = default);
    Task SendOutbidEmailAsync(string to, string auctionTitle, decimal newBid, Guid auctionId, CancellationToken cancellationToken = default);
    Task SendBuyNowConfirmationEmailAsync(string to, string auctionTitle, decimal price, Guid auctionId, CancellationToken cancellationToken = default);
    Task SendOrderShippedEmailAsync(string to, string itemTitle, string trackingNumber, string? carrier, CancellationToken cancellationToken = default);
    Task SendAuctionEndingSoonEmailAsync(string to, string auctionTitle, TimeSpan timeRemaining, Guid auctionId, CancellationToken cancellationToken = default);
}
