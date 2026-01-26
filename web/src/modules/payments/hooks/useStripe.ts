import { useQuery, useMutation } from '@tanstack/react-query'
import { stripeApi } from '../api'
import type {
  CreatePaymentIntentRequest,
  CreateCheckoutRequest,
  CreateCustomerRequest,
  CreateRefundRequest,
} from '../types'

export const stripeKeys = {
  all: ['stripe'] as const,
  paymentIntent: (id: string) => [...stripeKeys.all, 'payment-intent', id] as const,
}

export const useCreatePaymentIntent = () => {
  return useMutation({
    mutationFn: (data: CreatePaymentIntentRequest) => stripeApi.createPaymentIntent(data),
  })
}

export const usePaymentIntent = (paymentIntentId: string) => {
  return useQuery({
    queryKey: stripeKeys.paymentIntent(paymentIntentId),
    queryFn: () => stripeApi.getPaymentIntent(paymentIntentId),
    enabled: !!paymentIntentId,
  })
}

export const useCreateCheckoutSession = () => {
  return useMutation({
    mutationFn: (data: CreateCheckoutRequest) => stripeApi.createCheckoutSession(data),
  })
}

export const useCreateCustomer = () => {
  return useMutation({
    mutationFn: (data: CreateCustomerRequest) => stripeApi.createCustomer(data),
  })
}

export const useCreateRefund = () => {
  return useMutation({
    mutationFn: (data: CreateRefundRequest) => stripeApi.createRefund(data),
  })
}
