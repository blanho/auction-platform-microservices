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
} from '@mui/material'
import { InlineAlert, FormField } from '@/shared/ui'
import { Google, CheckCircle, East } from '@mui/icons-material'
import { registerSchema } from '../schemas'
import { useRegister, useCheckUsername } from '../hooks'
import { palette } from '@/shared/theme/tokens'
import { getErrorMessage } from '@/services/http'

interface RegisterFormData {
  email: string
  username: string
  password: string
  confirmPassword: string
}

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
          <CircularProgress size={20} sx={{ color: palette.neutral[500] }} />
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
              backgroundImage: 'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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
                Check Your Email
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
                We've sent a confirmation link to
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[900],
                  fontWeight: 600,
                  fontSize: '1.125rem',
                  mb: 4,
                }}
              >
                {registeredEmail}
              </Typography>

              <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem', mb: 5 }}>
                Click the link in the email to verify your account and complete your registration.
              </Typography>

              <Stack spacing={2}>
                <Button
                  fullWidth
                  variant="contained"
                  onClick={() => navigate('/login')}
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
                  Go to Login
                </Button>

                <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem' }}>
                  Didn't receive the email?{' '}
                  <Typography
                    component={Link}
                    to="/resend-confirmation"
                    sx={{
                      color: palette.neutral[900],
                      textDecoration: 'underline',
                      textUnderlineOffset: 3,
                      fontWeight: 500,
                      '&:hover': { color: palette.neutral[600] },
                    }}
                  >
                    Resend confirmation
                  </Typography>
                </Typography>
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
            backgroundImage: 'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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
          <motion.div
            initial="initial"
            animate="animate"
            variants={staggerContainer}
          >
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
                Start Your
                <br />
                Collection Today
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
                Join our community of collectors and discover extraordinary pieces from around the world.
              </Typography>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Stack spacing={2} sx={{ mt: 5 }}>
                {[
                  'Access exclusive auctions',
                  'Verified sellers & items',
                  'Secure payment protection',
                ].map((feature) => (
                  <Box key={feature} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <CheckCircle sx={{ color: palette.neutral[0], fontSize: 18 }} />
                    <Typography sx={{ color: 'rgba(255,255,255,0.8)', fontSize: '0.9375rem' }}>
                      {feature}
                    </Typography>
                  </Box>
                ))}
              </Stack>
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
                Create Account
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                Join our community of collectors
              </Typography>
            </motion.div>

            {registerMutation.isError && (
              <motion.div variants={staggerItem}>
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  {getErrorMessage(registerMutation.error)}
                </InlineAlert>
              </motion.div>
            )}

            <motion.div variants={staggerItem}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Google />}
                onClick={handleGoogleLogin}
                sx={{
                  py: 1.5,
                  borderColor: palette.neutral[300],
                  color: palette.neutral[900],
                  textTransform: 'none',
                  fontWeight: 500,
                  borderRadius: 0,
                  mb: 3,
                  '&:hover': {
                    borderColor: palette.neutral[900],
                    bgcolor: 'transparent',
                  },
                }}
              >
                Continue with Google
              </Button>

              <Divider
                sx={{
                  my: 3,
                  '&::before, &::after': {
                    borderColor: palette.neutral[200],
                  },
                }}
              >
                <Typography sx={{ color: palette.neutral[400], fontSize: '0.8125rem' }}>
                  or register with email
                </Typography>
              </Divider>
            </motion.div>

            <motion.div variants={staggerItem}>
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
                  sx={{ ...inputStyles, mb: 3 }}
                />

                <Box sx={{ mb: 4 }}>
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={agreedToTerms}
                        onChange={(e) => setAgreedToTerms(e.target.checked)}
                        size="small"
                        sx={{
                          color: palette.neutral[400],
                          '&.Mui-checked': { color: palette.neutral[900] },
                        }}
                      />
                    }
                    label={
                      <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[600] }}>
                        I agree to the{' '}
                        <Typography
                          component={Link}
                          to="/terms"
                          sx={{
                            color: palette.neutral[900],
                            textDecoration: 'underline',
                            textUnderlineOffset: 2,
                          }}
                        >
                          Terms of Service
                        </Typography>{' '}
                        and{' '}
                        <Typography
                          component={Link}
                          to="/privacy"
                          sx={{
                            color: palette.neutral[900],
                            textDecoration: 'underline',
                            textUnderlineOffset: 2,
                          }}
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
                  endIcon={!isSubmitting && !registerMutation.isPending && <East />}
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
                  {isSubmitting || registerMutation.isPending ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    'Create Account'
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
                Already have an account?{' '}
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
