import { Chip } from '@mui/material'
import type { ChipProps } from '@mui/material'
import type { ReactNode } from 'react'

interface StatusBadgeProps extends Omit<ChipProps, 'label'> {
  status: string
  icon?: ReactNode
  colorMap?: Record<string, { bg: string; color: string }>
  fallbackColor?: { bg: string; color: string }
}

const defaultColorMap: Record<string, { bg: string; color: string }> = {
  active: { bg: '#D1FAE5', color: '#059669' },
  pending: { bg: '#FEF3C7', color: '#D97706' },
  success: { bg: '#D1FAE5', color: '#059669' },
  error: { bg: '#FEE2E2', color: '#DC2626' },
  warning: { bg: '#FED7AA', color: '#EA580C' },
  completed: { bg: '#D1FAE5', color: '#059669' },
  failed: { bg: '#FEE2E2', color: '#DC2626' },
  cancelled: { bg: '#E5E7EB', color: '#6B7280' },
  draft: { bg: '#F3F4F6', color: '#6B7280' },
  'ending-soon': { bg: '#FEF3C7', color: '#D97706' },
  ended: { bg: '#E5E7EB', color: '#6B7280' },
  sold: { bg: '#D1FAE5', color: '#059669' },
  accepted: { bg: '#D1FAE5', color: '#16A34A' },
  rejected: { bg: '#FEE2E2', color: '#DC2626' },
  retracted: { bg: '#E5E7EB', color: '#6B7280' },
  outbid: { bg: '#DBEAFE', color: '#3B82F6' },
  paid: { bg: '#D1FAE5', color: '#059669' },
  shipped: { bg: '#DBEAFE', color: '#0284C7' },
  delivered: { bg: '#D1FAE5', color: '#059669' },
  refunded: { bg: '#FEE2E2', color: '#DC2626' },
  payment_pending: { bg: '#FEF3C7', color: '#D97706' },
  processing: { bg: '#DBEAFE', color: '#0284C7' },
  confirmed: { bg: '#D1FAE5', color: '#059669' },
  in_progress: { bg: '#DBEAFE', color: '#0284C7' },
  generated: { bg: '#D1FAE5', color: '#059669' },
  scheduled: { bg: '#E0E7FF', color: '#4F46E5' },
  enabled: { bg: '#D1FAE5', color: '#059669' },
  disabled: { bg: '#E5E7EB', color: '#6B7280' },
}

export function StatusBadge({
  status,
  icon,
  colorMap = defaultColorMap,
  fallbackColor = { bg: '#F3F4F6', color: '#6B7280' },
  size = 'small',
  ...props
}: StatusBadgeProps) {
  const statusLower = status.toLowerCase()
  const colors = colorMap[statusLower] || fallbackColor

  return (
    <Chip
      icon={icon}
      label={status}
      size={size}
      sx={{
        bgcolor: colors.bg,
        color: colors.color,
        fontWeight: 600,
        fontSize: '0.75rem',
        textTransform: 'uppercase',
      }}
      {...props}
    />
  )
}
