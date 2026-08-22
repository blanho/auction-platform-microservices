using Stripe.Checkout;

namespace Payment.Application.Interfaces;

public interface IStripeServiceFactory
{
    SessionService CreateSessionService();
}
