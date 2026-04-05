import { useTranslation } from 'react-i18next'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Typography,
  TextField,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

interface SuspendUserDialogProps {
  open: boolean
  user: AdminUser | null
  reason: string
  loading: boolean
  onClose: () => void
  onReasonChange: (reason: string) => void
  onConfirm: () => void
}

export function SuspendUserDialog({
  open,
  user,
  reason,
  loading,
  onClose,
  onReasonChange,
  onConfirm,
}: Readonly<SuspendUserDialogProps>) {
  const { t } = useTranslation('common')
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('userManagement.suspendTitle')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <InlineAlert severity="warning">
            {t('userManagement.suspendWarning')}
          </InlineAlert>
          <Typography>
            {t('userManagement.suspendQuestion')} <strong>{getAdminUserDisplayName(user)}</strong>?
          </Typography>
          <TextField
            label={t('userManagement.suspendReason')}
            fullWidth
            multiline
            rows={3}
            value={reason}
            onChange={(e) => onReasonChange(e.target.value)}
            placeholder={t('userManagement.suspendReasonPlaceholder')}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('cancel')}</Button>
        <Button variant="contained" color="error" onClick={onConfirm} disabled={loading}>
          {loading ? t('userManagement.suspending') : t('userManagement.suspendButton')}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
