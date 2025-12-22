using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class ResendEmailSenderOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "onboarding@resend.dev";
    public string FromName { get; set; } = "Auction Platform";
}

public class ResendEmailSender : INotificationSender
{
    private readonly ResendEmailSenderOptions _options;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly HttpClient _httpClient;

    public ChannelType Channel => ChannelType.Email;

    public ResendEmailSender(
        IOptions<ResendEmailSenderOptions> options,
        ILogger<ResendEmailSender> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Resend");
    }

    public async Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.RecipientEmail))
        {
            return SendResult.Failed(ChannelType.Email, "No email address provided", true);
        }

        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Resend API key not configured, skipping email to {Recipient}", request.RecipientEmail);
            return SendResult.Failed(ChannelType.Email, "Resend API key not configured");
        }

        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Headers = { { "Authorization", $"Bearer {_options.ApiKey}" } },
                Content = JsonContent.Create(new
                {
                    from = $"{_options.FromName} <{_options.FromAddress}>",
                    to = new[] { request.RecipientEmail },
                    subject = request.Subject ?? "Notification",
                    html = request.HtmlBody ?? request.Body ?? string.Empty,
                    text = request.Body
                })
            };

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Resend API error: {StatusCode} - {Error}", response.StatusCode, error);
                
                var isPermanent = response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                                  response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
                return SendResult.Failed(ChannelType.Email, error, isPermanent);
            }

            var result = await response.Content.ReadFromJsonAsync<ResendResponse>(cancellationToken: cancellationToken);
            
            _logger.LogInformation("Email sent via Resend to {Recipient}, ID: {MessageId}", 
                request.RecipientEmail, result?.Id);
            
            return SendResult.Succeeded(ChannelType.Email, result?.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Resend to {Recipient}", request.RecipientEmail);
            return SendResult.Failed(ChannelType.Email, ex.Message);
        }
    }

    private record ResendResponse(string Id);
}
