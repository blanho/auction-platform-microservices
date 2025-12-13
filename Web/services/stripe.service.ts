import apiClient from "@/lib/api/axios";

export interface CreatePaymentIntentRequest {
  amountInCents: number;
  currency: string;
  customerEmail: string;
  customerName: string;
  auctionId: string;
  buyerId: string;
  orderId?: string;
}

export interface CreatePaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

export interface CheckoutSessionRequest {
  customerEmail: string;
  amountInCents: number;
  currency: string;
  productName: string;
  productDescription: string;
  productImageUrl?: string;
  successUrl: string;
  cancelUrl: string;
  auctionId: string;
  buyerId: string;
}

export interface CheckoutSessionResponse {
  sessionId: string;
  sessionUrl: string;
}

export interface PaymentIntentStatus {
  id: string;
  status: string;
  amount: number;
  currency: string;
}

const UTILITY_API_URL = process.env.NEXT_PUBLIC_UTILITY_API_URL || "http://localhost:5005";

export const stripeService = {
  createPaymentIntent: async (request: CreatePaymentIntentRequest): Promise<CreatePaymentIntentResponse> => {
    const response = await apiClient.post<CreatePaymentIntentResponse>(
      `${UTILITY_API_URL}/api/payments/create-payment-intent`,
      request
    );
    return response.data;
  },

  createCheckoutSession: async (request: CheckoutSessionRequest): Promise<CheckoutSessionResponse> => {
    const response = await apiClient.post<CheckoutSessionResponse>(
      `${UTILITY_API_URL}/api/payments/create-checkout-session`,
      request
    );
    return response.data;
  },

  getPaymentIntentStatus: async (paymentIntentId: string): Promise<PaymentIntentStatus> => {
    const response = await apiClient.get<PaymentIntentStatus>(
      `${UTILITY_API_URL}/api/payments/payment-intent/${paymentIntentId}`
    );
    return response.data;
  },
};
