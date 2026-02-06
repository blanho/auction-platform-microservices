import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { palette } from '@/shared/theme/tokens'
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  Grid,
  CircularProgress,
  Switch,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import {
  Lock,
  Security,
  Notifications,
  Palette,
  Language,
  ChevronRight,
  Shield,
  Close,
} from '@mui/icons-material'
import { useProfile, useChangePassword, useDisableTwoFactor } from '../hooks'
import {
  useNotificationPreferences,
  useUpdateNotificationPreferences,
} from '@/modules/notifications/hooks'
import { changePasswordSchema } from '../schemas'
import type { ChangePasswordRequest } from '../types'
import { TwoFactorSetup } from '@/modules/auth/components/TwoFactorSetup'

export function SettingsPage() {
  const [activeSection, setActiveSection] = useState<'security' | 'notifications' | 'appearance'>(
    'security'
  )
  const [show2FADialog, setShow2FADialog] = useState(false)
  const [passwordChangeSuccess, setPasswordChangeSuccess] = useState(false)

  const { data: profile } = useProfile()
  const changePassword = useChangePassword()
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
    {
      id: 'notifications',
      icon: <Notifications />,
      label: 'Notifications',
      description: 'Email and push preferences',
    },
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
            color: palette.neutral[900],
          }}
        >
          Settings
        </Typography>
        <Typography sx={{ color: palette.neutral[500] }}>
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
                    borderBottom:
                      index < menuItems.length - 1 ? `1px solid ${palette.neutral[100]}` : 'none',
                    '&.Mui-selected': {
                      bgcolor: palette.brand.muted,
                      '&:hover': { bgcolor: palette.brand.muted },
                    },
                  }}
                >
                  <ListItemIcon
                    sx={{
                      color:
                        activeSection === item.id ? palette.brand.primary : palette.neutral[500],
                    }}
                  >
                    {item.icon}
                  </ListItemIcon>
                  <ListItemText
                    primary={item.label}
                    secondary={item.description}
                    primaryTypographyProps={{ fontWeight: 500, color: palette.neutral[900] }}
                    secondaryTypographyProps={{ fontSize: '0.8125rem' }}
                  />
                  <ChevronRight sx={{ color: palette.neutral[400] }} />
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
                  <Lock sx={{ color: palette.brand.primary }} />
                  <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                    Change Password
                  </Typography>
                </Box>

                {passwordChangeSuccess && (
                  <InlineAlert severity="success" sx={{ mb: 3 }}>
                    Password changed successfully!
                  </InlineAlert>
                )}

                {changePassword.isError && (
                  <InlineAlert severity="error" sx={{ mb: 3 }}>
                    Failed to change password. Please check your current password.
                  </InlineAlert>
                )}

                <form onSubmit={handleSubmit(onPasswordSubmit)}>
                  <FormField
                    name="currentPassword"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="Current Password"
                    type="password"
                    showPasswordToggle
                    sx={{ mb: 2.5 }}
                  />

                  <FormField
                    name="newPassword"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="New Password"
                    type="password"
                    showPasswordToggle
                    sx={{ mb: 2.5 }}
                  />

                  <FormField
                    name="confirmPassword"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="Confirm New Password"
                    type="password"
                    showPasswordToggle
                    sx={{ mb: 3 }}
                  />

                  <Button
                    type="submit"
                    variant="contained"
                    disabled={isSubmitting || changePassword.isPending}
                    sx={{
                      bgcolor: palette.neutral[900],
                      textTransform: 'none',
                      fontWeight: 600,
                      px: 4,
                      '&:hover': { bgcolor: palette.neutral[700] },
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
                  <Shield sx={{ color: palette.brand.primary }} />
                  <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                    Two-Factor Authentication
                  </Typography>
                </Box>

                <Typography sx={{ color: palette.neutral[500], mb: 3, fontSize: '0.9375rem' }}>
                  Add an extra layer of security to your account by enabling two-factor
                  authentication.
                </Typography>

                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    p: 2,
                    bgcolor: palette.neutral[50],
                    borderRadius: 1,
                    mb: 2,
                  }}
                >
                  <Box>
                    <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                      {profile?.twoFactorEnabled ? 'Enabled' : 'Disabled'}
                    </Typography>
                    <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                      {profile?.twoFactorEnabled
                        ? 'Your account is protected with 2FA'
                        : 'Enable 2FA for enhanced security'}
                    </Typography>
                  </Box>
                  <Button
                    variant={profile?.twoFactorEnabled ? 'outlined' : 'contained'}
                    onClick={() => setShow2FADialog(true)}
                    sx={{
                      textTransform: 'none',
                      fontWeight: 600,
                      ...(profile?.twoFactorEnabled
                        ? {
                            borderColor: palette.semantic.error,
                            color: palette.semantic.error,
                            '&:hover': {
                              borderColor: palette.semantic.errorHover,
                              bgcolor: palette.semantic.errorLight,
                            },
                          }
                        : {
                            bgcolor: palette.brand.primary,
                            '&:hover': { bgcolor: '#A16207' },
                          }),
                    }}
                  >
                    {profile?.twoFactorEnabled ? 'Manage 2FA' : 'Enable 2FA'}
                  </Button>
                </Box>
              </Card>
            </Box>
          )}

          <Dialog
            open={show2FADialog}
            onClose={() => setShow2FADialog(false)}
            maxWidth="sm"
            fullWidth
            PaperProps={{
              sx: { borderRadius: 2 },
            }}
          >
            <DialogTitle
              sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                {profile?.twoFactorEnabled
                  ? 'Manage Two-Factor Authentication'
                  : 'Set Up Two-Factor Authentication'}
              </Typography>
              <IconButton onClick={() => setShow2FADialog(false)} size="small">
                <Close />
              </IconButton>
            </DialogTitle>
            <DialogContent>
              <TwoFactorSetup
                isEnabled={profile?.twoFactorEnabled || false}
                onComplete={() => setShow2FADialog(false)}
              />
            </DialogContent>
          </Dialog>

          {activeSection === 'notifications' && (
            <Card
              sx={{
                p: 4,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900], mb: 3 }}>
                Notification Preferences
              </Typography>

              {prefsLoading ? (
                <CircularProgress />
              ) : (
                <>
                  {[
                    {
                      key: 'emailEnabled',
                      label: 'Email Notifications',
                      description: 'Receive notifications via email',
                    },
                    {
                      key: 'pushEnabled',
                      label: 'Push Notifications',
                      description: 'Receive push notifications in browser',
                    },
                    {
                      key: 'bidUpdates',
                      label: 'Bid Updates',
                      description: 'Get notified when someone outbids you',
                    },
                    {
                      key: 'auctionUpdates',
                      label: 'Auction Updates',
                      description: 'Reminders before auctions end and status changes',
                    },
                    {
                      key: 'promotionalEmails',
                      label: 'Promotional',
                      description: 'Special offers and platform updates',
                    },
                    {
                      key: 'systemAlerts',
                      label: 'System Alerts',
                      description: 'Important account and security updates',
                    },
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
                          <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                            {item.label}
                          </Typography>
                          <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                            {item.description}
                          </Typography>
                        </Box>
                        <Switch
                          checked={Boolean(
                            notificationPreferences?.[
                              item.key as keyof typeof notificationPreferences
                            ]
                          )}
                          onChange={(e) => {
                            updatePreferences.mutate({ [item.key]: e.target.checked })
                          }}
                          sx={{
                            '& .MuiSwitch-switchBase.Mui-checked': {
                              color: palette.brand.primary,
                            },
                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                              bgcolor: palette.brand.primary,
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
              <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900], mb: 3 }}>
                Appearance
              </Typography>

              <Box sx={{ mb: 4 }}>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[900], mb: 1.5 }}>
                  Theme
                </Typography>
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
                              bgcolor: palette.neutral[900],
                              '&:hover': { bgcolor: palette.neutral[700] },
                            }
                          : {
                              borderColor: palette.neutral[200],
                              color: palette.neutral[700],
                              '&:hover': { borderColor: palette.neutral[900] },
                            }),
                      }}
                    >
                      {theme}
                    </Button>
                  ))}
                </Box>
              </Box>

              <Box>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[900], mb: 1.5 }}>
                  Language
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Language sx={{ color: palette.neutral[500] }} />
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
          <Typography sx={{ color: palette.neutral[500] }}>
            Disabling 2FA will make your account less secure. Are you sure you want to continue?
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShow2FADialog(false)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleDisable2FA}
            disabled={disableTwoFactor.isPending}
            sx={{
              bgcolor: palette.semantic.error,
              textTransform: 'none',
              '&:hover': { bgcolor: palette.semantic.errorHover },
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
