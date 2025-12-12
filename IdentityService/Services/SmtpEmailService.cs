using System.Net;
using System.Net.Mail;

namespace IdentityService.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
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
        var subject = "Confirm your email - Auction Platform";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .button:hover {{ background: #5a67d8; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Auction Platform!</h1>
        </div>
        <div class='content'>
            <p>Hi <strong>{username}</strong>,</p>
            <p>Thank you for registering! Please confirm your email address by clicking the button below:</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Confirm Email</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea;'>{confirmationLink}</p>
            <p>This link will expire in 24 hours.</p>
            <p>If you didn't create an account, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Auction Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendPasswordResetAsync(string email, string username, string resetLink)
    {
        var subject = "Reset your password - Auction Platform";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #f5576c; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .button:hover {{ background: #e5465c; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hi <strong>{username}</strong>,</p>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #f5576c;'>{resetLink}</p>
            <div class='warning'>
                <strong>⚠️ Security Notice:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email or contact support if you're concerned about your account security.
            </div>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Auction Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var subject = "Welcome to Auction Platform! 🎉";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .feature {{ display: flex; align-items: center; margin: 15px 0; padding: 15px; background: white; border-radius: 8px; }}
        .feature-icon {{ font-size: 24px; margin-right: 15px; }}
        .button {{ display: inline-block; background: #11998e; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome Aboard, {username}! 🚀</h1>
        </div>
        <div class='content'>
            <p>Your email has been confirmed and your account is now active!</p>
            <p>Here's what you can do on Auction Platform:</p>
            <div class='feature'>
                <span class='feature-icon'>🔨</span>
                <div><strong>Bid on Items</strong> - Find amazing deals and place your bids</div>
            </div>
            <div class='feature'>
                <span class='feature-icon'>📦</span>
                <div><strong>Sell Your Items</strong> - List your items and reach thousands of buyers</div>
            </div>
            <div class='feature'>
                <span class='feature-icon'>👀</span>
                <div><strong>Watch Auctions</strong> - Save items to your watchlist</div>
            </div>
            <div class='feature'>
                <span class='feature-icon'>💰</span>
                <div><strong>Manage Your Wallet</strong> - Easy deposits and withdrawals</div>
            </div>
            <p style='text-align: center;'>
                <a href='http://localhost:3000/auctions' class='button'>Start Exploring</a>
            </p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Auction Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, htmlBody);
    }
}
