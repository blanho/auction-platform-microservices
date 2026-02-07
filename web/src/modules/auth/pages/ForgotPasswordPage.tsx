import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, alpha } from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { Email, ArrowBack, ArrowForward } from '@mui/icons-material'
import { z } from 'zod'
import { useForgotPassword } from '../hooks'
import { palette, colors, gradients } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const forgotPasswordSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
})

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>

const glassCardStyles = {
  background: 'rgba(255, 255, 255, 0.03)',
  backdropFilter: 'blur(20px)',
  WebkitBackdropFilter: 'blur(20px)',
  border: '1px solid rgba(255, 255, 255, 0.08)',
  borderRadius: 3,
}

const inputStyles = {
  '& .MuiOutlinedInput-root': {
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderRadius: 2,
    '& fieldset': { borderColor: 'rgba(255, 255, 255, 0.15)' },
    '&:hover fieldset': { borderColor: 'rgba(255, 255, 255, 0.3)' },
    '&.Mui-focused fieldset': { borderColor: palette.brand.primary },
  },
  '& .MuiInputLabel-root': { color: 'rgba(255, 255, 255, 0.7)' },
  '& .MuiInputLabel-root.Mui-focused': { color: palette.brand.primary },
  '& .MuiInputLabel-root.MuiInputLabel-shrink': { color: 'rgba(255, 255, 255, 0.7)' },
  '& .MuiOutlinedInput-input': { color: palette.neutral[50] },
  '& .MuiFormHelperText-root': { color: 'rgba(255, 255, 255, 0.5)' },
  '& .MuiFormHelperText-root.Mui-error': { color: palette.semantic.error },
}

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
    defaultValues: { email: '' },
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
          justifyContent: 'center',
          bgcolor: colors.background.primary,
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            top: '20%',
            left: '50%',
            transform: 'translateX(-50%)',
            width: '600px',
            height: '600px',
            background: `radial-gradient(circle, ${alpha(palette.brand.primary, 0.15)} 0%, transparent 70%)`,
            pointerEvents: 'none',
          }}
        />

        <Box sx={{ position: 'relative', width: '100%', maxWidth: 480, px: 3 }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
            <Box sx={{ ...glassCardStyles, p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ type: 'spring', stiffness: 300, damping: 20, delay: 0.2 }}
              >
                <Box
                  sx={{
                    width: 80,
                    height: 80,
                    borderRadius: '50%',
                    background: gradients.gold,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                    boxShadow: `0 0 40px ${alpha(palette.brand.primary, 0.4)}`,
                  }}
                >
                  <Email sx={{ fontSize: 40, color: palette.neutral[900] }} />
                </Box>
              </motion.div>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 600,
                  color: palette.neutral[50],
                  mb: 1,
                }}
              >
                Check Your Email
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 2 }}>
                We've sent password reset instructions to
              </Typography>

              <Typography
                sx={{
                  color: palette.brand.primary,
                  fontWeight: 600,
                  fontSize: '1.125rem',
                  mb: 4,
                }}
              >
                {sentEmail}
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.875rem', mb: 4 }}>
                If you don't see the email, check your spam folder.
              </Typography>

              <Button
                fullWidth
                variant="contained"
                component={Link}
                to="/login"
                endIcon={<ArrowForward />}
                sx={{
                  background: gradients.gold,
                  color: palette.neutral[900],
                  py: 1.5,
                  fontWeight: 600,
                  textTransform: 'none',
                  borderRadius: 2,
                  '&:hover': { background: gradients.goldHover },
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
                  color: 'rgba(255, 255, 255, 0.5)',
                  textTransform: 'none',
                  '&:hover': { color: 'rgba(255, 255, 255, 0.8)', background: 'transparent' },
                }}
              >
                Try a different email
              </Button>
            </Box>
          </motion.div>
        </Box>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: colors.background.primary,
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          top: '-20%',
          left: '-10%',
          width: '600px',
          height: '600px',
          background: `radial-gradient(circle, ${alpha(palette.brand.primary, 0.08)} 0%, transparent 70%)`,
          pointerEvents: 'none',
        }}
      />
      <Box
        sx={{
          position: 'absolute',
          bottom: '-30%',
          right: '-10%',
          width: '800px',
          height: '800px',
          background: `radial-gradient(circle, ${alpha(palette.purple.primary, 0.06)} 0%, transparent 70%)`,
          pointerEvents: 'none',
        }}
      />

      <Box sx={{ position: 'relative', width: '100%', maxWidth: 480, px: 3 }}>
        <motion.div initial="initial" animate="animate" variants={staggerContainer}>
          <motion.div variants={staggerItem}>
            <Box sx={{ mb: 3 }}>
              <Button
                component={Link}
                to="/login"
                startIcon={<ArrowBack />}
                sx={{
                  color: 'rgba(255, 255, 255, 0.6)',
                  textTransform: 'none',
                  '&:hover': { bgcolor: 'transparent', color: palette.neutral[50] },
                }}
              >
                Back to login
              </Button>
            </Box>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box sx={{ ...glassCardStyles, p: { xs: 3, sm: 5 } }}>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 600,
                  color: palette.neutral[50],
                  textAlign: 'center',
                  mb: 1,
                }}
              >
                Forgot Password?
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', textAlign: 'center', mb: 4 }}>
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
                  sx={{ ...inputStyles, mb: 3 }}
                />

                <Button
                  fullWidth
                  type="submit"
                  variant="contained"
                  disabled={isSubmitting || forgotPassword.isPending}
                  endIcon={!isSubmitting && !forgotPassword.isPending && <ArrowForward />}
                  sx={{
                    background: gradients.gold,
                    color: palette.neutral[900],
                    py: 1.5,
                    fontSize: '1rem',
                    fontWeight: 600,
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': { background: gradients.goldHover },
                    '&.Mui-disabled': {
                      background: 'rgba(255, 255, 255, 0.1)',
                      color: 'rgba(255, 255, 255, 0.3)',
                    },
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
                  color: 'rgba(255, 255, 255, 0.5)',
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
          </motion.div>
        </motion.div>
      </Box>
    </Box>
  )
}
