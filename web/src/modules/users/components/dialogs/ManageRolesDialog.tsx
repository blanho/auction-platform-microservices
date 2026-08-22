import { InlineAlert } from '@/shared/ui'
import {
  Box,
  Button,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { AVAILABLE_ROLES, ROLE_COLORS, ROLE_DESCRIPTIONS, type UserRole } from '../../constants'
import type { AdminUser } from '../../types'
import { getAdminUserDisplayName } from '../../utils'

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
  const { t } = useTranslation('common')
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('userManagement.manageRolesTitle')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <InlineAlert severity="info">
            {t('userManagement.manageRolesInfo')} <strong>{getAdminUserDisplayName(user)}</strong>
          </InlineAlert>
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
        <Button onClick={onClose}>{t('cancel')}</Button>
        <Button
          variant="contained"
          onClick={onConfirm}
          disabled={loading || selectedRoles.length === 0}
        >
          {loading ? t('userManagement.saving') : t('userManagement.saveRoles')}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
