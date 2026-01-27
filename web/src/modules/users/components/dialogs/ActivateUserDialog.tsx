import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Alert,
  Typography,
} from '@mui/material'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

interface ActivateUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function ActivateUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: ActivateUserDialogProps) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Activate User</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="info">This will allow the user to access the platform again.</Alert>
          <Typography>
            Are you sure you want to activate <strong>{getAdminUserDisplayName(user)}</strong>?
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="success" onClick={onConfirm} disabled={loading}>
          {loading ? 'Activating...' : 'Activate User'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
