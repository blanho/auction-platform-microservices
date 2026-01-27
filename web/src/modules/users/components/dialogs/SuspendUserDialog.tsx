import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Alert,
  Typography,
  TextField,
} from '@mui/material'
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
}: SuspendUserDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Suspend User</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="warning">
            This will prevent the user from logging in and participating in auctions.
          </Alert>
          <Typography>
            Are you sure you want to suspend <strong>{getAdminUserDisplayName(user)}</strong>?
          </Typography>
          <TextField
            label="Reason for suspension"
            fullWidth
            multiline
            rows={3}
            value={reason}
            onChange={(e) => onReasonChange(e.target.value)}
            placeholder="Enter the reason for suspending this user..."
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="error" onClick={onConfirm} disabled={loading}>
          {loading ? 'Suspending...' : 'Suspend User'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
