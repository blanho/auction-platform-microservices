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

interface DeleteUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function DeleteUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: DeleteUserDialogProps) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Delete User</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="error">
            This action is permanent and cannot be undone. All user data, auctions, bids, and
            transaction history will be deleted.
          </Alert>
          <Typography>
            Are you sure you want to delete <strong>{getAdminUserDisplayName(user)}</strong>?
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="error" onClick={onConfirm} disabled={loading}>
          {loading ? 'Deleting...' : 'Delete User'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
