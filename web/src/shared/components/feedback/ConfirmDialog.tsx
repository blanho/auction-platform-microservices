import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  Box,
  useTheme,
  alpha,
} from '@mui/material'
import { Warning as WarningIcon, Info as InfoIcon } from '@mui/icons-material'
import { LoadingButton } from '../inputs/LoadingButton'

type ConfirmVariant = 'danger' | 'warning' | 'info'

interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void | Promise<void>
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  variant?: ConfirmVariant
  loading?: boolean
}

const variantStyles: Record<ConfirmVariant, { color: string; icon: React.ReactNode }> = {
  danger: {
    color: 'error.main',
    icon: <WarningIcon />,
  },
  warning: {
    color: 'warning.main',
    icon: <WarningIcon />,
  },
  info: {
    color: 'info.main',
    icon: <InfoIcon />,
  },
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  variant = 'danger',
  loading = false,
}: ConfirmDialogProps) {
  const theme = useTheme()
  const styles = variantStyles[variant]

  const handleConfirm = async () => {
    await onConfirm()
  }

  return (
    <Dialog
      open={open}
      onClose={loading ? undefined : onClose}
      maxWidth="xs"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: 3,
          overflow: 'hidden',
        },
      }}
    >
      <Box
        sx={{
          backgroundColor: alpha(theme.palette[variant === 'danger' ? 'error' : variant].main, 0.08),
          px: 3,
          py: 2,
          display: 'flex',
          alignItems: 'center',
          gap: 1.5,
          color: styles.color,
        }}
      >
        {styles.icon}
        <DialogTitle sx={{ p: 0, color: 'inherit', fontWeight: 600 }}>
          {title}
        </DialogTitle>
      </Box>
      <DialogContent sx={{ pt: 3 }}>
        <DialogContentText>{message}</DialogContentText>
      </DialogContent>
          <DialogActions sx={{ px: 3, pb: 3 }}>
            <Button
              onClick={onClose}
              disabled={loading}
              variant="outlined"
              color="inherit"
              sx={{ cursor: 'pointer' }}
            >
              {cancelText}
            </Button>
            <LoadingButton
              onClick={handleConfirm}
              loading={loading}
              variant="contained"
              color={variant === 'danger' ? 'error' : variant}
              autoFocus
            >
              {confirmText}
            </LoadingButton>
          </DialogActions>
        </Dialog>
  )
}
