import type { OrderStatus, PaymentStatus } from '../types'

export const ORDER_STATUS = {
  PENDING: 'pending',
  PAYMENT_PENDING: 'payment_pending',
  PAID: 'paid',
  PROCESSING: 'processing',
  SHIPPED: 'shipped',
  DELIVERED: 'delivered',
  COMPLETED: 'completed',
  CANCELLED: 'cancelled',
  DISPUTED: 'disputed',
  REFUNDED: 'refunded',
} as const

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  pending: 'Pending',
  payment_pending: 'Awaiting Payment',
  paid: 'Paid',
  processing: 'Processing',
  shipped: 'Shipped',
  delivered: 'Delivered',
  completed: 'Completed',
  cancelled: 'Cancelled',
  disputed: 'Disputed',
  refunded: 'Refunded',
}

export const ORDER_STATUS_COLORS: Record<
  OrderStatus,
  'success' | 'warning' | 'info' | 'error' | 'default'
> = {
  pending: 'default',
  payment_pending: 'warning',
  paid: 'info',
  processing: 'info',
  shipped: 'info',
  delivered: 'success',
  completed: 'success',
  cancelled: 'error',
  disputed: 'error',
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

export const PAYMENT_STATUS_COLORS: Record<
  PaymentStatus,
  'success' | 'warning' | 'info' | 'error' | 'default'
> = {
  pending: 'default',
  processing: 'info',
  completed: 'success',
  failed: 'error',
  refunded: 'warning',
  cancelled: 'error',
}
