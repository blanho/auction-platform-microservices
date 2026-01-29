import { Alert, AlertTitle } from '@mui/material'
import type { AlertColor, SxProps, Theme } from '@mui/material'
import type { ReactNode } from 'react'

interface InlineAlertProps {
  severity: AlertColor
  title?: string
  children: ReactNode
  variant?: 'standard' | 'outlined' | 'filled'
  sx?: SxProps<Theme>
}

export function InlineAlert({ severity, title, children, variant, sx }: InlineAlertProps) {
  return (
    <Alert severity={severity} variant={variant} sx={sx}>
      {title && <AlertTitle>{title}</AlertTitle>}
      {children}
    </Alert>
  )
}
