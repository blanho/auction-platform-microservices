import { useState, useCallback } from 'react'
import type { ReactNode } from 'react'
import { Alert, AlertTitle, IconButton, Box } from '@mui/material'
import { Close as CloseIcon } from '@mui/icons-material'
import { AnimatePresence, motion } from 'framer-motion'
import { notification } from '@/shared/lib/animations'
import { ToastContext, type ToastMessage } from './ToastContext'

interface ToastProviderProps {
  children: ReactNode
  maxToasts?: number
}

export function ToastProvider({ children, maxToasts = 3 }: ToastProviderProps) {
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  const removeToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id))
  }, [])

  const showToast = useCallback((options: Omit<ToastMessage, 'id'>) => {
    const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
    const toast: ToastMessage = {
      id,
      duration: 5000,
      ...options,
    }

    setToasts(prev => {
      const newToasts = [...prev, toast]
      if (newToasts.length > maxToasts) {
        return newToasts.slice(-maxToasts)
      }
      return newToasts
    })

    if (toast.duration && toast.duration > 0) {
      setTimeout(() => removeToast(id), toast.duration)
    }
  }, [maxToasts, removeToast])

  const showSuccess = useCallback((message: string, title?: string) => {
    showToast({ severity: 'success', message, title })
  }, [showToast])

  const showError = useCallback((message: string, title?: string) => {
    showToast({ severity: 'error', message, title, duration: 8000 })
  }, [showToast])

  const showWarning = useCallback((message: string, title?: string) => {
    showToast({ severity: 'warning', message, title })
  }, [showToast])

  const showInfo = useCallback((message: string, title?: string) => {
    showToast({ severity: 'info', message, title })
  }, [showToast])

  return (
    <ToastContext.Provider value={{ showToast, showSuccess, showError, showWarning, showInfo }}>
      {children}
      <Box
        sx={{
          position: 'fixed',
          top: 16,
          right: 16,
          zIndex: 9999,
          display: 'flex',
          flexDirection: 'column',
          gap: 1,
          maxWidth: 400,
        }}
      >
        <AnimatePresence mode="sync">
          {toasts.map((toast) => (
            <motion.div
              key={toast.id}
              initial="initial"
              animate="animate"
              exit="exit"
              variants={notification}
              layout
            >
              <Alert
                severity={toast.severity}
                variant="filled"
                onClose={() => removeToast(toast.id)}
                action={
                  <IconButton
                    size="small"
                    color="inherit"
                    onClick={() => removeToast(toast.id)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <CloseIcon fontSize="small" />
                  </IconButton>
                }
                sx={{
                  borderRadius: 2,
                  boxShadow: 6,
                  '& .MuiAlert-action': {
                    pt: 0,
                  },
                }}
              >
                {toast.title && <AlertTitle>{toast.title}</AlertTitle>}
                {toast.message}
              </Alert>
            </motion.div>
          ))}
        </AnimatePresence>
      </Box>
    </ToastContext.Provider>
  )
}
