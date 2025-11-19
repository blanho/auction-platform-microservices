using Microsoft.Extensions.Options;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Payment.Infrastructure.Services;

public class StripeServiceFactory : IStripeServiceFactory
{
    public StripeServiceFactory(IOptions<StripeOptions> options)
    {

        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public PaymentIntentService CreatePaymentIntentService() => new();
    public SessionService CreateSessionService() => new();
    public CustomerService CreateCustomerService() => new();
    public RefundService CreateRefundService() => new();
}
