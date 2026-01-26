import type { TransactionType, TransactionStatus } from '../types'

export const TRANSACTION_TYPE = {
  DEPOSIT: 'deposit',
  WITHDRAWAL: 'withdrawal',
  PAYMENT: 'payment',
  REFUND: 'refund',
  HOLD: 'hold',
  RELEASE: 'release',
  ESCROW_HOLD: 'escrow_hold',
  ESCROW_RELEASE: 'escrow_release',
  FEE: 'fee',
} as const

export const TRANSACTION_TYPE_LABELS: Record<TransactionType, string> = {
  deposit: 'Deposit',
  withdrawal: 'Withdrawal',
  payment: 'Payment',
  refund: 'Refund',
  hold: 'Hold',
  release: 'Release',
  escrow_hold: 'Escrow Hold',
  escrow_release: 'Escrow Release',
  fee: 'Platform Fee',
}

export const TRANSACTION_TYPE_COLORS: Record<TransactionType, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  deposit: 'success',
  withdrawal: 'warning',
  payment: 'info',
  refund: 'success',
  hold: 'default',
  release: 'info',
  escrow_hold: 'warning',
  escrow_release: 'success',
  fee: 'default',
}

export const TRANSACTION_STATUS = {
  PENDING: 'pending',
  COMPLETED: 'completed',
  FAILED: 'failed',
  CANCELLED: 'cancelled',
} as const

export const TRANSACTION_STATUS_LABELS: Record<TransactionStatus, string> = {
  pending: 'Pending',
  completed: 'Completed',
  failed: 'Failed',
  cancelled: 'Cancelled',
}

export const TRANSACTION_STATUS_COLORS: Record<TransactionStatus, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  pending: 'warning',
  completed: 'success',
  failed: 'error',
  cancelled: 'default',
}

export const CURRENCY_SYMBOLS: Record<string, string> = {
  USD: '$',
  EUR: '€',
  GBP: '£',
  VND: '₫',
}

export const PAYMENT_DEFAULT_CURRENCY = 'USD'
