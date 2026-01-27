import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Alert,
  Box,
  Chip,
  Typography,
  Skeleton,
  Divider,
} from '@mui/material'
import { Security, PhonelinkLock, Key, Refresh, Block } from '@mui/icons-material'
import { getAdminUserDisplayName, get2FAStatusColor, getRecoveryCodesColor } from '../../utils'
import type { AdminUser, User2FAStatus } from '../../types'

interface TwoFactorDialogProps {
  open: boolean
  user: AdminUser | null
  status: User2FAStatus | undefined
  loading: boolean
  resetLoading: boolean
  disableLoading: boolean
  onClose: () => void
  onReset: () => void
  onDisable: () => void
}

export function TwoFactorDialog({
  open,
  user,
  status,
  loading,
  resetLoading,
  disableLoading,
  onClose,
  onReset,
  onDisable,
}: TwoFactorDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Security />
          Two-Factor Authentication
        </Box>
      </DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ pt: 1 }}>
          <Box>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              User
            </Typography>
            <Typography variant="body1" fontWeight={500}>
              {getAdminUserDisplayName(user)}
            </Typography>
          </Box>

          {loading ? (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Skeleton width="100%" height={40} />
              <Skeleton width="100%" height={40} />
            </Box>
          ) : (
            <>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <PhonelinkLock
                    sx={{ color: status?.twoFactorEnabled ? 'success.main' : 'text.disabled' }}
                  />
                  <Typography>2FA Status</Typography>
                </Box>
                <Chip
                  label={status?.twoFactorEnabled ? 'Enabled' : 'Disabled'}
                  color={get2FAStatusColor(status?.twoFactorEnabled || false)}
                  size="small"
                />
              </Box>

              {status?.twoFactorEnabled && (
                <>
                  <Box
                    sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
                  >
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Key
                        sx={{
                          color: status?.hasRecoveryCodes ? 'success.main' : 'warning.main',
                        }}
                      />
                      <Typography>Recovery Codes</Typography>
                    </Box>
                    <Chip
                      label={status?.hasRecoveryCodes ? 'Available' : 'Not Set'}
                      color={getRecoveryCodesColor(status?.hasRecoveryCodes || false)}
                      size="small"
                    />
                  </Box>

                  <Divider />

                  <Alert severity="warning">
                    These actions will affect the user&apos;s account security. Use with caution.
                  </Alert>

                  <Box sx={{ display: 'flex', gap: 2 }}>
                    <Button
                      variant="outlined"
                      color="warning"
                      startIcon={<Refresh />}
                      onClick={onReset}
                      disabled={resetLoading}
                      fullWidth
                    >
                      {resetLoading ? 'Resetting...' : 'Reset 2FA'}
                    </Button>
                    <Button
                      variant="outlined"
                      color="error"
                      startIcon={<Block />}
                      onClick={onDisable}
                      disabled={disableLoading}
                      fullWidth
                    >
                      {disableLoading ? 'Disabling...' : 'Disable 2FA'}
                    </Button>
                  </Box>
                </>
              )}

              {!status?.twoFactorEnabled && (
                <Alert severity="info">
                  This user has not enabled two-factor authentication.
                </Alert>
              )}
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}
