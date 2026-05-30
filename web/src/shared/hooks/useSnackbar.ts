import { useState, useCallback } from 'react'

type SnackbarSeverity = 'success' | 'error' | 'info' | 'warning'

interface SnackbarState {
  open: boolean
  message: string
  severity: SnackbarSeverity
}

interface UseSnackbarReturn extends SnackbarState {
  show: (message: string, severity?: SnackbarSeverity) => void
  close: () => void
}

const INITIAL_STATE: SnackbarState = { open: false, message: '', severity: 'info' }

/**
 * Manages a single snackbar/toast notification.
 * Decouples notification state from page-level business logic.
 */
export function useSnackbar(): UseSnackbarReturn {
  const [state, setState] = useState<SnackbarState>(INITIAL_STATE)

  const show = useCallback((message: string, severity: SnackbarSeverity = 'info') => {
    setState({ open: true, message, severity })
  }, [])

  const close = useCallback(() => {
    setState((prev) => ({ ...prev, open: false }))
  }, [])

  return { ...state, show, close }
}
