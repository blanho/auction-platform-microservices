import type { TransactionType, TransactionStatus } from '../types'
import {
  TRANSACTION_TYPE_COLORS,
  TRANSACTION_TYPE_LABELS,
  TRANSACTION_STATUS_COLORS,
  TRANSACTION_STATUS_LABELS,
  CURRENCY_SYMBOLS,
  PAYMENT_DEFAULT_CURRENCY,
} from '../constants'

export function getTransactionTypeColor(
  type: TransactionType
): 'success' | 'warning' | 'info' | 'error' | 'default' {
  return TRANSACTION_TYPE_COLORS[type] || 'default'
}

export function getTransactionTypeLabel(type: TransactionType): string {
  return TRANSACTION_TYPE_LABELS[type] || type
}

export function getTransactionStatusColor(
  status: TransactionStatus
): 'success' | 'warning' | 'info' | 'error' | 'default' {
  return TRANSACTION_STATUS_COLORS[status] || 'default'
}

export function getTransactionStatusLabel(status: TransactionStatus): string {
  return TRANSACTION_STATUS_LABELS[status] || status
}

export function isPositiveTransaction(type: TransactionType): boolean {
  return type === 'deposit' || type === 'refund' || type === 'release'
}

export function isNegativeTransaction(type: TransactionType): boolean {
  return type === 'withdrawal' || type === 'payment' || type === 'hold'
}

export function formatWalletBalance(amount: number, currency: string = PAYMENT_DEFAULT_CURRENCY): string {
  const symbol = CURRENCY_SYMBOLS[currency] || '$'
  return `${symbol}${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

export function formatTransactionAmount(
  amount: number,
  type: TransactionType,
  currency: string = PAYMENT_DEFAULT_CURRENCY
): string {
  const symbol = CURRENCY_SYMBOLS[currency] || '$'
  const formattedAmount = amount.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
  const sign = isPositiveTransaction(type) ? '+' : '-'
  return `${sign}${symbol}${formattedAmount}`
}

export function calculateAvailableBalance(balance: number, heldAmount: number): number {
  return Math.max(0, balance - heldAmount)
}

export function canWithdraw(availableBalance: number, amount: number): boolean {
  return availableBalance >= amount && amount > 0
}
