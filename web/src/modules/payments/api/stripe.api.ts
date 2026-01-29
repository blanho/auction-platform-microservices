import { http } from '@/services/http'
import type {
  PaymentIntent,
  PaymentIntentDetails,
  CreatePaymentIntentRequest,
  CheckoutSession,
  CreateCheckoutRequest,
  StripeCustomer,
  CreateCustomerRequest,
  Refund,
  CreateRefundRequest,
} from '../types'

export const stripeApi = {
  async createPaymentIntent(data: CreatePaymentIntentRequest): Promise<PaymentIntent> {
    const response = await http.post<PaymentIntent>('/payments/payment-intent', data)
    return response.data
  },

  async getPaymentIntent(paymentIntentId: string): Promise<PaymentIntentDetails> {
    const response = await http.get<PaymentIntentDetails>(
      `/payments/payment-intent/${paymentIntentId}`
    )
    return response.data
  },

  async createCheckoutSession(data: CreateCheckoutRequest): Promise<CheckoutSession> {
    const response = await http.post<CheckoutSession>('/payments/checkout-session', data)
    return response.data
  },

  async createCustomer(data: CreateCustomerRequest): Promise<StripeCustomer> {
    const response = await http.post<StripeCustomer>('/payments/customer', data)
    return response.data
  },

  async createRefund(data: CreateRefundRequest): Promise<Refund> {
    const response = await http.post<Refund>('/payments/refund', data)
    return response.data
  },
}
