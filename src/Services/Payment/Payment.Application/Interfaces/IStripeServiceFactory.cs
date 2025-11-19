using Stripe;
using Stripe.Checkout;

namespace Payment.Application.Interfaces;

public interface IStripeServiceFactory
{
    PaymentIntentService CreatePaymentIntentService();
    SessionService CreateSessionService();
    CustomerService CreateCustomerService();
    RefundService CreateRefundService();
}
