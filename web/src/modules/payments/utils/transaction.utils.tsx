import i18n from '@/i18n'
import { palette } from '@/shared/theme/tokens'
import {
  ArrowDownward,
  ArrowUpward,
  Cancel,
  CheckCircle,
  History,
  Pending,
  Receipt,
} from '@mui/icons-material'
import type { TransactionStatus, TransactionType } from '../types'

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
  const result = configs[type] || {
    label: type,
    icon: <Receipt />,
    color: palette.neutral[500],
    bgColor: palette.neutral[100],
  }
  return {
    ...result,
    label: i18n.t(`payments:transactionTypes.${type}`, { defaultValue: result.label }),
  }
}

export function getTransactionStatusConfig(status: TransactionStatus): TransactionStatusConfig {
  const configs: Record<TransactionStatus, TransactionStatusConfig> = {
    completed: { label: 'Completed', color: 'success', icon: <CheckCircle fontSize="small" /> },
    pending: { label: 'Pending', color: 'warning', icon: <Pending fontSize="small" /> },
    failed: { label: 'Failed', color: 'error', icon: <Cancel fontSize="small" /> },
    cancelled: { label: 'Cancelled', color: 'default', icon: <Cancel fontSize="small" /> },
  }
  const result = configs[status]
  return {
    ...result,
    label: i18n.t(`payments:status.${status}`, { defaultValue: result.label }),
  }
}
