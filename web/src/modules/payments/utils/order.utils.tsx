import {
  Pending,
  CheckCircle,
  LocalShipping,
  Cancel,
  Inventory,
  AttachMoney,
  Receipt,
} from '@mui/icons-material'
import type { OrderStatus, PaymentStatus } from '../types'
import {
  ORDER_STATUS_COLORS,
  ORDER_STATUS_LABELS,
  PAYMENT_STATUS_COLORS,
  PAYMENT_STATUS_LABELS,
} from '../constants'

export interface OrderStatusConfig {
  icon: React.ReactElement
  color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'
  label?: string
}

export function getOrderStatusConfig(status: OrderStatus): OrderStatusConfig {
  const config: Record<OrderStatus, OrderStatusConfig> = {
    pending: { icon: <Pending fontSize="small" />, color: 'warning', label: 'Pending' },
    payment_pending: {
      icon: <Pending fontSize="small" />,
      color: 'warning',
      label: 'Awaiting Payment',
    },
    paid: { icon: <CheckCircle fontSize="small" />, color: 'info', label: 'Paid' },
    processing: { icon: <Pending fontSize="small" />, color: 'info', label: 'Processing' },
    shipped: { icon: <LocalShipping fontSize="small" />, color: 'primary', label: 'Shipped' },
    delivered: { icon: <Inventory fontSize="small" />, color: 'success', label: 'Delivered' },
    completed: { icon: <CheckCircle fontSize="small" />, color: 'success', label: 'Completed' },
    cancelled: { icon: <Cancel fontSize="small" />, color: 'error', label: 'Cancelled' },
    disputed: { icon: <Cancel fontSize="small" />, color: 'error', label: 'Disputed' },
    refunded: { icon: <Cancel fontSize="small" />, color: 'default', label: 'Refunded' },
  }
  return config[status]
}

export function getAdminOrderStatusConfig(status: OrderStatus): {
  label: string
  color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'
  icon: React.ReactElement
} {
  const configs: Record<
    OrderStatus,
    {
      label: string
      color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'
      icon: React.ReactElement
    }
  > = {
    pending: { label: 'Pending', color: 'warning', icon: <Pending fontSize="small" /> },
    payment_pending: {
      label: 'Awaiting Payment',
      color: 'warning',
      icon: <Pending fontSize="small" />,
    },
    paid: { label: 'Paid', color: 'info', icon: <AttachMoney fontSize="small" /> },
    processing: { label: 'Processing', color: 'info', icon: <LocalShipping fontSize="small" /> },
    shipped: { label: 'Shipped', color: 'primary', icon: <LocalShipping fontSize="small" /> },
    delivered: { label: 'Delivered', color: 'success', icon: <Inventory fontSize="small" /> },
    completed: { label: 'Completed', color: 'success', icon: <CheckCircle fontSize="small" /> },
    cancelled: { label: 'Cancelled', color: 'error', icon: <Cancel fontSize="small" /> },
    disputed: { label: 'Disputed', color: 'error', icon: <Cancel fontSize="small" /> },
    refunded: { label: 'Refunded', color: 'default', icon: <Receipt fontSize="small" /> },
  }
  return configs[status]
}

export function getOrderActiveStep(status: OrderStatus): number {
  const stepMap: Record<OrderStatus, number> = {
    pending: 0,
    payment_pending: 0,
    paid: 1,
    processing: 1,
    shipped: 2,
    delivered: 3,
    completed: 4,
    cancelled: -1,
    disputed: -1,
    refunded: -1,
  }
  return stepMap[status]
}

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
