import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Button,
  CircularProgress,
} from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { CheckCircle, ArrowBack } from '@mui/icons-material'
import { z } from 'zod'
import { useResetPassword } from '../hooks'
import { palette } from '@/shared/theme/tokens'

const resetPasswordSchema = z
  .object({
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
      .regex(/[0-9]/, 'Password must contain at least one number'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const token = searchParams.get('token')
  const email = searchParams.get('email')

  const [resetSuccess, setResetSuccess] = useState(false)

  const resetPassword = useResetPassword()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordForm>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      password: '',
      confirmPassword: '',
    },
  })

  const onSubmit = async (data: ResetPasswordForm) => {
    if (!token || !email) {return}

    try {
      await resetPassword.mutateAsync({
        email,
        token,
        newPassword: data.password,
        confirmPassword: data.confirmPassword,
      })
      setResetSuccess(true)
    } catch {
      /* Error handled by mutation */
    }
  }

  if (!token || !email) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          bgcolor: palette.neutral[50],
        }}
      >
        <Container maxWidth="sm">
          <Box
            sx={{
              bgcolor: 'white',
              borderRadius: 2,
              p: { xs: 3, sm: 5 },
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              textAlign: 'center',
            }}
          >
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: palette.neutral[900],
                mb: 2,
              }}
            >
              Invalid Link
            </Typography>

            <Typography sx={{ color: palette.neutral[500], mb: 4 }}>
              This password reset link is invalid or has expired. Please request a new one.
            </Typography>

            <Button
              fullWidth
              variant="contained"
              component={Link}
              to="/forgot-password"
              sx={{
                bgcolor: palette.neutral[900],
                py: 1.5,
                fontWeight: 600,
                textTransform: 'none',
                '&:hover': { bgcolor: palette.neutral[700] },
              }}
            >
              Request New Link
            </Button>
          </Box>
        </Container>
      </Box>
    )
  }

  if (resetSuccess) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          bgcolor: palette.neutral[50],
        }}
      >
        <Container maxWidth="sm">
          <Box
            sx={{
              bgcolor: 'white',
              borderRadius: 2,
              p: { xs: 3, sm: 5 },
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              textAlign: 'center',
            }}
          >
            <CheckCircle sx={{ fontSize: 64, color: palette.semantic.success, mb: 2 }} />

            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: palette.neutral[900],
                mb: 1,
              }}
            >
              Password Reset Successfully
            </Typography>

            <Typography sx={{ color: palette.neutral[500], mb: 4 }}>
              Your password has been reset. You can now sign in with your new password.
            </Typography>

            <Button
              fullWidth
              variant="contained"
              onClick={() => navigate('/login')}
              sx={{
                bgcolor: palette.neutral[900],
                py: 1.5,
                fontWeight: 600,
                textTransform: 'none',
                '&:hover': { bgcolor: palette.neutral[700] },
              }}
            >
              Sign In
            </Button>
          </Box>
        </Container>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        bgcolor: palette.neutral[50],
      }}
    >
      <Container maxWidth="sm">
        <Box sx={{ mb: 3 }}>
          <Button
            component={Link}
            to="/login"
            startIcon={<ArrowBack />}
            sx={{
              color: palette.neutral[700],
              textTransform: 'none',
              '&:hover': { bgcolor: 'transparent', color: palette.neutral[900] },
            }}
          >
            Back to login
          </Button>
        </Box>

        <Box
          sx={{
            bgcolor: 'white',
            borderRadius: 2,
            p: { xs: 3, sm: 5 },
            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          }}
        >
          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 600,
              color: '#1C1917',
              textAlign: 'center',
              mb: 1,
            }}
          >
            Create New Password
          </Typography>

          <Typography
            sx={{
              color: '#78716C',
              textAlign: 'center',
              mb: 4,
            }}
          >
            Your new password must be different from previously used passwords
          </Typography>

          {resetPassword.isError && (
            <InlineAlert severity="error" sx={{ mb: 3 }}>
              Failed to reset password. The link may have expired.
            </InlineAlert>
          )}

          <form onSubmit={handleSubmit(onSubmit)}>
            <FormField
              name="password"
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

            <Box sx={{ mb: 3 }}>
              <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500], mb: 1 }}>
                Password requirements:
              </Typography>
              <Typography sx={{ fontSize: '0.8125rem', color: '#A1A1AA' }}>
                • At least 8 characters
              </Typography>
              <Typography sx={{ fontSize: '0.8125rem', color: '#A1A1AA' }}>
                • One uppercase letter
              </Typography>
              <Typography sx={{ fontSize: '0.8125rem', color: '#A1A1AA' }}>
                • One lowercase letter
              </Typography>
              <Typography sx={{ fontSize: '0.8125rem', color: '#A1A1AA' }}>• One number</Typography>
            </Box>

            <Button
              fullWidth
              type="submit"
              variant="contained"
              disabled={isSubmitting || resetPassword.isPending}
              sx={{
                bgcolor: palette.neutral[900],
                py: 1.5,
                fontSize: '1rem',
                fontWeight: 600,
                textTransform: 'none',
                '&:hover': { bgcolor: palette.neutral[700] },
                '&.Mui-disabled': { bgcolor: '#E5E5E5' },
              }}
            >
              {isSubmitting || resetPassword.isPending ? (
                <CircularProgress size={24} color="inherit" />
              ) : (
                'Reset Password'
              )}
            </Button>
          </form>
        </Box>
      </Container>
    </Box>
  )
}
