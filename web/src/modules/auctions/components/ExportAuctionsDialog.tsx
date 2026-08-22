import { Construction } from '@mui/icons-material'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'

interface ExportAuctionsDialogProps {
  open: boolean
  onClose: () => void
}

export function ExportAuctionsDialog({ open, onClose }: ExportAuctionsDialogProps) {
  const { t } = useTranslation('common')

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('import.exportTitle')}</DialogTitle>
      <DialogContent sx={{ textAlign: 'center', py: 4 }}>
        <Construction sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography variant="body1" color="text.secondary">
          {t('import.exportComingSoon')}
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('close')}</Button>
      </DialogActions>
    </Dialog>
  )
}
