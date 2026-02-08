import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, Stack } from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { Email, West, East } from '@mui/icons-material'
import { z } from 'zod'
import { useForgotPassword } from '../hooks'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const forgotPasswordSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
})

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>

const inputStyles = {
  '& .MuiOutlinedInput-root': {
    backgroundColor: palette.neutral[0],
    borderRadius: 0,
    '& fieldset': {
      borderColor: palette.neutral[300],
    },
    '&:hover fieldset': {
      borderColor: palette.neutral[500],
    },
    '&.Mui-focused fieldset': {
      borderColor: palette.neutral[900],
      borderWidth: 1,
    },
  },
  '& .MuiInputLabel-root': {
    color: palette.neutral[500],
    fontSize: '0.875rem',
  },
  '& .MuiInputLabel-root.Mui-focused': {
    color: palette.neutral[900],
  },
  '& .MuiOutlinedInput-input': {
    color: palette.neutral[900],
    py: 1.75,
  },
  '& .MuiFormHelperText-root.Mui-error': {
    color: palette.semantic.error,
  },
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
          bgcolor: palette.neutral[50],
        }}
      >
        <Box
          sx={{
            display: { xs: 'none', lg: 'block' },
            width: '50%',
            position: 'relative',
            overflow: 'hidden',
          }}
        >
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              backgroundImage: 'url(https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=1200&q=80)',
              backgroundSize: 'cover',
              backgroundPosition: 'center',
            }}
          />
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              bgcolor: 'rgba(28, 25, 23, 0.4)',
            }}
          />
        </Box>

        <Box
          sx={{
            flex: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            px: { xs: 3, md: 6 },
          }}
        >
          <Box sx={{ width: '100%', maxWidth: 440, textAlign: 'center' }}>
            <motion.div initial="initial" animate="animate" variants={fadeInUp}>
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
                    bgcolor: palette.neutral[900],
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 4,
                  }}
                >
                  <Email sx={{ fontSize: 40, color: palette.neutral[0] }} />
                </Box>
              </motion.div>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 2,
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

              <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem', mb: 5 }}>
                If you don't see the email, check your spam folder.
              </Typography>

              <Stack spacing={2}>
                <Button
                  fullWidth
                  variant="contained"
                  component={Link}
                  to="/login"
                  endIcon={<East />}
                  sx={{
                    bgcolor: palette.neutral[900],
                    color: palette.neutral[0],
                    py: 1.75,
                    fontWeight: 500,
                    textTransform: 'uppercase',
                    letterSpacing: '0.1em',
                    fontSize: '0.875rem',
                    borderRadius: 0,
                    boxShadow: 'none',
                    '&:hover': {
                      bgcolor: palette.neutral[800],
                      boxShadow: 'none',
                    },
                  }}
                >
                  Back to Login
                </Button>

                <Button
                  fullWidth
                  variant="text"
                  onClick={() => setEmailSent(false)}
                  sx={{
                    color: palette.neutral[500],
                    textTransform: 'none',
                    '&:hover': {
                      color: palette.neutral[900],
                      bgcolor: 'transparent',
                    },
                  }}
                >
                  Try a different email
                </Button>
              </Stack>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        bgcolor: palette.neutral[50],
      }}
    >
      <Box
        sx={{
          display: { xs: 'none', lg: 'block' },
          width: '50%',
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            backgroundImage: 'url(https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=1200&q=80)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
          }}
        />
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            bgcolor: 'rgba(28, 25, 23, 0.4)',
          }}
        />

        <Box
          sx={{
            position: 'absolute',
            bottom: 60,
            left: 60,
            right: 60,
            color: palette.neutral[0],
          }}
        >
          <motion.div initial="initial" animate="animate" variants={staggerContainer}>
            <motion.div variants={staggerItem}>
              <Typography
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontSize: { lg: '2.5rem', xl: '3rem' },
                  fontWeight: 400,
                  lineHeight: 1.2,
                  mb: 3,
                }}
              >
                Password Recovery
              </Typography>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Typography
                sx={{
                  fontSize: '1rem',
                  color: 'rgba(255,255,255,0.7)',
                  maxWidth: 400,
                  lineHeight: 1.6,
                }}
              >
                No worries, we'll help you regain access to your account securely.
              </Typography>
            </motion.div>
          </motion.div>
        </Box>
      </Box>

      <Box
        sx={{
          flex: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          px: { xs: 3, md: 6 },
        }}
      >
        <Box sx={{ width: '100%', maxWidth: 440 }}>
          <motion.div initial="initial" animate="animate" variants={staggerContainer}>
            <motion.div variants={staggerItem}>
              <Button
                component={Link}
                to="/login"
                startIcon={<West />}
                sx={{
                  color: palette.neutral[500],
                  textTransform: 'none',
                  mb: 4,
                  ml: -1,
                  '&:hover': {
                    bgcolor: 'transparent',
                    color: palette.neutral[900],
                  },
                }}
              >
                Back to login
              </Button>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Typography
                component={Link}
                to="/"
                sx={{
                  display: 'block',
                  fontFamily: '"Playfair Display", serif',
                  fontStyle: 'italic',
                  fontWeight: 500,
                  fontSize: '1.75rem',
                  color: palette.neutral[900],
                  textDecoration: 'none',
                  mb: 6,
                }}
              >
                TheAuction
              </Typography>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Forgot Password?
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                No worries, we'll send you reset instructions
              </Typography>
            </motion.div>

            {forgotPassword.isError && (
              <motion.div variants={staggerItem}>
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  Something went wrong. Please try again.
                </InlineAlert>
              </motion.div>
            )}

            <motion.div variants={staggerItem}>
              <form onSubmit={handleSubmit(onSubmit)}>
                <FormField
                  name="email"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="Email Address"
                  type="email"
                  sx={{ ...inputStyles, mb: 4 }}
                />

                <Button
                  fullWidth
                  type="submit"
                  variant="contained"
                  disabled={isSubmitting || forgotPassword.isPending}
                  endIcon={!isSubmitting && !forgotPassword.isPending && <East />}
                  sx={{
                    bgcolor: palette.neutral[900],
                    color: palette.neutral[0],
                    py: 1.75,
                    fontSize: '0.875rem',
                    fontWeight: 500,
                    textTransform: 'uppercase',
                    letterSpacing: '0.1em',
                    borderRadius: 0,
                    boxShadow: 'none',
                    '&:hover': {
                      bgcolor: palette.neutral[800],
                      boxShadow: 'none',
                    },
                    '&.Mui-disabled': {
                      bgcolor: palette.neutral[300],
                      color: palette.neutral[500],
                    },
                  }}
                >
                  {isSubmitting || forgotPassword.isPending ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    'Reset Password'
                  )}
                </Button>
              </form>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Typography
                sx={{
                  mt: 5,
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
                    color: palette.neutral[900],
                    textDecoration: 'underline',
                    textUnderlineOffset: 3,
                    fontWeight: 500,
                    '&:hover': { color: palette.neutral[600] },
                  }}
                >
                  Sign in
                </Typography>
              </Typography>
            </motion.div>
          </motion.div>
        </Box>
      </Box>
    </Box>
  )
}
