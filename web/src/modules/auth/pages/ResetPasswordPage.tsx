import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, Stack } from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import {
  LockReset,
  West,
  East,
  CheckCircle,
  Cancel,
  Error as ErrorIcon,
} from '@mui/icons-material'
import { z } from 'zod'
import { useResetPassword } from '../hooks'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const resetPasswordSchema = z
  .object({
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Must contain at least one uppercase letter')
      .regex(/[a-z]/, 'Must contain at least one lowercase letter')
      .regex(/\d/, 'Must contain at least one number')
      .regex(/[^A-Za-z0-9]/, 'Must contain at least one special character'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  })

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>

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

interface PasswordRequirement {
  label: string
  test: (password: string) => boolean
}

const passwordRequirements: PasswordRequirement[] = [
  { label: 'At least 8 characters', test: (p) => p.length >= 8 },
  { label: 'One uppercase letter', test: (p) => /[A-Z]/.test(p) },
  { label: 'One lowercase letter', test: (p) => /[a-z]/.test(p) },
  { label: 'One number', test: (p) => /\d/.test(p) },
  { label: 'One special character', test: (p) => /[^A-Za-z0-9]/.test(p) },
]

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [resetSuccess, setResetSuccess] = useState(false)
  const resetPassword = useResetPassword()

  const token = searchParams.get('token')
  const email = searchParams.get('email')

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordForm>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { password: '', confirmPassword: '' },
  })

  const watchPassword = watch('password', '')

  useEffect(() => {
    if (resetSuccess) {
      const timer = setTimeout(() => navigate('/login'), 3000)
      return () => clearTimeout(timer)
    }
  }, [resetSuccess, navigate])

  const onSubmit = async (data: ResetPasswordForm) => {
    if (!token || !email) {
      return
    }
    try {
      await resetPassword.mutateAsync({
        token,
        email,
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
              <Box
                sx={{
                  width: 80,
                  height: 80,
                  borderRadius: '50%',
                  bgcolor: palette.semantic.error,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 4,
                }}
              >
                <ErrorIcon sx={{ fontSize: 40, color: 'white' }} />
              </Box>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 2,
                }}
              >
                Invalid Reset Link
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                This password reset link is invalid or has expired. Please request a new one.
              </Typography>

              <Button
                fullWidth
                variant="contained"
                component={Link}
                to="/forgot-password"
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
                Request New Link
              </Button>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  if (resetSuccess) {
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
                    bgcolor: palette.semantic.success,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 4,
                  }}
                >
                  <CheckCircle sx={{ fontSize: 40, color: 'white' }} />
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
                Password Reset!
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                Your password has been successfully reset. You'll be redirected to login shortly.
              </Typography>

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
                Sign In Now
              </Button>
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
                Create New Password
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
                Choose a strong password to keep your account secure.
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
          py: 6,
          overflow: 'auto',
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
              <Box
                sx={{
                  width: 56,
                  height: 56,
                  borderRadius: '50%',
                  bgcolor: palette.neutral[900],
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mb: 3,
                }}
              >
                <LockReset sx={{ fontSize: 28, color: palette.neutral[0] }} />
              </Box>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Set New Password
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                Create a strong password for your account
              </Typography>
            </motion.div>

            {resetPassword.isError && (
              <motion.div variants={staggerItem}>
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  Something went wrong. Please try again.
                </InlineAlert>
              </motion.div>
            )}

            <motion.div variants={staggerItem}>
              <form onSubmit={handleSubmit(onSubmit)}>
                <FormField
                  name="password"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="New Password"
                  type="password"
                  showPasswordToggle
                  sx={{ ...inputStyles, mb: 2 }}
                />

                <Box
                  sx={{
                    mb: 3,
                    p: 2,
                    border: `1px solid ${palette.neutral[200]}`,
                    bgcolor: palette.neutral[0],
                  }}
                >
                  <Typography
                    sx={{
                      fontSize: '0.75rem',
                      color: palette.neutral[500],
                      mb: 1.5,
                      fontWeight: 500,
                      textTransform: 'uppercase',
                      letterSpacing: '0.05em',
                    }}
                  >
                    Password Requirements
                  </Typography>
                  <Stack spacing={0.75}>
                    {passwordRequirements.map((req) => {
                      const isMet = req.test(watchPassword)
                      return (
                        <Box
                          key={req.label}
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 1,
                          }}
                        >
                          {isMet ? (
                            <CheckCircle sx={{ fontSize: 16, color: palette.semantic.success }} />
                          ) : (
                            <Cancel sx={{ fontSize: 16, color: palette.neutral[300] }} />
                          )}
                          <Typography
                            sx={{
                              fontSize: '0.8125rem',
                              color: isMet ? palette.semantic.success : palette.neutral[500],
                            }}
                          >
                            {req.label}
                          </Typography>
                        </Box>
                      )
                    })}
                  </Stack>
                </Box>

                <FormField
                  name="confirmPassword"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="Confirm Password"
                  type="password"
                  showPasswordToggle
                  sx={{ ...inputStyles, mb: 4 }}
                />

                <Button
                  fullWidth
                  type="submit"
                  variant="contained"
                  disabled={isSubmitting || resetPassword.isPending}
                  endIcon={!isSubmitting && !resetPassword.isPending && <East />}
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
                  {isSubmitting || resetPassword.isPending ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    'Reset Password'
                  )}
                </Button>
              </form>
            </motion.div>
          </motion.div>
        </Box>
      </Box>
    </Box>
  )
}
