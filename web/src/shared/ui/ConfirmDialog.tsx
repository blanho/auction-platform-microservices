import { forwardRef } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  CircularProgress,
} from '@mui/material'
import { Warning, CheckCircle, Info, Error } from '@mui/icons-material'

type ConfirmDialogVariant = 'warning' | 'danger' | 'info' | 'success'

interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  variant?: ConfirmDialogVariant
  loading?: boolean
}

const variantConfig: Record<ConfirmDialogVariant, { icon: React.ReactNode; color: string; bgColor: string; buttonColor: string }> = {
  warning: {
    icon: <Warning sx={{ fontSize: 32 }} />,
    color: '#CA8A04',
    bgColor: '#FEF3C7',
    buttonColor: '#CA8A04',
  },
  danger: {
    icon: <Error sx={{ fontSize: 32 }} />,
    color: '#DC2626',
    bgColor: '#FEE2E2',
    buttonColor: '#DC2626',
  },
  info: {
    icon: <Info sx={{ fontSize: 32 }} />,
    color: '#2563EB',
    bgColor: '#DBEAFE',
    buttonColor: '#2563EB',
  },
  success: {
    icon: <CheckCircle sx={{ fontSize: 32 }} />,
    color: '#16A34A',
    bgColor: '#DCFCE7',
    buttonColor: '#16A34A',
  },
}

export const ConfirmDialog = forwardRef<HTMLDivElement, ConfirmDialogProps>(function ConfirmDialog(
  {
    open,
    onClose,
    onConfirm,
    title,
    message,
    confirmLabel = 'Confirm',
    cancelLabel = 'Cancel',
    variant = 'warning',
    loading = false,
  },
  ref
) {
  const config = variantConfig[variant]

  return (
    <Dialog
      ref={ref}
      open={open}
      onClose={loading ? undefined : onClose}
      maxWidth="xs"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 },
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: '50%',
              bgcolor: config.bgColor,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: config.color,
            }}
          >
            {config.icon}
          </Box>
          <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917' }}>
            {title}
          </Typography>
        </Box>
      </DialogTitle>
      <DialogContent>
        <Typography sx={{ color: '#78716C', pl: 8 }}>
          {message}
        </Typography>
      </DialogContent>
      <DialogActions sx={{ p: 3, pt: 1 }}>
        <Button
          onClick={onClose}
          disabled={loading}
          sx={{ color: '#78716C', textTransform: 'none' }}
        >
          {cancelLabel}
        </Button>
        <Button
          variant="contained"
          onClick={onConfirm}
          disabled={loading}
          sx={{
            bgcolor: config.buttonColor,
            textTransform: 'none',
            fontWeight: 600,
            minWidth: 100,
            '&:hover': {
              bgcolor: variant === 'danger' ? '#B91C1C' : variant === 'warning' ? '#A16207' : variant === 'success' ? '#15803D' : '#1D4ED8',
            },
          }}
        >
          {loading ? <CircularProgress size={20} color="inherit" /> : confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
})
