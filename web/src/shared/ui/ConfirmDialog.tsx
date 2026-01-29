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
import { palette } from '@/shared/theme/tokens'

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

const variantConfig: Record<
  ConfirmDialogVariant,
  { icon: React.ReactNode; color: string; bgColor: string; buttonColor: string; hoverColor: string }
> = {
  warning: {
    icon: <Warning sx={{ fontSize: 32 }} />,
    color: palette.brand.primary,
    bgColor: palette.brand.muted,
    buttonColor: palette.brand.primary,
    hoverColor: palette.brand.secondary,
  },
  danger: {
    icon: <Error sx={{ fontSize: 32 }} />,
    color: palette.semantic.error,
    bgColor: palette.semantic.errorLight,
    buttonColor: palette.semantic.error,
    hoverColor: palette.semantic.errorHover,
  },
  info: {
    icon: <Info sx={{ fontSize: 32 }} />,
    color: palette.semantic.info,
    bgColor: palette.semantic.infoLight,
    buttonColor: palette.semantic.info,
    hoverColor: palette.semantic.infoHover,
  },
  success: {
    icon: <CheckCircle sx={{ fontSize: 32 }} />,
    color: palette.semantic.success,
    bgColor: palette.semantic.successLight,
    buttonColor: palette.semantic.success,
    hoverColor: palette.semantic.successHover,
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
          <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
            {title}
          </Typography>
        </Box>
      </DialogTitle>
      <DialogContent>
        <Typography sx={{ color: palette.neutral[500], pl: 8 }}>{message}</Typography>
      </DialogContent>
      <DialogActions sx={{ p: 3, pt: 1 }}>
        <Button
          onClick={onClose}
          disabled={loading}
          sx={{ color: palette.neutral[500], textTransform: 'none' }}
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
              bgcolor: config.hoverColor,
            },
          }}
        >
          {loading ? <CircularProgress size={20} color="inherit" /> : confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
})
