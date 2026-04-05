import { useTranslation } from 'react-i18next'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
} from '@mui/material'
import { Construction } from '@mui/icons-material'

interface BulkImportDialogProps {
  open: boolean
  onClose: () => void
  onComplete: () => void
}

export function BulkImportDialog({ open, onClose, onComplete: _onComplete }: BulkImportDialogProps) {
  const { t } = useTranslation('common')

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('import.bulkTitle')}</DialogTitle>
      <DialogContent sx={{ textAlign: 'center', py: 4 }}>
        <Construction sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography variant="body1" color="text.secondary">
          {t('import.bulkComingSoon')}
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('close')}</Button>
      </DialogActions>
    </Dialog>
  )
}
