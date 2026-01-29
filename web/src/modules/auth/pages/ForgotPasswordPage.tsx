import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Button,
  CircularProgress,
} from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { Email, ArrowBack } from '@mui/icons-material'
import { z } from 'zod'
import { useForgotPassword } from '../hooks'
import { palette } from '@/shared/theme/tokens'

const forgotPasswordSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
})

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>

export function ForgotPasswordPage() {
  const [emailSent, setEmailSent] = useState(false)
  const [sentEmail, setSentEmail] = useState('')

  const forgotPassword = useForgotPassword()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordForm>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: '',
    },
  })

  const onSubmit = async (data: ForgotPasswordForm) => {
    try {
      await forgotPassword.mutateAsync({ email: data.email })
      setSentEmail(data.email)
      setEmailSent(true)
    } catch {
      /* Error handled by mutation */
    }
  }

  if (emailSent) {
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
            <Box
              sx={{
                width: 64,
                height: 64,
                borderRadius: '50%',
                bgcolor: '#FEF3C7',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mx: 'auto',
                mb: 3,
              }}
            >
              <Email sx={{ fontSize: 32, color: palette.brand.primary }} />
            </Box>

            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: palette.neutral[900],
                mb: 1,
              }}
            >
              Check Your Email
            </Typography>

            <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
              We've sent password reset instructions to
            </Typography>

            <Typography
              sx={{
                color: palette.neutral[900],
                fontWeight: 600,
                fontSize: '1.125rem',
                mb: 4,
              }}
            >
              {sentEmail}
            </Typography>

            <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem', mb: 4 }}>
              If you don't see the email, check your spam folder or make sure you entered the
              correct email address.
            </Typography>

            <Button
              fullWidth
              variant="contained"
              component={Link}
              to="/login"
              sx={{
                bgcolor: palette.neutral[900],
                py: 1.5,
                fontWeight: 600,
                textTransform: 'none',
                '&:hover': { bgcolor: palette.neutral[700] },
              }}
            >
              Back to Login
            </Button>

            <Button
              fullWidth
              variant="text"
              onClick={() => setEmailSent(false)}
              sx={{
                mt: 2,
                color: palette.neutral[500],
                textTransform: 'none',
              }}
            >
              Try a different email
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
              color: palette.neutral[900],
              textAlign: 'center',
              mb: 1,
            }}
          >
            Forgot Password?
          </Typography>

          <Typography
            sx={{
              color: palette.neutral[500],
              textAlign: 'center',
              mb: 4,
            }}
          >
            No worries, we'll send you reset instructions
          </Typography>

          {forgotPassword.isError && (
            <InlineAlert severity="error" sx={{ mb: 3 }}>
              Something went wrong. Please try again.
            </InlineAlert>
          )}

          <form onSubmit={handleSubmit(onSubmit)}>
            <FormField
              name="email"
              register={register}
              errors={errors}
              fullWidth
              label="Email Address"
              type="email"
              sx={{ mb: 3 }}
            />

            <Button
              fullWidth
              type="submit"
              variant="contained"
              disabled={isSubmitting || forgotPassword.isPending}
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
              {isSubmitting || forgotPassword.isPending ? (
                <CircularProgress size={24} color="inherit" />
              ) : (
                'Reset Password'
              )}
            </Button>
          </form>

          <Typography
            sx={{
              mt: 4,
              textAlign: 'center',
              color: palette.neutral[500],
              fontSize: '0.9375rem',
            }}
          >
            Remember your password?{' '}
            <Typography
              component={Link}
              to="/login"
              sx={{
                color: palette.brand.primary,
                textDecoration: 'none',
                fontWeight: 600,
                '&:hover': { textDecoration: 'underline' },
              }}
            >
              Sign in
            </Typography>
          </Typography>
        </Box>
      </Container>
    </Box>
  )
}
