import type { OrderStatus, PaymentStatus } from '../types'

export const ORDER_STATUS = {
  PENDING: 'pending',
  PAYMENT_PENDING: 'payment_pending',
  PAID: 'paid',
  SHIPPED: 'shipped',
  DELIVERED: 'delivered',
  COMPLETED: 'completed',
  CANCELLED: 'cancelled',
  REFUNDED: 'refunded',
} as const

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  pending: 'Pending',
  payment_pending: 'Awaiting Payment',
  paid: 'Paid',
  shipped: 'Shipped',
  delivered: 'Delivered',
  completed: 'Completed',
  cancelled: 'Cancelled',
  refunded: 'Refunded',
}

export const ORDER_STATUS_COLORS: Record<OrderStatus, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  pending: 'default',
  payment_pending: 'warning',
  paid: 'info',
  shipped: 'info',
  delivered: 'success',
  completed: 'success',
  cancelled: 'error',
  refunded: 'warning',
}

export const PAYMENT_STATUS = {
  PENDING: 'pending',
  PROCESSING: 'processing',
  COMPLETED: 'completed',
  FAILED: 'failed',
  REFUNDED: 'refunded',
  CANCELLED: 'cancelled',
} as const

export const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  pending: 'Pending',
  processing: 'Processing',
  completed: 'Completed',
  failed: 'Failed',
  refunded: 'Refunded',
  cancelled: 'Cancelled',
}

export const PAYMENT_STATUS_COLORS: Record<PaymentStatus, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  pending: 'default',
  processing: 'info',
  completed: 'success',
  failed: 'error',
  refunded: 'warning',
  cancelled: 'error',
}
