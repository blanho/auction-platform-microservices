using Common.Messaging.Events;
using MassTransit;

namespace IdentityService.Services;

public class EventBasedEmailService : IEmailService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EventBasedEmailService> _logger;

    public EventBasedEmailService(
        IPublishEndpoint publishEndpoint,
        IEmailTemplateService templateService,
        ILogger<EventBasedEmailService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var emailEvent = new EmailRequestedEvent
        {
            RecipientEmail = to,
            Subject = subject,
            HtmlBody = htmlBody,
            Source = "IdentityService"
        };

        await _publishEndpoint.Publish(emailEvent);
        _logger.LogInformation("Published EmailRequestedEvent for {Email}", to);
    }

    public async Task SendEmailConfirmationAsync(string email, string username, string confirmationLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["username"] = username,
            ["confirmationLink"] = confirmationLink
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("email-confirmation", variables);

        var emailEvent = new EmailRequestedEvent
        {
            RecipientEmail = email,
            RecipientName = username,
            TemplateName = "email-confirmation",
            Subject = subject,
            HtmlBody = htmlBody,
            Variables = variables,
            Source = "IdentityService"
        };

        await _publishEndpoint.Publish(emailEvent);
        _logger.LogInformation("Published email confirmation event for {Email}", email);
    }

    public async Task SendPasswordResetAsync(string email, string username, string resetLink)
    {
        var variables = new Dictionary<string, string>
        {
            ["username"] = username,
            ["resetLink"] = resetLink
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("password-reset", variables);

        var emailEvent = new EmailRequestedEvent
        {
            RecipientEmail = email,
            RecipientName = username,
            TemplateName = "password-reset",
            Subject = subject,
            HtmlBody = htmlBody,
            Variables = variables,
            Source = "IdentityService"
        };

        await _publishEndpoint.Publish(emailEvent);
        _logger.LogInformation("Published password reset event for {Email}", email);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var variables = new Dictionary<string, string>
        {
            ["username"] = username
        };

        var (subject, htmlBody) = await _templateService.RenderTemplateAsync("welcome", variables);

        var emailEvent = new EmailRequestedEvent
        {
            RecipientEmail = email,
            RecipientName = username,
            TemplateName = "welcome",
            Subject = subject,
            HtmlBody = htmlBody,
            Variables = variables,
            Source = "IdentityService"
        };

        await _publishEndpoint.Publish(emailEvent);
        _logger.LogInformation("Published welcome email event for {Email}", email);
    }
}
