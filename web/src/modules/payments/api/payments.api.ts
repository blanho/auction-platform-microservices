import { http } from '@/services/http'
import type { CheckoutSession } from '../types'

export const paymentsApi = {
  async createOrderCheckoutSession(orderId: string): Promise<CheckoutSession> {
    const response = await http.post<CheckoutSession>(
      `/payments/orders/${orderId}/checkout-session`
    )
    return response.data
  },
}
