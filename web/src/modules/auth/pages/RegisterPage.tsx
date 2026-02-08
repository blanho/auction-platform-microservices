'use no memo'
import { useState, useEffect, useMemo } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import debounce from 'lodash/debounce'
import { motion } from 'framer-motion'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import {
  Box,
  Typography,
  TextField,
  Button,
  Divider,
  InputAdornment,
  Checkbox,
  FormControlLabel,
  CircularProgress,
  Stack,
  FormHelperText,
  alpha,
} from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { Google, CheckCircle, ArrowForward, Gavel } from '@mui/icons-material'
import { registerSchema } from '../schemas'
import { useRegister, useCheckUsername } from '../hooks'
import { palette, colors, gradients } from '@/shared/theme/tokens'
import { getErrorMessage } from '@/services/http'

interface RegisterFormData {
  email: string
  username: string
  password: string
  confirmPassword: string
}

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
    '& fieldset': {
      borderColor: 'rgba(255, 255, 255, 0.15)',
    },
    '&:hover fieldset': {
      borderColor: 'rgba(255, 255, 255, 0.3)',
    },
    '&.Mui-focused fieldset': {
      borderColor: palette.brand.primary,
    },
  },
  '& .MuiInputLabel-root': {
    color: 'rgba(255, 255, 255, 0.7)',
  },
  '& .MuiInputLabel-root.Mui-focused': {
    color: palette.brand.primary,
  },
  '& .MuiInputLabel-root.MuiInputLabel-shrink': {
    color: 'rgba(255, 255, 255, 0.7)',
  },
  '& .MuiOutlinedInput-input': {
    color: palette.neutral[50],
  },
  '& .MuiFormHelperText-root': {
    color: 'rgba(255, 255, 255, 0.5)',
  },
  '& .MuiFormHelperText-root.Mui-error': {
    color: palette.semantic.error,
  },
  '& .MuiIconButton-root': {
    color: 'rgba(255, 255, 255, 0.5)',
  },
}

