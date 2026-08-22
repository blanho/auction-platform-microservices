import { useContext } from 'react'
import type { ToastContextValue } from '../context/ToastContext'
import { ToastContext } from '../context/ToastContext'

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider')
  }
  return context
}
