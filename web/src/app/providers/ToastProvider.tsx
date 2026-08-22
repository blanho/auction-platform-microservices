import type { AlertColor } from '@mui/material'
import { Alert, Snackbar } from '@mui/material'
import type { ReactNode } from 'react'
import { useCallback, useMemo, useState } from 'react'
import type { Toast, ToastContextValue } from '../context/ToastContext'
import { ToastContext } from '../context/ToastContext'

interface ToastProviderProps {
  children: ReactNode
}

export function ToastProvider({ children }: ToastProviderProps) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const addToast = useCallback((message: string, severity: AlertColor, duration = 4000) => {
    const id = crypto.randomUUID()
    setToasts((prev) => [...prev, { id, message, severity, duration }])
  }, [])

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  const value = useMemo<ToastContextValue>(
    () => ({
      success: (message: string) => addToast(message, 'success'),
      error: (message: string) => addToast(message, 'error', 6000),
      warning: (message: string) => addToast(message, 'warning'),
      info: (message: string) => addToast(message, 'info'),
    }),
    [addToast]
  )

  return (
    <ToastContext.Provider value={value}>
      {children}
      {toasts.map((toast, index) => (
        <Snackbar
          key={toast.id}
          open
          autoHideDuration={toast.duration}
          onClose={() => removeToast(toast.id)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          sx={{ bottom: { xs: 24 + index * 64, sm: 24 + index * 64 } }}
        >
          <Alert
            onClose={() => removeToast(toast.id)}
            severity={toast.severity}
            variant="filled"
            elevation={6}
            sx={{ width: '100%', minWidth: 300 }}
          >
            {toast.message}
          </Alert>
        </Snackbar>
      ))}
    </ToastContext.Provider>
  )
}
