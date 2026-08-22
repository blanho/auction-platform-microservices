import type { AlertColor } from '@mui/material'
import { createContext } from 'react'

export interface Toast {
  id: string
  message: string
  severity: AlertColor
  duration?: number
}

export interface ToastContextValue {
  success: (message: string) => void
  error: (message: string) => void
  warning: (message: string) => void
  info: (message: string) => void
}

export const ToastContext = createContext<ToastContextValue | null>(null)
