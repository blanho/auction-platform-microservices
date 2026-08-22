import { getCurrentLocale } from '@/i18n'
import { getErrorMessage } from '@/services/http'
import { palette } from '@/shared/theme/tokens'
import { FormField, InlineAlert } from '@/shared/ui'
import { zodResolver } from '@hookform/resolvers/zod'
import { CameraAlt, Cancel, CheckCircle, Pending, Store, Verified } from '@mui/icons-material'
import {
  Avatar,
  Box,
  Button,
  Card,
  Chip,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  IconButton,
  Skeleton,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { usersApi } from '../api'
import { useApplyForSeller, useProfile, useSellerStatus, useUpdateProfile } from '../hooks'
import { createUpdateProfileSchema } from '../schemas'
import type { UpdateProfileRequest } from '../types'

export function ProfilePage() {
  const { t } = useTranslation('users')
  const updateProfileSchema = createUpdateProfileSchema(t)
  const [isEditing, setIsEditing] = useState(false)
  const [uploadingAvatar, setUploadingAvatar] = useState(false)
  const [avatarError, setAvatarError] = useState<string | null>(null)
  const [showSellerDialog, setShowSellerDialog] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { data: profile, isLoading: profileLoading, error: profileError } = useProfile()
  const { data: sellerStatus, isLoading: sellerLoading } = useSellerStatus()
  const updateProfile = useUpdateProfile()
  const applyForSeller = useApplyForSeller()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<UpdateProfileRequest>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      fullName: profile?.fullName || '',
      phoneNumber: profile?.phoneNumber || '',
      bio: profile?.bio || '',
      location: profile?.location || '',
    },
  })

  useEffect(() => {
    if (!profile) {
      return
    }

    reset({
      fullName: profile.fullName || '',
      phoneNumber: profile.phoneNumber || '',
      bio: profile.bio || '',
      location: profile.location || '',
    })
  }, [profile, reset])

  const onSubmit = async (data: UpdateProfileRequest) => {
    try {
      await updateProfile.mutateAsync(data)
      setIsEditing(false)
    } catch {
      return
    }
  }

  const handleAvatarClick = () => {
    fileInputRef.current?.click()
  }

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) {
      return
    }

    setAvatarError(null)
    setUploadingAvatar(true)
    try {
      await usersApi.uploadAvatar(file)
    } catch (error) {
      setAvatarError(getErrorMessage(error))
    } finally {
      setUploadingAvatar(false)
    }
  }

  const handleApplyForSeller = async () => {
    try {
      await applyForSeller.mutateAsync(true)
      setShowSellerDialog(false)
    } catch {
      return
    }
  }

  const getSellerStatusChip = () => {
    if (sellerLoading) {
      return null
    }
    if (!sellerStatus) {
      return null
    }

    if (sellerStatus.isSeller) {
      return (
        <Chip icon={<Store />} label={t('profile.verifiedSeller')} color="success" size="small" />
      )
    }

    switch (sellerStatus.applicationStatus) {
      case 'pending':
        return (
          <Chip
            icon={<Pending />}
            label={t('profile.sellerApplicationPending')}
            color="warning"
            size="small"
          />
        )
      case 'rejected':
        return <Chip icon={<Cancel />} label={t('seller.rejected')} color="error" size="small" />
      default:
        return null
    }
  }

  if (profileLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <Grid container spacing={4}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Card sx={{ textAlign: 'center', p: 4 }}>
              <Skeleton variant="circular" width={120} height={120} sx={{ mx: 'auto', mb: 2 }} />
              <Skeleton variant="text" width="60%" sx={{ mx: 'auto' }} />
              <Skeleton variant="text" width="40%" sx={{ mx: 'auto' }} />
            </Card>
          </Grid>
          <Grid size={{ xs: 12, md: 8 }}>
            <Card sx={{ p: 3 }}>
              <Skeleton variant="text" width="30%" sx={{ mb: 2 }} />
              <Skeleton variant="rectangular" height={200} />
            </Card>
          </Grid>
        </Grid>
      </Container>
    )
  }

  if (profileError) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <InlineAlert severity="error">{t('profile.loadError')}</InlineAlert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 600,
            color: palette.neutral[900],
          }}
        >
          {t('profile.myProfile')}
        </Typography>
        <Typography sx={{ color: palette.neutral[500] }}>{t('profile.description')}</Typography>
      </Box>

      {avatarError && (
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {avatarError}
        </InlineAlert>
      )}

      <Grid container spacing={4}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              textAlign: 'center',
              p: 4,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            }}
          >
            <Box sx={{ position: 'relative', display: 'inline-block', mb: 3 }}>
              <Avatar
                src={profile?.avatarUrl}
                sx={{
                  width: 120,
                  height: 120,
                  fontSize: '3rem',
                  bgcolor: palette.neutral[900],
                }}
              >
                {(profile?.fullName || profile?.username)?.[0]?.toUpperCase()}
              </Avatar>
              <IconButton
                onClick={handleAvatarClick}
                disabled={uploadingAvatar}
                sx={{
                  position: 'absolute',
                  bottom: 0,
                  right: 0,
                  bgcolor: palette.brand.primary,
                  color: 'white',
                  '&:hover': { bgcolor: '#A16207' },
                  width: 36,
                  height: 36,
                }}
              >
                {uploadingAvatar ? (
                  <CircularProgress size={20} color="inherit" />
                ) : (
                  <CameraAlt fontSize="small" />
                )}
              </IconButton>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                hidden
                onChange={handleAvatarChange}
              />
            </Box>

            <Typography variant="h5" sx={{ fontWeight: 600, color: palette.neutral[900], mb: 0.5 }}>
              {profile?.fullName || profile?.username}
            </Typography>

            <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
              @{profile?.username}
            </Typography>

            <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center', flexWrap: 'wrap' }}>
              {profile?.emailConfirmed && (
                <Chip
                  icon={<Verified />}
                  label={t('profile.verified')}
                  size="small"
                  sx={{ bgcolor: palette.semantic.infoLight, color: '#1D4ED8' }}
                />
              )}
              {getSellerStatusChip()}
            </Box>

            {!sellerStatus?.isSeller && sellerStatus?.applicationStatus !== 'pending' && (
              <>
                <Divider sx={{ my: 3 }} />
                <Button
                  variant="outlined"
                  startIcon={<Store />}
                  onClick={() => setShowSellerDialog(true)}
                  sx={{
                    borderColor: palette.brand.primary,
                    color: palette.brand.primary,
                    textTransform: 'none',
                    '&:hover': {
                      borderColor: '#A16207',
                      bgcolor: palette.brand.muted,
                    },
                  }}
                >
                  {t('profile.becomeSeller')}
                </Button>
              </>
            )}

            <Divider sx={{ my: 3 }} />

            <Box sx={{ textAlign: 'left' }}>
              <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500], mb: 0.5 }}>
                {t('profile.memberSinceLabel')}
              </Typography>
              <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                {profile?.createdAt
                  ? new Date(profile.createdAt).toLocaleDateString(getCurrentLocale(), {
                      month: 'long',
                      year: 'numeric',
                    })
                  : t('profile.notAvailable')}
              </Typography>
            </Box>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <Card
            sx={{
              p: 4,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            }}
          >
            <Box
              sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                {t('profile.personalInformation')}
              </Typography>
              {!isEditing && (
                <Button
                  onClick={() => {
                    reset({
                      fullName: profile?.fullName || '',
                      phoneNumber: profile?.phoneNumber || '',
                      bio: profile?.bio || '',
                      location: profile?.location || '',
                    })
                    setIsEditing(true)
                  }}
                  sx={{
                    color: palette.brand.primary,
                    textTransform: 'none',
                    fontWeight: 600,
                  }}
                >
                  {t('profile.editProfile')}
                </Button>
              )}
            </Box>

            {updateProfile.isSuccess && (
              <InlineAlert severity="success" sx={{ mb: 3 }}>
                {t('messages.profileUpdated')}
              </InlineAlert>
            )}

            {updateProfile.isError && (
              <InlineAlert severity="error" sx={{ mb: 3 }}>
                {t('profile.updateError')}
              </InlineAlert>
            )}

            <form onSubmit={handleSubmit(onSubmit)}>
              <Stack spacing={2.5}>
                <FormField
                  name="fullName"
                  register={register}
                  errors={errors}
                  fullWidth
                  label={t('profile.fullName')}
                  disabled={!isEditing}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
                <TextField
                  fullWidth
                  label={t('profile.email')}
                  value={profile?.email || ''}
                  disabled
                  slotProps={{ inputLabel: { shrink: true } }}
                  helperText={t('profile.emailChangeHelp')}
                />
                <FormField
                  name="phoneNumber"
                  register={register}
                  errors={errors}
                  fullWidth
                  label={t('profile.phone')}
                  disabled={!isEditing}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
                <FormField
                  name="location"
                  register={register}
                  errors={errors}
                  fullWidth
                  label={t('profile.location')}
                  disabled={!isEditing}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
                <FormField
                  name="bio"
                  register={register}
                  errors={errors}
                  fullWidth
                  label={t('profile.bio')}
                  multiline
                  rows={4}
                  helperText={t('profile.characterCount', {
                    count: profile?.bio?.length || 0,
                    max: 500,
                  })}
                  disabled={!isEditing}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              </Stack>

              {isEditing && (
                <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
                  <Button
                    type="submit"
                    variant="contained"
                    disabled={!isDirty || updateProfile.isPending}
                    sx={{
                      bgcolor: palette.neutral[900],
                      textTransform: 'none',
                      fontWeight: 600,
                      px: 4,
                      '&:hover': { bgcolor: palette.neutral[700] },
                    }}
                  >
                    {updateProfile.isPending ? (
                      <CircularProgress size={20} color="inherit" />
                    ) : (
                      t('profile.saveChanges')
                    )}
                  </Button>
                  <Button
                    variant="outlined"
                    onClick={() => {
                      reset()
                      setIsEditing(false)
                    }}
                    sx={{
                      borderColor: '#D4D4D4',
                      color: palette.neutral[700],
                      textTransform: 'none',
                      '&:hover': { borderColor: palette.neutral[900] },
                    }}
                  >
                    {t('profile.cancel')}
                  </Button>
                </Box>
              )}
            </form>
          </Card>
        </Grid>
      </Grid>

      <Dialog
        open={showSellerDialog}
        onClose={() => setShowSellerDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>{t('profile.becomeSeller')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>{t('profile.sellerDialogDescription')}</Typography>
          <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem' }}>
            {t('profile.sellerTermsIntroduction')}
          </Typography>
          <Box component="ul" sx={{ color: palette.neutral[500], fontSize: '0.875rem', pl: 3 }}>
            <li>{t('profile.sellerTerms.accurateDescriptions')}</li>
            <li>{t('profile.sellerTerms.timelyShipping')}</li>
            <li>{t('profile.sellerTerms.promptResponses')}</li>
            <li>{t('profile.sellerTerms.acceptReturns')}</li>
          </Box>
          {applyForSeller.isError && (
            <InlineAlert severity="error" sx={{ mt: 2 }}>
              {getErrorMessage(applyForSeller.error)}
            </InlineAlert>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShowSellerDialog(false)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('profile.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleApplyForSeller}
            disabled={applyForSeller.isPending}
            startIcon={
              applyForSeller.isPending ? (
                <CircularProgress size={16} color="inherit" />
              ) : (
                <CheckCircle />
              )
            }
            sx={{
              bgcolor: palette.brand.primary,
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {t('profile.applyNow')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
