import apiClient from "@/lib/api/axios";
import { API_ENDPOINTS } from "@/constants/api";

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

export interface RefundRequest {
  paymentIntentId: string;
  amountInCents?: number;
  reason?: string;
}

export interface RefundResponse {
  id: string;
  status: string;
  amount: number;
}

export const paymentService = {
  createPaymentIntent: async (request: CreatePaymentIntentRequest): Promise<CreatePaymentIntentResponse> => {
    const { data } = await apiClient.post<CreatePaymentIntentResponse>(
      API_ENDPOINTS.PAYMENTS.CREATE_PAYMENT_INTENT,
      request
    );
    return data;
  },

  createCheckoutSession: async (request: CheckoutSessionRequest): Promise<CheckoutSessionResponse> => {
    const { data } = await apiClient.post<CheckoutSessionResponse>(
      API_ENDPOINTS.PAYMENTS.CREATE_CHECKOUT_SESSION,
      request
    );
    return data;
  },

  getPaymentIntentStatus: async (paymentIntentId: string): Promise<PaymentIntentStatus> => {
    const { data } = await apiClient.get<PaymentIntentStatus>(
      API_ENDPOINTS.PAYMENTS.PAYMENT_INTENT_STATUS(paymentIntentId)
    );
    return data;
  },

  requestRefund: async (request: RefundRequest): Promise<RefundResponse> => {
    const { data } = await apiClient.post<RefundResponse>(
      API_ENDPOINTS.PAYMENTS.REFUND,
      request
    );
    return data;
  },
} as const;
