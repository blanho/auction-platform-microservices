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

    public SessionService CreateSessionService() => new();
}
