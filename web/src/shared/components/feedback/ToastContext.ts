import { createContext } from 'react'
import type { AlertColor } from '@mui/material'

export interface ToastMessage {
  id: string
  severity: AlertColor
  title?: string
  message: string
  duration?: number
}

export interface ToastContextValue {
  showToast: (options: Omit<ToastMessage, 'id'>) => void
  showSuccess: (message: string, title?: string) => void
  showError: (message: string, title?: string) => void
  showWarning: (message: string, title?: string) => void
  showInfo: (message: string, title?: string) => void
}

export const ToastContext = createContext<ToastContextValue | null>(null)
