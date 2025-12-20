using System.Net;
using System.Net.Mail;

namespace IdentityService.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IConfiguration configuration,
        IEmailTemplateService templateService,
        ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("Smtp");
        var host = smtpSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(smtpSettings["Port"] ?? "587");
        var username = smtpSettings["Username"] ?? "";
        var password = smtpSettings["Password"] ?? "";
        var fromEmail = smtpSettings["FromEmail"] ?? username;
        var fromName = smtpSettings["FromName"] ?? "Auction Platform";
        var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
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
