import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
} from '@mui/material'
import { Construction } from '@mui/icons-material'

interface ExportAuctionsDialogProps {
  open: boolean
  onClose: () => void
}

export function ExportAuctionsDialog({ open, onClose }: ExportAuctionsDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Export Auctions</DialogTitle>
      <DialogContent sx={{ textAlign: 'center', py: 4 }}>
        <Construction sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography variant="body1" color="text.secondary">
          Export functionality is coming soon.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}
