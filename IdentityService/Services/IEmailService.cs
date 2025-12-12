namespace IdentityService.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendEmailConfirmationAsync(string email, string username, string confirmationLink);
    Task SendPasswordResetAsync(string email, string username, string resetLink);
    Task SendWelcomeEmailAsync(string email, string username);
}
