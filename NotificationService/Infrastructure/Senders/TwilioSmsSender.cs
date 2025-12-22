using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class TwilioSmsSenderOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class TwilioSmsSender : INotificationSender
{
    private readonly TwilioSmsSenderOptions _options;
    private readonly ILogger<TwilioSmsSender> _logger;
    private readonly HttpClient _httpClient;

    public ChannelType Channel => ChannelType.Sms;

    public TwilioSmsSender(
        IOptions<TwilioSmsSenderOptions> options,
        ILogger<TwilioSmsSender> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Twilio");
    }

    public async Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.RecipientPhone))
        {
            return SendResult.Failed(ChannelType.Sms, "No phone number provided", true);
        }

        if (string.IsNullOrEmpty(_options.AccountSid) || string.IsNullOrEmpty(_options.AuthToken))
        {
            _logger.LogWarning("Twilio credentials not configured, skipping SMS to {Phone}", request.RecipientPhone);
            return SendResult.Failed(ChannelType.Sms, "Twilio credentials not configured");
        }

        try
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_options.AccountSid}/Messages.json";
            
            var authValue = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{_options.AccountSid}:{_options.AuthToken}"));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = request.RecipientPhone,
                ["From"] = _options.FromNumber,
                ["Body"] = request.Body ?? string.Empty
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
                Headers = { { "Authorization", $"Basic {authValue}" } }
            };

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Twilio API error: {StatusCode} - {Error}", response.StatusCode, error);
                
                var isPermanent = response.StatusCode == System.Net.HttpStatusCode.BadRequest;
                return SendResult.Failed(ChannelType.Sms, error, isPermanent);
            }

            var result = await response.Content.ReadFromJsonAsync<TwilioResponse>(cancellationToken: cancellationToken);
            
            _logger.LogInformation("SMS sent via Twilio to {Phone}, SID: {Sid}", 
                request.RecipientPhone, result?.Sid);
            
            return SendResult.Succeeded(ChannelType.Sms, result?.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Twilio to {Phone}", request.RecipientPhone);
            return SendResult.Failed(ChannelType.Sms, ex.Message);
        }
    }

    private record TwilioResponse(string Sid, string Status);
}
