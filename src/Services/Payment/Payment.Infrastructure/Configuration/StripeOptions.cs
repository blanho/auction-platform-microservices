using System.ComponentModel.DataAnnotations;

namespace Payment.Infrastructure.Configuration;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    [Required(ErrorMessage = "Stripe SecretKey is required")]
    [MinLength(10, ErrorMessage = "Stripe SecretKey appears to be invalid")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stripe WebhookSecret is required")]
    [RegularExpression(@"^whsec_.*", ErrorMessage = "Stripe WebhookSecret must start with 'whsec_'")]
    public string WebhookSecret { get; set; } = string.Empty;

    public string? PublishableKey { get; set; }
}
