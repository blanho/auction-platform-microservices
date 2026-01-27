import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Alert,
  Box,
  Checkbox,
  FormControlLabel,
  Chip,
  Typography,
} from '@mui/material'
import { AVAILABLE_ROLES, ROLE_DESCRIPTIONS, ROLE_COLORS, type UserRole } from '../../constants'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

interface ManageRolesDialogProps {
  open: boolean
  user: AdminUser | null
  selectedRoles: string[]
  loading: boolean
  onClose: () => void
  onRoleToggle: (role: string) => void
  onConfirm: () => void
}

export function ManageRolesDialog({
  open,
  user,
  selectedRoles,
  loading,
  onClose,
  onRoleToggle,
  onConfirm,
}: ManageRolesDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Manage User Roles</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="info">
            Select the roles for <strong>{getAdminUserDisplayName(user)}</strong>
          </Alert>
          <Box>
            {AVAILABLE_ROLES.map((role) => (
              <FormControlLabel
                key={role}
                control={
                  <Checkbox
                    checked={selectedRoles.includes(role)}
                    onChange={() => onRoleToggle(role)}
                  />
                }
                label={
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Chip
                      label={role}
                      size="small"
                      color={ROLE_COLORS[role as UserRole] || 'default'}
                      sx={{ textTransform: 'capitalize' }}
                    />
                    <Typography variant="caption" color="text.secondary">
                      ({ROLE_DESCRIPTIONS[role as UserRole]})
                    </Typography>
                  </Box>
                }
                sx={{ display: 'block', mb: 1 }}
              />
            ))}
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={onConfirm}
          disabled={loading || selectedRoles.length === 0}
        >
          {loading ? 'Saving...' : 'Save Roles'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
