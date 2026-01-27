import { http } from '@/services/http'
import type {
  Order,
  CreateOrderRequest,
  UpdateOrderRequest,
  ProcessPaymentRequest,
  ShipOrderRequest,
  CancelOrderRequest,
  OrderFilters,
  OrderStats,
  OrderStatus,
} from '../types'

interface GetAllOrdersParams {
  page?: number
  pageSize?: number
  search?: string
  status?: OrderStatus
  fromDate?: string
  toDate?: string
}

export const ordersApi = {
  async getOrderById(id: string): Promise<Order> {
    const response = await http.get<Order>(`/orders/${id}`)
    return response.data
  },

  async getOrderByAuctionId(auctionId: string): Promise<Order> {
    const response = await http.get<Order>(`/orders/auction/${auctionId}`)
    return response.data
  },

  async getOrdersByBuyer(
    username: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ items: Order[]; totalCount: number }> {
    const response = await http.get<Order[]>(`/orders/buyer/${username}`, {
      params: { page, pageSize },
    })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    return { items: response.data, totalCount }
  },

  async getOrdersBySeller(
    username: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ items: Order[]; totalCount: number }> {
    const response = await http.get<Order[]>(`/orders/seller/${username}`, {
      params: { page, pageSize },
    })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    return { items: response.data, totalCount }
  },

  async getMyPurchases(filters: OrderFilters): Promise<{ items: Order[]; totalCount: number; totalPages: number }> {
    const response = await http.get<Order[]>('/orders/buyer/me', { params: filters })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    const pageSize = filters.pageSize || 20
    return { items: response.data, totalCount, totalPages: Math.ceil(totalCount / pageSize) }
  },

  async getMySales(filters: OrderFilters): Promise<{ items: Order[]; totalCount: number; totalPages: number }> {
    const response = await http.get<Order[]>('/orders/seller/me', { params: filters })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    const pageSize = filters.pageSize || 20
    return { items: response.data, totalCount, totalPages: Math.ceil(totalCount / pageSize) }
  },

  async createOrder(data: CreateOrderRequest): Promise<Order> {
    const response = await http.post<Order>('/orders', data)
    return response.data
  },

  async updateOrder(id: string, data: UpdateOrderRequest): Promise<Order> {
    const response = await http.put<Order>(`/orders/${id}`, data)
    return response.data
  },

  async processPayment(id: string, data?: ProcessPaymentRequest): Promise<Order> {
    const response = await http.post<Order>(`/orders/${id}/payment`, data || {})
    return response.data
  },

  async shipOrder(id: string, data: ShipOrderRequest): Promise<Order> {
    const response = await http.post<Order>(`/orders/${id}/ship`, data)
    return response.data
  },

  async cancelOrder(id: string, data?: CancelOrderRequest): Promise<Order> {
    const response = await http.post<Order>(`/orders/${id}/cancel`, data || {})
    return response.data
  },

  async markDelivered(id: string): Promise<Order> {
    const response = await http.post<Order>(`/orders/${id}/deliver`)
    return response.data
  },

  async getAllOrders(params: GetAllOrdersParams = {}): Promise<{ items: Order[]; totalCount: number; totalPages: number }> {
    const response = await http.get<Order[]>('/orders', { params })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    const pageSize = params.pageSize || 20
    return { items: response.data, totalCount, totalPages: Math.ceil(totalCount / pageSize) }
  },

  async getOrderStats(): Promise<OrderStats> {
    const response = await http.get<OrderStats>('/orders/stats')
    return response.data
  },
}
