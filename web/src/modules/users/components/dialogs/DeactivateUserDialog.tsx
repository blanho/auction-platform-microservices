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

interface DeactivateUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function DeactivateUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: DeactivateUserDialogProps) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Deactivate User</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="warning">
            This will disable the user&apos;s account. They will not be able to log in.
          </Alert>
          <Typography>
            Are you sure you want to deactivate <strong>{getAdminUserDisplayName(user)}</strong>?
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="warning" onClick={onConfirm} disabled={loading}>
          {loading ? 'Deactivating...' : 'Deactivate User'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
