import {
  ArrowUpward,
  ArrowDownward,
  History,
  Pending,
  CheckCircle,
  Cancel,
  Receipt,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { TransactionType, TransactionStatus } from '../types'

export interface TransactionTypeConfig {
  label: string
  icon: React.ReactNode
  color: string
  bgColor: string
}

export interface TransactionStatusConfig {
  label: string
  color: 'success' | 'warning' | 'error' | 'default'
  icon: React.ReactElement
}

export function getTransactionIcon(type: TransactionType): React.ReactElement {
  switch (type) {
    case 'deposit':
      return <ArrowDownward sx={{ color: palette.semantic.success }} />
    case 'withdrawal':
      return <ArrowUpward sx={{ color: palette.semantic.error }} />
    case 'payment':
      return <ArrowUpward sx={{ color: palette.semantic.error }} />
    case 'refund':
      return <ArrowDownward sx={{ color: palette.semantic.success }} />
    default:
      return <History sx={{ color: palette.neutral[500] }} />
  }
}

export function getTransactionStatusChipConfig(status: TransactionStatus): {
  color: 'success' | 'warning' | 'error' | 'default'
  label: string
} {
  const config: Record<
    TransactionStatus,
    { color: 'success' | 'warning' | 'error' | 'default'; label: string }
  > = {
    completed: { color: 'success', label: 'Completed' },
    pending: { color: 'warning', label: 'Pending' },
    failed: { color: 'error', label: 'Failed' },
    cancelled: { color: 'default', label: 'Cancelled' },
  }
  return config[status] || { color: 'default', label: status }
}

export function getTransactionTypeConfig(type: TransactionType): TransactionTypeConfig {
  const configs: Record<TransactionType, TransactionTypeConfig> = {
    deposit: {
      label: 'Deposit',
      icon: <ArrowDownward />,
      color: palette.semantic.success,
      bgColor: palette.semantic.successLight,
    },
    withdrawal: {
      label: 'Withdrawal',
      icon: <ArrowUpward />,
      color: palette.semantic.error,
      bgColor: palette.semantic.errorLight,
    },
    payment: {
      label: 'Payment',
      icon: <ArrowUpward />,
      color: palette.semantic.error,
      bgColor: palette.semantic.errorLight,
    },
    refund: {
      label: 'Refund',
      icon: <ArrowDownward />,
      color: palette.semantic.success,
      bgColor: palette.semantic.successLight,
    },
    hold: {
      label: 'Hold',
      icon: <Pending />,
      color: palette.semantic.warning,
      bgColor: palette.semantic.warningLight,
    },
    release: {
      label: 'Release',
      icon: <CheckCircle />,
      color: palette.semantic.success,
      bgColor: palette.semantic.successLight,
    },
    escrow_hold: {
      label: 'Escrow Hold',
      icon: <Pending />,
      color: palette.semantic.warning,
      bgColor: palette.semantic.warningLight,
    },
    escrow_release: {
      label: 'Escrow Release',
      icon: <CheckCircle />,
      color: palette.semantic.success,
      bgColor: palette.semantic.successLight,
    },
    fee: {
      label: 'Platform Fee',
      icon: <Receipt />,
      color: palette.neutral[500],
      bgColor: palette.neutral[100],
    },
  }
  return (
    configs[type] || {
      label: type,
      icon: <Receipt />,
      color: palette.neutral[500],
      bgColor: palette.neutral[100],
    }
  )
}

export function getTransactionStatusConfig(status: TransactionStatus): TransactionStatusConfig {
  const configs: Record<TransactionStatus, TransactionStatusConfig> = {
    completed: { label: 'Completed', color: 'success', icon: <CheckCircle fontSize="small" /> },
    pending: { label: 'Pending', color: 'warning', icon: <Pending fontSize="small" /> },
    failed: { label: 'Failed', color: 'error', icon: <Cancel fontSize="small" /> },
    cancelled: { label: 'Cancelled', color: 'default', icon: <Cancel fontSize="small" /> },
  }
  return configs[status]
}