export function RegisterPage() {
  const navigate = useNavigate()
  const [agreedToTerms, setAgreedToTerms] = useState(false)
  const [registrationSuccess, setRegistrationSuccess] = useState(false)
  const [registeredEmail, setRegisteredEmail] = useState('')
  const [debouncedUsername, setDebouncedUsername] = useState('')

  const registerMutation = useRegister()
  const usernameQuery = useCheckUsername(debouncedUsername)

  const [usernameValue, setUsernameValue] = useState('')

  const debouncedSetUsername = useMemo(
    () =>
      debounce((value: string) => {
        if (value.length >= 3) {
          setDebouncedUsername(value)
        }
      }, 500),
    []
  )

  useEffect(() => {
    return () => {
      debouncedSetUsername.cancel()
    }
  }, [debouncedSetUsername])

  const {
    register,
    handleSubmit,
    control,
    setError,
    clearErrors,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      username: '',
      password: '',
      confirmPassword: '',
    },
  })

  useEffect(() => {
    debouncedSetUsername(usernameValue)
  }, [usernameValue, debouncedSetUsername])

  useEffect(() => {
    if (usernameQuery.data !== undefined) {
      if (usernameQuery.data === false) {
        setError('username', { type: 'manual', message: 'Username is already taken' })
      } else {
        clearErrors('username')
      }
    }
  }, [usernameQuery.data, setError, clearErrors])

  const usernameEndAdornment = useMemo(() => {
    if (usernameQuery.isLoading) {
      return (
        <InputAdornment position="end">
          <CircularProgress size={20} />
        </InputAdornment>
      )
    }
    if (usernameValue && !errors.username && debouncedUsername === usernameValue) {
      return (
        <InputAdornment position="end">
          <CheckCircle sx={{ color: palette.semantic.success }} />
        </InputAdornment>
      )
    }
    return null
  }, [usernameQuery.isLoading, usernameValue, errors.username, debouncedUsername])

  const onSubmit = async (data: RegisterFormData) => {
    if (!agreedToTerms) {
      return
    }

    try {
      await registerMutation.mutateAsync(data)
      setRegisteredEmail(data.email)
      setRegistrationSuccess(true)
    } catch {
      /* Error handled by mutation */
    }
  }

  const handleGoogleLogin = () => {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const returnUrl = encodeURIComponent(globalThis.location.origin + '/auth/callback')
    globalThis.location.href = `${baseUrl}/api/auth/external-login/Google?returnUrl=${returnUrl}`
  }

  if (registrationSuccess) {
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
            <Box
              sx={{
                ...glassCardStyles,
                p: { xs: 3, sm: 5 },
                textAlign: 'center',
              }}
            >
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
                    background: `linear-gradient(135deg, ${palette.semantic.success} 0%, ${palette.semantic.successHover} 100%)`,
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
                Check Your Email
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 3 }}>
                We've sent a confirmation link to
              </Typography>

              <Typography
                sx={{
                  color: palette.brand.primary,
                  fontWeight: 600,
                  fontSize: '1.125rem',
                  mb: 4,
                }}
              >
                {registeredEmail}
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.875rem', mb: 4 }}>
                Click the link in the email to verify your account and complete your registration.
              </Typography>

              <Stack spacing={2}>
                <Button
                  fullWidth
                  variant="contained"
                  onClick={() => navigate('/login')}
                  endIcon={<ArrowForward />}
                  sx={{
                    background: gradients.gold,
                    color: palette.neutral[900],
                    py: 1.5,
                    fontWeight: 600,
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': {
                      background: gradients.goldHover,
                    },
                  }}
                >
                  Go to Login
                </Button>

                <Typography sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.875rem' }}>
                  Didn't receive the email?{' '}
                  <Typography
                    component={Link}
                    to="/resend-confirmation"
                    sx={{
                      color: palette.brand.primary,
                      textDecoration: 'none',
                      fontWeight: 600,
                      '&:hover': { textDecoration: 'underline' },
                    }}
                  >
                    Resend confirmation
                  </Typography>
                </Typography>
              </Stack>
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
        bgcolor: colors.background.primary,
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          top: '-20%',
          right: '-10%',
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
          left: '-10%',
          width: '800px',
          height: '800px',
          background: `radial-gradient(circle, ${alpha(palette.purple.primary, 0.06)} 0%, transparent 70%)`,
          pointerEvents: 'none',
        }}
      />

      <Box
        sx={{
          display: { xs: 'none', lg: 'flex' },
          width: '50%',
          position: 'relative',
          alignItems: 'center',
          justifyContent: 'center',
          overflow: 'hidden',
          borderRight: '1px solid rgba(255, 255, 255, 0.05)',
        }}
      >
        <motion.div
          initial="initial"
          animate="animate"
          variants={staggerContainer}
          style={{ position: 'relative', zIndex: 1, padding: '3rem', textAlign: 'center', width: '100%', maxWidth: 520 }}
        >
          <motion.div variants={staggerItem}>
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
                mb: 4,
                boxShadow: `0 0 60px ${alpha(palette.brand.primary, 0.4)}`,
              }}
            >
              <Gavel sx={{ fontSize: 40, color: palette.neutral[900] }} />
            </Box>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Typography
              variant="h2"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                background: gradients.gold,
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                mb: 2,
              }}
            >
              AUCTION
            </Typography>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Typography
              variant="h5"
              sx={{
                color: 'rgba(255,255,255,0.6)',
                fontWeight: 300,
                lineHeight: 1.6,
              }}
            >
              Join our community of collectors and discover extraordinary pieces
            </Typography>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Stack spacing={2} sx={{ mt: 6, maxWidth: 320, mx: 'auto' }}>
              {[
                'Access exclusive auctions',
                'Verified sellers & items',
                'Secure payment protection',
              ].map((feature) => (
                <Box key={feature} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <CheckCircle sx={{ color: palette.brand.primary, fontSize: 20 }} />
                  <Typography sx={{ color: 'rgba(255,255,255,0.6)', fontSize: '0.9375rem' }}>
                    {feature}
                  </Typography>
                </Box>
              ))}
            </Stack>
          </motion.div>
        </motion.div>
      </Box>

      <Box
        sx={{
          flex: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          p: { xs: 2, sm: 4 },
          overflow: 'auto',
        }}
      >
        <Box sx={{ width: '100%', maxWidth: 480, px: { xs: 1, sm: 0 } }}>
          <motion.div initial="initial" animate="animate" variants={staggerContainer}>
            <motion.div variants={staggerItem}>
              <Box sx={{ mb: 3, textAlign: 'center' }}>
                <Typography
                  component={Link}
                  to="/"
                  sx={{
                    fontFamily: '"Playfair Display", serif',
                    fontWeight: 700,
                    fontSize: '1.5rem',
                    background: gradients.gold,
                    backgroundClip: 'text',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    textDecoration: 'none',
                    display: { lg: 'none' },
                  }}
                >
                  AUCTION
                </Typography>
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
                  Create Account
                </Typography>

                <Typography
                  sx={{
                    color: 'rgba(255, 255, 255, 0.5)',
                    textAlign: 'center',
                    mb: 4,
                  }}
                >
                  Sign up to start bidding on exclusive items
                </Typography>

                {registerMutation.isError && (
                  <InlineAlert severity="error" sx={{ mb: 3 }}>
                    {getErrorMessage(registerMutation.error)}
                  </InlineAlert>
                )}

                <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
                  <Button
                    fullWidth
                    variant="outlined"
                    startIcon={<Google />}
                    onClick={handleGoogleLogin}
                    sx={{
                      py: 1.25,
                      borderColor: 'rgba(255, 255, 255, 0.1)',
                      color: 'rgba(255, 255, 255, 0.7)',
                      textTransform: 'none',
                      fontWeight: 500,
                      borderRadius: 2,
                      '&:hover': {
                        borderColor: 'rgba(255, 255, 255, 0.3)',
                        bgcolor: 'rgba(255, 255, 255, 0.05)',
                      },
                    }}
                  >
                    Continue with Google
                  </Button>
                </Stack>

                <Divider
                  sx={{
                    my: 3,
                    '&::before, &::after': {
                      borderColor: 'rgba(255, 255, 255, 0.1)',
                    },
                  }}
                >
                  <Typography sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.875rem' }}>
                    or register with email
                  </Typography>
                </Divider>

                <form onSubmit={handleSubmit(onSubmit)}>
                  <Controller
                    name="username"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        fullWidth
                        label="Username"
                        onChange={(e) => {
                          field.onChange(e)
                          setUsernameValue(e.target.value)
                        }}
                        error={!!errors.username}
                        helperText={errors.username?.message}
                        slotProps={{
                          input: {
                            endAdornment: usernameEndAdornment,
                          },
                        }}
                        sx={{ ...inputStyles, mb: 2 }}
                      />
                    )}
                  />

                  <FormField
                    name="email"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="Email Address"
                    type="email"
                    sx={{ ...inputStyles, mb: 2 }}
                  />

                  <FormField
                    name="password"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="Password"
                    type="password"
                    showPasswordToggle
                    sx={{ ...inputStyles, mb: 2 }}
                  />

                  <FormField
                    name="confirmPassword"
                    register={register}
                    errors={errors}
                    fullWidth
                    label="Confirm Password"
                    type="password"
                    showPasswordToggle
                    sx={{ ...inputStyles, mb: 2 }}
                  />

                  <Box sx={{ mb: 3 }}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={agreedToTerms}
                          onChange={(e) => setAgreedToTerms(e.target.checked)}
                          size="small"
                          sx={{
                            color: 'rgba(255, 255, 255, 0.3)',
                            '&.Mui-checked': { color: palette.brand.primary },
                          }}
                        />
                      }
                      label={
                        <Typography sx={{ fontSize: '0.875rem', color: 'rgba(255, 255, 255, 0.6)' }}>
                          I agree to the{' '}
                          <Typography
                            component={Link}
                            to="/terms"
                            sx={{ color: palette.brand.primary, textDecoration: 'none' }}
                          >
                            Terms of Service
                          </Typography>{' '}
                          and{' '}
                          <Typography
                            component={Link}
                            to="/privacy"
                            sx={{ color: palette.brand.primary, textDecoration: 'none' }}
                          >
                            Privacy Policy
                          </Typography>
                        </Typography>
                      }
                    />
                    {!agreedToTerms && isSubmitting && (
                      <FormHelperText error>
                        You must agree to the terms and conditions
                      </FormHelperText>
                    )}
                  </Box>

                  <Button
                    fullWidth
                    type="submit"
                    variant="contained"
                    disabled={isSubmitting || registerMutation.isPending || !agreedToTerms}
                    endIcon={!isSubmitting && !registerMutation.isPending && <ArrowForward />}
                    sx={{
                      background: gradients.gold,
                      color: palette.neutral[900],
                      py: 1.5,
                      fontSize: '1rem',
                      fontWeight: 600,
                      textTransform: 'none',
                      borderRadius: 2,
                      '&:hover': {
                        background: gradients.goldHover,
                      },
                      '&.Mui-disabled': {
                        background: 'rgba(255, 255, 255, 0.1)',
                        color: 'rgba(255, 255, 255, 0.3)',
                      },
                    }}
                  >
                    {isSubmitting || registerMutation.isPending ? (
                      <CircularProgress size={24} color="inherit" />
                    ) : (
                      'Create Account'
                    )}
                  </Button>
                </form>

                <Typography
                  sx={{
                    mt: 3,
                    textAlign: 'center',
                    color: 'rgba(255, 255, 255, 0.5)',
                    fontSize: '0.9375rem',
                  }}
                >
                  Already have an account?{' '}
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
    </Box>
  )
}
