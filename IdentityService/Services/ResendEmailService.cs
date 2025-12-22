using System.Net.Http.Json;
using System.Text.Json;

namespace IdentityService.Services;

public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ResendEmailService(
        IConfiguration configuration,
        IEmailTemplateService templateService,
        ILogger<ResendEmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _templateService = templateService;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Resend");

        _apiKey = _configuration["Email:Resend:ApiKey"]
            ?? throw new InvalidOperationException("Email:Resend:ApiKey is not configured");
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var fromEmail = _configuration["Email:Resend:FromEmail"] ?? "onboarding@resend.dev";
        var fromName = _configuration["Email:Resend:FromName"] ?? "Auction Platform";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = JsonContent.Create(new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { to },
                    subject,
                    html = htmlBody
                })
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Failed to send email: {error}");
            }

            _logger.LogInformation("Email sent successfully via Resend to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Resend to {Email}", to);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string username, string confirmationLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["username"] = username,
            ["confirmationLink"] = confirmationLink
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("email-confirmation", variables);
        await SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendPasswordResetAsync(string email, string username, string resetLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["username"] = username,
            ["resetLink"] = resetLink
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("password-reset", variables);
        await SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var variables = new Dictionary<string, string>
        {
            ["username"] = username,
            ["exploreLink"] = $"{frontendUrl}/auctions"
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("welcome", variables);
        await SendEmailAsync(email, subject, htmlBody);
    }
}
