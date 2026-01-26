export interface PaymentIntent {
  paymentIntentId: string
  clientSecret: string
  status: string
}

export interface PaymentIntentDetails {
  paymentIntentId: string
  status: string
  amount?: number
  currency?: string
}

export interface CreatePaymentIntentRequest {
  amountInCents: number
  currency?: string
  customerId?: string
  metadata?: Record<string, string>
}

export interface CheckoutSession {
  sessionId: string
  url: string
}

export interface CreateCheckoutRequest {
  customerId?: string
  customerEmail?: string
  amountInCents: number
  currency?: string
  productName: string
  productDescription: string
  productImageUrl?: string
  successUrl: string
  cancelUrl: string
  metadata?: Record<string, string>
}

export interface StripeCustomer {
  customerId: string
  email: string
  name: string
}

export interface CreateCustomerRequest {
  email: string
  name: string
}

export interface Refund {
  refundId: string
  status: string
  amount?: number
}

export interface CreateRefundRequest {
  paymentIntentId: string
  amountInCents?: number
}

export interface PaymentMethod {
  id: string
  type: 'card' | 'bank_account'
  last4: string
  brand?: string
  expiryMonth?: number
  expiryYear?: number
  isDefault: boolean
}
