import { useState, useRef } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Avatar,
  Card,
  Grid,
  Alert,
  CircularProgress,
  Chip,
  Divider,
  IconButton,
  Skeleton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
} from '@mui/material'
import {
  CameraAlt,
  Verified,
  Store,
  CheckCircle,
  Pending,
  Cancel,
} from '@mui/icons-material'
import { useProfile, useUpdateProfile, useSellerStatus, useApplyForSeller } from '../hooks'
import { updateProfileSchema } from '../schemas'
import type { UpdateProfileRequest } from '../types'
import { usersApi } from '../api'

export function ProfilePage() {
  const [isEditing, setIsEditing] = useState(false)
  const [uploadingAvatar, setUploadingAvatar] = useState(false)
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
      displayName: profile?.displayName || '',
      firstName: profile?.firstName || '',
      lastName: profile?.lastName || '',
      phoneNumber: profile?.phoneNumber || '',
      bio: profile?.bio || '',
    },
  })

  const onSubmit = async (data: UpdateProfileRequest) => {
    try {
      await updateProfile.mutateAsync(data)
      setIsEditing(false)
    } catch (err) {
      console.error(err)
    }
  }

  const handleAvatarClick = () => {
    fileInputRef.current?.click()
  }

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    setUploadingAvatar(true)
    try {
      await usersApi.uploadAvatar(file)
    } catch (err) {
      console.error(err)
    } finally {
      setUploadingAvatar(false)
    }
  }

  const handleApplyForSeller = async () => {
    try {
      await applyForSeller.mutateAsync(true)
      setShowSellerDialog(false)
    } catch (err) {
      console.error(err)
    }
  }

  const getSellerStatusChip = () => {
    if (sellerLoading) return null
    if (!sellerStatus) return null

    if (sellerStatus.isSeller) {
      return (
        <Chip
          icon={<Store />}
          label="Verified Seller"
          color="success"
          size="small"
        />
      )
    }

    switch (sellerStatus.applicationStatus) {
      case 'pending':
        return (
          <Chip
            icon={<Pending />}
            label="Seller Application Pending"
            color="warning"
            size="small"
          />
        )
      case 'rejected':
        return (
          <Chip
            icon={<Cancel />}
            label="Application Rejected"
            color="error"
            size="small"
          />
        )
      default:
        return null
    }
  }

  if (profileLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
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
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">Failed to load profile. Please try again.</Alert>
      </Container>
    )
  }

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
          My Profile
        </Typography>
        <Typography sx={{ color: '#78716C' }}>
          Manage your account information
        </Typography>
      </Box>

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
                  bgcolor: '#1C1917',
                }}
              >
                {profile?.displayName?.[0]?.toUpperCase()}
              </Avatar>
              <IconButton
                onClick={handleAvatarClick}
                disabled={uploadingAvatar}
                sx={{
                  position: 'absolute',
                  bottom: 0,
                  right: 0,
                  bgcolor: '#CA8A04',
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

            <Typography variant="h5" sx={{ fontWeight: 600, color: '#1C1917', mb: 0.5 }}>
              {profile?.displayName}
            </Typography>

            <Typography sx={{ color: '#78716C', mb: 2 }}>@{profile?.username}</Typography>

            <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center', flexWrap: 'wrap' }}>
              {profile?.isVerified && (
                <Chip
                  icon={<Verified />}
                  label="Verified"
                  size="small"
                  sx={{ bgcolor: '#DBEAFE', color: '#1D4ED8' }}
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
                    borderColor: '#CA8A04',
                    color: '#CA8A04',
                    textTransform: 'none',
                    '&:hover': {
                      borderColor: '#A16207',
                      bgcolor: '#FEF3C7',
                    },
                  }}
                >
                  Become a Seller
                </Button>
              </>
            )}

            <Divider sx={{ my: 3 }} />

            <Box sx={{ textAlign: 'left' }}>
              <Typography sx={{ fontSize: '0.875rem', color: '#78716C', mb: 0.5 }}>
                Member since
              </Typography>
              <Typography sx={{ fontWeight: 500, color: '#1C1917' }}>
                {profile?.createdAt
                  ? new Date(profile.createdAt).toLocaleDateString('en-US', {
                      month: 'long',
                      year: 'numeric',
                    })
                  : 'N/A'}
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
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 600, color: '#1C1917' }}>
                Personal Information
              </Typography>
              {!isEditing && (
                <Button
                  onClick={() => {
                    reset({
                      displayName: profile?.displayName || '',
                      firstName: profile?.firstName || '',
                      lastName: profile?.lastName || '',
                      phoneNumber: profile?.phoneNumber || '',
                      bio: profile?.bio || '',
                    })
                    setIsEditing(true)
                  }}
                  sx={{
                    color: '#CA8A04',
                    textTransform: 'none',
                    fontWeight: 600,
                  }}
                >
                  Edit Profile
                </Button>
              )}
            </Box>

            {updateProfile.isSuccess && (
              <Alert severity="success" sx={{ mb: 3 }}>
                Profile updated successfully!
              </Alert>
            )}

            {updateProfile.isError && (
              <Alert severity="error" sx={{ mb: 3 }}>
                Failed to update profile. Please try again.
              </Alert>
            )}

            <form onSubmit={handleSubmit(onSubmit)}>
              <Stack spacing={2.5}>
                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2.5}>
                  <TextField
                    fullWidth
                    label="First Name"
                    {...register('firstName')}
                    error={!!errors.firstName}
                    helperText={errors.firstName?.message}
                    disabled={!isEditing}
                    InputLabelProps={{ shrink: true }}
                  />
                  <TextField
                    fullWidth
                    label="Last Name"
                    {...register('lastName')}
                    error={!!errors.lastName}
                    helperText={errors.lastName?.message}
                    disabled={!isEditing}
                    InputLabelProps={{ shrink: true }}
                  />
                </Stack>
                <TextField
                  fullWidth
                  label="Display Name"
                  {...register('displayName')}
                  error={!!errors.displayName}
                  helperText={errors.displayName?.message}
                  disabled={!isEditing}
                  InputLabelProps={{ shrink: true }}
                />
                <TextField
                  fullWidth
                  label="Email"
                  value={profile?.email || ''}
                  disabled
                  InputLabelProps={{ shrink: true }}
                  helperText="Contact support to change your email"
                />
                <TextField
                  fullWidth
                  label="Phone Number"
                  {...register('phoneNumber')}
                  error={!!errors.phoneNumber}
                  helperText={errors.phoneNumber?.message}
                  disabled={!isEditing}
                  InputLabelProps={{ shrink: true }}
                />
                <TextField
                  fullWidth
                  label="Bio"
                  multiline
                  rows={4}
                  {...register('bio')}
                  error={!!errors.bio}
                  helperText={errors.bio?.message || `${profile?.bio?.length || 0}/500 characters`}
                  disabled={!isEditing}
                  InputLabelProps={{ shrink: true }}
                />
              </Stack>

              {isEditing && (
                <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
                  <Button
                    type="submit"
                    variant="contained"
                    disabled={!isDirty || updateProfile.isPending}
                    sx={{
                      bgcolor: '#1C1917',
                      textTransform: 'none',
                      fontWeight: 600,
                      px: 4,
                      '&:hover': { bgcolor: '#44403C' },
                    }}
                  >
                    {updateProfile.isPending ? (
                      <CircularProgress size={20} color="inherit" />
                    ) : (
                      'Save Changes'
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
                      color: '#44403C',
                      textTransform: 'none',
                      '&:hover': { borderColor: '#1C1917' },
                    }}
                  >
                    Cancel
                  </Button>
                </Box>
              )}
            </form>
          </Card>
        </Grid>
      </Grid>

      <Dialog open={showSellerDialog} onClose={() => setShowSellerDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Become a Seller</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>
            As a seller, you can list your own items for auction and reach thousands of collectors.
          </Typography>
          <Typography sx={{ color: '#78716C', fontSize: '0.875rem' }}>
            By applying, you agree to our seller terms and conditions, including:
          </Typography>
          <Box component="ul" sx={{ color: '#78716C', fontSize: '0.875rem', pl: 3 }}>
            <li>Maintain accurate item descriptions</li>
            <li>Ship items within the specified timeframe</li>
            <li>Respond to buyer inquiries promptly</li>
            <li>Accept returns for items not as described</li>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShowSellerDialog(false)}
            sx={{ color: '#78716C', textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleApplyForSeller}
            disabled={applyForSeller.isPending}
            startIcon={applyForSeller.isPending ? <CircularProgress size={16} color="inherit" /> : <CheckCircle />}
            sx={{
              bgcolor: '#CA8A04',
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            Apply Now
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
