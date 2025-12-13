#nullable enable
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;
        private readonly bool _isEnabled;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _isEnabled = _configuration.GetValue<bool>("Email:Enabled", false);
            _smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            _smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
            _smtpUser = _configuration["Email:SmtpUser"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@auction.com";
            _fromName = _configuration["Email:FromName"] ?? "Auction Platform";
            _enableSsl = _configuration.GetValue<bool>("Email:EnableSsl", true);
        }

        public async Task<SendEmailResultDto> SendEmailAsync(EmailDto email, CancellationToken cancellationToken = default)
        {
            if (!_isEnabled)
            {
                _logger.LogInformation("Email sending is disabled. Would have sent email to {To} with subject: {Subject}", 
                    email.To, email.Subject);
                return new SendEmailResultDto { Success = true, MessageId = "disabled" };
            }

            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort);
                client.EnableSsl = _enableSsl;

                if (!string.IsNullOrEmpty(_smtpUser))
                {
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                }

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = email.IsHtml
                };
                message.To.Add(email.To);

                await client.SendMailAsync(message, cancellationToken);

                _logger.LogInformation("Email sent successfully to {To}", email.To);

                return new SendEmailResultDto
                {
                    Success = true,
                    MessageId = Guid.NewGuid().ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", email.To);
                return new SendEmailResultDto
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<SendEmailResultDto> SendTemplatedEmailAsync(
            string to, 
            string templateName, 
            Dictionary<string, string> data, 
            CancellationToken cancellationToken = default)
        {
            var template = GetTemplate(templateName);
            var body = ReplaceTemplatePlaceholders(template.HtmlBody, data);
            var subject = ReplaceTemplatePlaceholders(template.Subject, data);

            return await SendEmailAsync(new EmailDto
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            }, cancellationToken);
        }

        public async Task SendAuctionWonEmailAsync(
            string to, 
            string auctionTitle, 
            int winningBid, 
            Guid auctionId, 
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { "AuctionTitle", auctionTitle },
                { "WinningBid", $"${winningBid:N0}" },
                { "AuctionId", auctionId.ToString() },
                { "AuctionUrl", $"/auctions/{auctionId}" }
            };

            await SendTemplatedEmailAsync(to, "AuctionWon", data, cancellationToken);
        }

        public async Task SendOutbidEmailAsync(
            string to, 
            string auctionTitle, 
            int newBid, 
            Guid auctionId, 
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { "AuctionTitle", auctionTitle },
                { "NewBid", $"${newBid:N0}" },
                { "AuctionId", auctionId.ToString() },
                { "AuctionUrl", $"/auctions/{auctionId}" }
            };

            await SendTemplatedEmailAsync(to, "Outbid", data, cancellationToken);
        }

        public async Task SendBuyNowConfirmationEmailAsync(
            string to, 
            string auctionTitle, 
            int price, 
            Guid auctionId, 
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { "AuctionTitle", auctionTitle },
                { "Price", $"${price:N0}" },
                { "AuctionId", auctionId.ToString() },
                { "AuctionUrl", $"/auctions/{auctionId}" }
            };

            await SendTemplatedEmailAsync(to, "BuyNowConfirmation", data, cancellationToken);
        }

        public async Task SendOrderShippedEmailAsync(
            string to, 
            string itemTitle, 
            string trackingNumber, 
            string? carrier, 
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { "ItemTitle", itemTitle },
                { "TrackingNumber", trackingNumber },
                { "Carrier", carrier ?? "Not specified" }
            };

            await SendTemplatedEmailAsync(to, "OrderShipped", data, cancellationToken);
        }

        public async Task SendAuctionEndingSoonEmailAsync(
            string to, 
            string auctionTitle, 
            TimeSpan timeRemaining, 
            Guid auctionId, 
            CancellationToken cancellationToken = default)
        {
            var timeString = timeRemaining.TotalHours >= 1 
                ? $"{timeRemaining.Hours} hour(s)" 
                : $"{timeRemaining.Minutes} minute(s)";

            var data = new Dictionary<string, string>
            {
                { "AuctionTitle", auctionTitle },
                { "TimeRemaining", timeString },
                { "AuctionId", auctionId.ToString() },
                { "AuctionUrl", $"/auctions/{auctionId}" }
            };

            await SendTemplatedEmailAsync(to, "AuctionEndingSoon", data, cancellationToken);
        }

        private EmailTemplateDto GetTemplate(string templateName)
        {
            return templateName switch
            {
                "AuctionWon" => new EmailTemplateDto
                {
                    Name = "AuctionWon",
                    Subject = "Congratulations! You won the auction: {{AuctionTitle}}",
                    HtmlBody = GetAuctionWonTemplate()
                },
                "Outbid" => new EmailTemplateDto
                {
                    Name = "Outbid",
                    Subject = "You've been outbid on: {{AuctionTitle}}",
                    HtmlBody = GetOutbidTemplate()
                },
                "BuyNowConfirmation" => new EmailTemplateDto
                {
                    Name = "BuyNowConfirmation",
                    Subject = "Purchase Confirmed: {{AuctionTitle}}",
                    HtmlBody = GetBuyNowConfirmationTemplate()
                },
                "OrderShipped" => new EmailTemplateDto
                {
                    Name = "OrderShipped",
                    Subject = "Your order has been shipped: {{ItemTitle}}",
                    HtmlBody = GetOrderShippedTemplate()
                },
                "AuctionEndingSoon" => new EmailTemplateDto
                {
                    Name = "AuctionEndingSoon",
                    Subject = "Auction ending soon: {{AuctionTitle}}",
                    HtmlBody = GetAuctionEndingSoonTemplate()
                },
                _ => new EmailTemplateDto
                {
                    Name = "Default",
                    Subject = "Notification from Auction Platform",
                    HtmlBody = "<p>{{Message}}</p>"
                }
            };
        }

        private static string ReplaceTemplatePlaceholders(string template, Dictionary<string, string> data)
        {
            foreach (var kvp in data)
            {
                template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }
            return template;
        }

        private static string GetBaseTemplate(string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #fff; }}
        .header {{ background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 30px; text-align: center; }}
        .header h1 {{ color: #fff; margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; background: #f59e0b; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: 600; margin: 20px 0; }}
        .button:hover {{ background: #d97706; }}
        .footer {{ background: #f8f8f8; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .highlight {{ background: #fef3c7; padding: 15px; border-radius: 6px; margin: 15px 0; }}
        .price {{ font-size: 28px; font-weight: bold; color: #f59e0b; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🏆 Auction Platform</h1>
        </div>
        <div class=""content"">
            {content}
        </div>
        <div class=""footer"">
            <p>© 2024 Auction Platform. All rights reserved.</p>
            <p>You received this email because you're registered on our platform.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetAuctionWonTemplate()
        {
            return GetBaseTemplate(@"
                <h2>🎉 Congratulations!</h2>
                <p>You've won the auction:</p>
                <div class=""highlight"">
                    <h3 style=""margin: 0 0 10px 0;"">{{AuctionTitle}}</h3>
                    <p class=""price"">{{WinningBid}}</p>
                </div>
                <p>Complete your purchase to claim your item. The seller will be notified and will ship your item once payment is confirmed.</p>
                <a href=""{{AuctionUrl}}"" class=""button"">View Auction & Pay</a>
                <p style=""color: #666; font-size: 14px;"">Please complete payment within 48 hours to avoid cancellation.</p>
            ");
        }

        private static string GetOutbidTemplate()
        {
            return GetBaseTemplate(@"
                <h2>⚠️ You've Been Outbid!</h2>
                <p>Someone placed a higher bid on:</p>
                <div class=""highlight"">
                    <h3 style=""margin: 0 0 10px 0;"">{{AuctionTitle}}</h3>
                    <p>Current highest bid: <span class=""price"">{{NewBid}}</span></p>
                </div>
                <p>Don't let this one slip away! Place a new bid now to stay in the running.</p>
                <a href=""{{AuctionUrl}}"" class=""button"">Place New Bid</a>
                <p style=""color: #666; font-size: 14px;"">Tip: Set up auto-bidding to automatically bid up to your maximum amount.</p>
            ");
        }

        private static string GetBuyNowConfirmationTemplate()
        {
            return GetBaseTemplate(@"
                <h2>✅ Purchase Confirmed!</h2>
                <p>Thank you for your purchase:</p>
                <div class=""highlight"">
                    <h3 style=""margin: 0 0 10px 0;"">{{AuctionTitle}}</h3>
                    <p class=""price"">{{Price}}</p>
                </div>
                <p>The seller has been notified and will ship your item soon. You'll receive a notification when your item ships.</p>
                <a href=""{{AuctionUrl}}"" class=""button"">View Order Details</a>
            ");
        }

        private static string GetOrderShippedTemplate()
        {
            return GetBaseTemplate(@"
                <h2>📦 Your Order Has Shipped!</h2>
                <p>Great news! Your item is on its way:</p>
                <div class=""highlight"">
                    <h3 style=""margin: 0 0 10px 0;"">{{ItemTitle}}</h3>
                    <p><strong>Carrier:</strong> {{Carrier}}</p>
                    <p><strong>Tracking Number:</strong> {{TrackingNumber}}</p>
                </div>
                <p>You can track your package using the tracking number above.</p>
            ");
        }

        private static string GetAuctionEndingSoonTemplate()
        {
            return GetBaseTemplate(@"
                <h2>⏰ Auction Ending Soon!</h2>
                <p>An auction you're watching is about to end:</p>
                <div class=""highlight"">
                    <h3 style=""margin: 0 0 10px 0;"">{{AuctionTitle}}</h3>
                    <p><strong>Time remaining:</strong> {{TimeRemaining}}</p>
                </div>
                <p>Don't miss your chance to win this item!</p>
                <a href=""{{AuctionUrl}}"" class=""button"">Bid Now</a>
            ");
        }
    }
}
