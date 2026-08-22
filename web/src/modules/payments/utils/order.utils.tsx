import i18n from '@/i18n'
import {
  AttachMoney,
  Cancel,
  CheckCircle,
  Inventory,
  LocalShipping,
  Pending,
  Receipt,
} from '@mui/icons-material'
import type { OrderStatus } from '../types'

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
  const result = config[status]
  return {
    ...result,
    label: i18n.t(`payments:orderStatuses.${status}`, { defaultValue: result.label }),
  }
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
  const result = configs[status]
  return {
    ...result,
    label: i18n.t(`payments:orderStatuses.${status}`, { defaultValue: result.label }),
  }
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
