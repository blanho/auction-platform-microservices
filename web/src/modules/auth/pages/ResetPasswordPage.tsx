import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, alpha, IconButton, InputAdornment } from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import {
  LockReset,
  ArrowBack,
  ArrowForward,
  Visibility,
  VisibilityOff,
  CheckCircle,
  Cancel,
  Error as ErrorIcon,
} from '@mui/icons-material'
import { z } from 'zod'
import { useResetPassword } from '../hooks'
import { palette, colors, gradients } from '@/shared/theme/tokens'
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
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
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
            background: `radial-gradient(circle, ${alpha(palette.semantic.error, 0.15)} 0%, transparent 70%)`,
            pointerEvents: 'none',
          }}
        />

        <Box sx={{ position: 'relative', width: '100%', maxWidth: 480, px: 3 }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
            <Box sx={{ ...glassCardStyles, p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
              <Box
                sx={{
                  width: 80,
                  height: 80,
                  borderRadius: '50%',
                  background: `linear-gradient(135deg, ${palette.semantic.error} 0%, #B91C1C 100%)`,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                }}
              >
                <ErrorIcon sx={{ fontSize: 40, color: 'white' }} />
              </Box>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 600,
                  color: palette.neutral[50],
                  mb: 1,
                }}
              >
                Invalid Reset Link
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 4 }}>
                This password reset link is invalid or has expired. Please request a new one.
              </Typography>

              <Button
                fullWidth
                variant="contained"
                component={Link}
                to="/forgot-password"
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
                Request New Link
              </Button>
            </Box>
          </motion.div>
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
            background: `radial-gradient(circle, ${alpha(palette.semantic.success, 0.15)} 0%, transparent 70%)`,
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
                    background: `linear-gradient(135deg, ${palette.semantic.success} 0%, #15803D 100%)`,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                    boxShadow: `0 0 40px ${alpha(palette.semantic.success, 0.4)}`,
                  }}
                >
                  <CheckCircle sx={{ fontSize: 40, color: 'white' }} />
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
                Password Reset!
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 4 }}>
                Your password has been successfully reset. You'll be redirected to login shortly.
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
                Sign In Now
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
              <Box
                sx={{
                  width: 64,
                  height: 64,
                  borderRadius: '50%',
                  background: gradients.gold,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                  boxShadow: `0 0 30px ${alpha(palette.brand.primary, 0.3)}`,
                }}
              >
                <LockReset sx={{ fontSize: 32, color: palette.neutral[900] }} />
              </Box>

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
                Set New Password
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', textAlign: 'center', mb: 4 }}>
                Create a strong password for your account
              </Typography>

              {resetPassword.isError && (
                <InlineAlert severity="error" sx={{ mb: 3 }}>
                  Something went wrong. Please try again.
                </InlineAlert>
              )}

              <form onSubmit={handleSubmit(onSubmit)}>
                <FormField
                  name="password"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="New Password"
                  type={showPassword ? 'text' : 'password'}
                  sx={{ ...inputStyles, mb: 2 }}
                  slotProps={{
                    input: {
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowPassword(!showPassword)}
                            edge="end"
                            sx={{ color: 'rgba(255, 255, 255, 0.5)' }}
                          >
                            {showPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    },
                  }}
                />

                <Box sx={{ mb: 3, p: 2, borderRadius: 2, bgcolor: 'rgba(255, 255, 255, 0.03)' }}>
                  <Typography
                    sx={{ fontSize: '0.75rem', color: 'rgba(255, 255, 255, 0.5)', mb: 1.5, fontWeight: 500 }}
                  >
                    Password Requirements
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {passwordRequirements.map((req) => {
                      const isMet = req.test(watchPassword)
                      return (
                        <Box
                          key={req.label}
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 0.5,
                            px: 1,
                            py: 0.5,
                            borderRadius: 1,
                            bgcolor: isMet ? alpha(palette.semantic.success, 0.1) : 'rgba(255, 255, 255, 0.03)',
                            border: `1px solid ${isMet ? alpha(palette.semantic.success, 0.3) : 'rgba(255, 255, 255, 0.08)'}`,
                          }}
                        >
                          {isMet ? (
                            <CheckCircle sx={{ fontSize: 14, color: palette.semantic.success }} />
                          ) : (
                            <Cancel sx={{ fontSize: 14, color: 'rgba(255, 255, 255, 0.3)' }} />
                          )}
                          <Typography
                            sx={{
                              fontSize: '0.6875rem',
                              color: isMet ? palette.semantic.success : 'rgba(255, 255, 255, 0.4)',
                            }}
                          >
                            {req.label}
                          </Typography>
                        </Box>
                      )
                    })}
                  </Box>
                </Box>

                <FormField
                  name="confirmPassword"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="Confirm Password"
                  type={showConfirmPassword ? 'text' : 'password'}
                  sx={{ ...inputStyles, mb: 3 }}
                  slotProps={{
                    input: {
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            edge="end"
                            sx={{ color: 'rgba(255, 255, 255, 0.5)' }}
                          >
                            {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    },
                  }}
                />

                <Button
                  fullWidth
                  type="submit"
                  variant="contained"
                  disabled={isSubmitting || resetPassword.isPending}
                  endIcon={!isSubmitting && !resetPassword.isPending && <ArrowForward />}
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
                  {isSubmitting || resetPassword.isPending ? (
                    <CircularProgress size={24} color="inherit" />
                  ) : (
                    'Reset Password'
                  )}
                </Button>
              </form>
            </Box>
          </motion.div>
        </motion.div>
      </Box>
    </Box>
  )
}
