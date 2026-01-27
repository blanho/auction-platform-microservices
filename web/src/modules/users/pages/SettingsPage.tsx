import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Card,
  Grid,
  Alert,
  CircularProgress,
  Switch,
  Divider,
  IconButton,
  InputAdornment,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from '@mui/material'
import {
  Visibility,
  VisibilityOff,
  Lock,
  Security,
  Notifications,
  Palette,
  Language,
  ChevronRight,
  Shield,
} from '@mui/icons-material'
import { useProfile, useChangePassword, useEnableTwoFactor, useDisableTwoFactor } from '../hooks'
import { useNotificationPreferences, useUpdateNotificationPreferences } from '@/modules/notifications/hooks'
import { changePasswordSchema } from '../schemas'
import type { ChangePasswordRequest } from '../types'

export function SettingsPage() {
  const [activeSection, setActiveSection] = useState<'security' | 'notifications' | 'appearance'>('security')
  const [showCurrentPassword, setShowCurrentPassword] = useState(false)
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [show2FADialog, setShow2FADialog] = useState(false)
  const [passwordChangeSuccess, setPasswordChangeSuccess] = useState(false)

  const { data: profile } = useProfile()
  const changePassword = useChangePassword()
  const enableTwoFactor = useEnableTwoFactor()
  const disableTwoFactor = useDisableTwoFactor()
  const { data: notificationPreferences, isLoading: prefsLoading } = useNotificationPreferences()
  const updatePreferences = useUpdateNotificationPreferences()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordRequest>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  })

  const onPasswordSubmit = async (data: ChangePasswordRequest) => {
    try {
      await changePassword.mutateAsync(data)
      setPasswordChangeSuccess(true)
      reset()
      setTimeout(() => setPasswordChangeSuccess(false), 5000)
    } catch (err) {
      console.error(err)
    }
  }

  const handleToggle2FA = async () => {
    if (profile?.twoFactorEnabled) {
      setShow2FADialog(true)
    } else {
      try {
        await enableTwoFactor.mutateAsync()
      } catch (err) {
        console.error(err)
      }
    }
  }

  const handleDisable2FA = async () => {
    try {
      await disableTwoFactor.mutateAsync()
      setShow2FADialog(false)
    } catch (err) {
      console.error(err)
    }
  }

  const menuItems = [
    { id: 'security', icon: <Security />, label: 'Security', description: 'Password and 2FA' },
    { id: 'notifications', icon: <Notifications />, label: 'Notifications', description: 'Email and push preferences' },
    { id: 'appearance', icon: <Palette />, label: 'Appearance', description: 'Theme and display' },
  ]

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 600,
            color: '#1C1917',
          }}
        >
          Settings
        </Typography>
        <Typography sx={{ color: '#78716C' }}>
          Manage your account security and preferences
        </Typography>
      </Box>

      <Grid container spacing={4}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            }}
          >
            <List disablePadding>
              {menuItems.map((item, index) => (
                <ListItemButton
                  key={item.id}
                  selected={activeSection === item.id}
                  onClick={() => setActiveSection(item.id as typeof activeSection)}
                  sx={{
                    py: 2,
                    borderBottom: index < menuItems.length - 1 ? '1px solid #F5F5F5' : 'none',
                    '&.Mui-selected': {
                      bgcolor: '#FEF3C7',
                      '&:hover': { bgcolor: '#FEF3C7' },
                    },
                  }}
                >
                  <ListItemIcon sx={{ color: activeSection === item.id ? '#CA8A04' : '#78716C' }}>
                    {item.icon}
                  </ListItemIcon>
                  <ListItemText
                    primary={item.label}
                    secondary={item.description}
                    primaryTypographyProps={{ fontWeight: 500, color: '#1C1917' }}
                    secondaryTypographyProps={{ fontSize: '0.8125rem' }}
                  />
                  <ChevronRight sx={{ color: '#A1A1AA' }} />
                </ListItemButton>
              ))}
            </List>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          {activeSection === 'security' && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <Card
                sx={{
                  p: 4,
                  borderRadius: 2,
                  boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
                  <Lock sx={{ color: '#CA8A04' }} />
                  <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917' }}>
                    Change Password
                  </Typography>
                </Box>

                {passwordChangeSuccess && (
                  <Alert severity="success" sx={{ mb: 3 }}>
                    Password changed successfully!
                  </Alert>
                )}

                {changePassword.isError && (
                  <Alert severity="error" sx={{ mb: 3 }}>
                    Failed to change password. Please check your current password.
                  </Alert>
                )}

                <form onSubmit={handleSubmit(onPasswordSubmit)}>
                  <TextField
                    fullWidth
                    label="Current Password"
                    type={showCurrentPassword ? 'text' : 'password'}
                    {...register('currentPassword')}
                    error={!!errors.currentPassword}
                    helperText={errors.currentPassword?.message}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                            edge="end"
                            size="small"
                          >
                            {showCurrentPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    sx={{ mb: 2.5 }}
                  />

                  <TextField
                    fullWidth
                    label="New Password"
                    type={showNewPassword ? 'text' : 'password'}
                    {...register('newPassword')}
                    error={!!errors.newPassword}
                    helperText={errors.newPassword?.message}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowNewPassword(!showNewPassword)}
                            edge="end"
                            size="small"
                          >
                            {showNewPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    sx={{ mb: 2.5 }}
                  />

                  <TextField
                    fullWidth
                    label="Confirm New Password"
                    type={showConfirmPassword ? 'text' : 'password'}
                    {...register('confirmPassword')}
                    error={!!errors.confirmPassword}
                    helperText={errors.confirmPassword?.message}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            edge="end"
                            size="small"
                          >
                            {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    sx={{ mb: 3 }}
                  />

                  <Button
                    type="submit"
                    variant="contained"
                    disabled={isSubmitting || changePassword.isPending}
                    sx={{
                      bgcolor: '#1C1917',
                      textTransform: 'none',
                      fontWeight: 600,
                      px: 4,
                      '&:hover': { bgcolor: '#44403C' },
                    }}
                  >
                    {isSubmitting || changePassword.isPending ? (
                      <CircularProgress size={20} color="inherit" />
                    ) : (
                      'Update Password'
                    )}
                  </Button>
                </form>
              </Card>

              <Card
                sx={{
                  p: 4,
                  borderRadius: 2,
                  boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                  <Shield sx={{ color: '#CA8A04' }} />
                  <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917' }}>
                    Two-Factor Authentication
                  </Typography>
                </Box>

                <Typography sx={{ color: '#78716C', mb: 3, fontSize: '0.9375rem' }}>
                  Add an extra layer of security to your account by enabling two-factor authentication.
                </Typography>

                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    p: 2,
                    bgcolor: '#FAFAF9',
                    borderRadius: 1,
                  }}
                >
                  <Box>
                    <Typography sx={{ fontWeight: 500, color: '#1C1917' }}>
                      {profile?.twoFactorEnabled ? 'Enabled' : 'Disabled'}
                    </Typography>
                    <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>
                      {profile?.twoFactorEnabled
                        ? 'Your account is protected with 2FA'
                        : 'Enable 2FA for enhanced security'}
                    </Typography>
                  </Box>
                  <Switch
                    checked={profile?.twoFactorEnabled || false}
                    onChange={handleToggle2FA}
                    disabled={enableTwoFactor.isPending || disableTwoFactor.isPending}
                    sx={{
                      '& .MuiSwitch-switchBase.Mui-checked': {
                        color: '#CA8A04',
                      },
                      '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                        bgcolor: '#CA8A04',
                      },
                    }}
                  />
                </Box>
              </Card>
            </Box>
          )}

          {activeSection === 'notifications' && (
            <Card
              sx={{
                p: 4,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917', mb: 3 }}>
                Notification Preferences
              </Typography>

              {prefsLoading ? (
                <CircularProgress />
              ) : (
                <>
                  {[
                    { key: 'emailEnabled', label: 'Email Notifications', description: 'Receive notifications via email' },
                    { key: 'pushEnabled', label: 'Push Notifications', description: 'Receive push notifications in browser' },
                    { key: 'bidUpdates', label: 'Bid Updates', description: 'Get notified when someone outbids you' },
                    { key: 'auctionUpdates', label: 'Auction Updates', description: 'Reminders before auctions end and status changes' },
                    { key: 'promotionalEmails', label: 'Promotional', description: 'Special offers and platform updates' },
                    { key: 'systemAlerts', label: 'System Alerts', description: 'Important account and security updates' },
                  ].map((item, index) => (
                    <Box key={item.key}>
                      <Box
                        sx={{
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'space-between',
                          py: 2,
                        }}
                      >
                        <Box>
                          <Typography sx={{ fontWeight: 500, color: '#1C1917' }}>
                            {item.label}
                          </Typography>
                          <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>
                            {item.description}
                          </Typography>
                        </Box>
                        <Switch
                          checked={Boolean(notificationPreferences?.[item.key as keyof typeof notificationPreferences])}
                          onChange={(e) => {
                            updatePreferences.mutate({ [item.key]: e.target.checked })
                          }}
                          sx={{
                            '& .MuiSwitch-switchBase.Mui-checked': {
                              color: '#CA8A04',
                            },
                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                              bgcolor: '#CA8A04',
                            },
                          }}
                        />
                      </Box>
                      {index < 5 && <Divider />}
                    </Box>
                  ))}
                </>
              )}
            </Card>
          )}

          {activeSection === 'appearance' && (
            <Card
              sx={{
                p: 4,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917', mb: 3 }}>
                Appearance
              </Typography>

              <Box sx={{ mb: 4 }}>
                <Typography sx={{ fontWeight: 500, color: '#1C1917', mb: 1.5 }}>Theme</Typography>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  {['Light', 'Dark', 'System'].map((theme) => (
                    <Button
                      key={theme}
                      variant={theme === 'Light' ? 'contained' : 'outlined'}
                      sx={{
                        px: 3,
                        py: 1,
                        textTransform: 'none',
                        ...(theme === 'Light'
                          ? {
                              bgcolor: '#1C1917',
                              '&:hover': { bgcolor: '#44403C' },
                            }
                          : {
                              borderColor: '#E5E5E5',
                              color: '#44403C',
                              '&:hover': { borderColor: '#1C1917' },
                            }),
                      }}
                    >
                      {theme}
                    </Button>
                  ))}
                </Box>
              </Box>

              <Box>
                <Typography sx={{ fontWeight: 500, color: '#1C1917', mb: 1.5 }}>Language</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Language sx={{ color: '#78716C' }} />
                  <Typography>English (US)</Typography>
                </Box>
              </Box>
            </Card>
          )}
        </Grid>
      </Grid>

      <Dialog open={show2FADialog} onClose={() => setShow2FADialog(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Disable Two-Factor Authentication?</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: '#78716C' }}>
            Disabling 2FA will make your account less secure. Are you sure you want to continue?
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShow2FADialog(false)}
            sx={{ color: '#78716C', textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleDisable2FA}
            disabled={disableTwoFactor.isPending}
            sx={{
              bgcolor: '#DC2626',
              textTransform: 'none',
              '&:hover': { bgcolor: '#B91C1C' },
            }}
          >
            {disableTwoFactor.isPending ? (
              <CircularProgress size={20} color="inherit" />
            ) : (
              'Disable 2FA'
            )}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
