import type { OrderStatus, PaymentStatus } from '../types'
import {
  ORDER_STATUS_COLORS,
  ORDER_STATUS_LABELS,
  PAYMENT_STATUS_COLORS,
  PAYMENT_STATUS_LABELS,
} from '../constants'

export function getOrderStatusColor(
  status: OrderStatus
): 'success' | 'warning' | 'info' | 'error' | 'default' {
  return ORDER_STATUS_COLORS[status] || 'default'
}

export function getOrderStatusLabel(status: OrderStatus): string {
  return ORDER_STATUS_LABELS[status] || status
}

export function getPaymentStatusColor(
  status: PaymentStatus
): 'success' | 'warning' | 'info' | 'error' | 'default' {
  return PAYMENT_STATUS_COLORS[status] || 'default'
}

export function getPaymentStatusLabel(status: PaymentStatus): string {
  return PAYMENT_STATUS_LABELS[status] || status
}

export function isOrderCompleted(status: OrderStatus): boolean {
  return status === 'completed' || status === 'delivered'
}

export function isOrderCancellable(status: OrderStatus): boolean {
  return status === 'pending' || status === 'payment_pending'
}

export function isOrderShippable(status: OrderStatus): boolean {
  return status === 'paid'
}

export function isOrderDeliverable(status: OrderStatus): boolean {
  return status === 'shipped'
}

export function canRequestRefund(status: OrderStatus, paymentStatus: PaymentStatus): boolean {
  return (
    (status === 'paid' || status === 'shipped' || status === 'delivered') &&
    paymentStatus === 'completed'
  )
}
