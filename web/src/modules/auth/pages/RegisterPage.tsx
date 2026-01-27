'use no memo'
import { useState, useEffect, useMemo } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import debounce from 'lodash/debounce'
import { motion } from 'framer-motion'
import { fadeInUp, fadeInRight, staggerContainer, staggerItem } from '@/shared/lib/animations'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Divider,
  Alert,
  IconButton,
  InputAdornment,
  Checkbox,
  FormControlLabel,
  CircularProgress,
  Stack,
  FormHelperText,
} from '@mui/material'
import { Visibility, VisibilityOff, Google, GitHub, CheckCircle } from '@mui/icons-material'
import { registerSchema } from '../schemas'
import { useRegister, useCheckUsername } from '../hooks'

interface RegisterFormData {
  email: string
  username: string
  password: string
  confirmPassword: string
}

export function RegisterPage() {
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [agreedToTerms, setAgreedToTerms] = useState(false)
  const [registrationSuccess, setRegistrationSuccess] = useState(false)
  const [registeredEmail, setRegisteredEmail] = useState('')
  const [debouncedUsername, setDebouncedUsername] = useState('')

  const registerMutation = useRegister()
  const usernameQuery = useCheckUsername(debouncedUsername)

  const [usernameValue, setUsernameValue] = useState('')

  const debouncedSetUsername = useMemo(
    () => debounce((value: string) => {
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
      if (!usernameQuery.data) {
        setError('username', { type: 'manual', message: 'Username is already taken' })
      } else {
        clearErrors('username')
      }
    }
  }, [usernameQuery.data, setError, clearErrors])

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

  const handleSocialLogin = (provider: 'google' | 'github') => {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    window.location.href = `${baseUrl}/api/auth/external-login?provider=${provider}`
  }

  if (registrationSuccess) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          bgcolor: '#FAFAF9',
        }}
      >
        <Container maxWidth="sm">
          <motion.div
            initial="initial"
            animate="animate"
            variants={fadeInUp}
          >
          <Box
            sx={{
              bgcolor: 'white',
              borderRadius: 2,
              p: { xs: 3, sm: 5 },
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              textAlign: 'center',
            }}
          >
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{ type: 'spring', stiffness: 300, damping: 20, delay: 0.2 }}
            >
              <CheckCircle sx={{ fontSize: 64, color: '#22C55E', mb: 2 }} />
            </motion.div>

            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: '#1C1917',
                mb: 1,
              }}
            >
              Check Your Email
            </Typography>

            <Typography sx={{ color: '#78716C', mb: 3 }}>
              We've sent a confirmation link to
            </Typography>

            <Typography
              sx={{
                color: '#1C1917',
                fontWeight: 600,
                fontSize: '1.125rem',
                mb: 4,
              }}
            >
              {registeredEmail}
            </Typography>

            <Typography sx={{ color: '#78716C', fontSize: '0.875rem', mb: 4 }}>
              Click the link in the email to verify your account and complete your registration.
            </Typography>

            <Stack spacing={2}>
              <Button
                fullWidth
                variant="contained"
                onClick={() => navigate('/login')}
                sx={{
                  bgcolor: '#1C1917',
                  py: 1.5,
                  fontWeight: 600,
                  textTransform: 'none',
                  '&:hover': { bgcolor: '#44403C' },
                }}
              >
                Go to Login
              </Button>

              <Typography sx={{ color: '#78716C', fontSize: '0.875rem' }}>
                Didn't receive the email?{' '}
                <Typography
                  component={Link}
                  to="/resend-confirmation"
                  sx={{
                    color: '#CA8A04',
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
        </Container>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        bgcolor: '#FAFAF9',
      }}
    >
      <Box
        sx={{
          display: { xs: 'none', lg: 'flex' },
          width: '50%',
          bgcolor: '#1C1917',
          position: 'relative',
          alignItems: 'center',
          justifyContent: 'center',
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            backgroundImage: 'url(https://images.unsplash.com/photo-1513519245088-0e12902e35a6?w=1200)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            opacity: 0.4,
          }}
        />
        <motion.div
          initial="initial"
          animate="animate"
          variants={fadeInRight}
          style={{ position: 'relative', zIndex: 1, padding: '1.5rem', textAlign: 'center' }}
        >
          <Typography
            variant="h2"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 700,
              color: 'white',
              mb: 2,
            }}
          >
            AUCTION
          </Typography>
          <Typography
            variant="h5"
            sx={{
              color: 'rgba(255,255,255,0.8)',
              fontWeight: 300,
              maxWidth: 400,
            }}
          >
            Join our community of collectors and discover extraordinary pieces
          </Typography>
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
        <Container maxWidth="sm">
          <motion.div
            initial="initial"
            animate="animate"
            variants={staggerContainer}
          >
          <Box sx={{ mb: 3, textAlign: 'center' }}>
            <Typography
              component={Link}
              to="/"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                fontSize: '1.5rem',
                color: '#1C1917',
                textDecoration: 'none',
                display: { lg: 'none' },
              }}
            >
              AUCTION
            </Typography>
          </Box>

          <motion.div variants={staggerItem}>
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
              Create Account
            </Typography>

            <Typography
              sx={{
                color: '#78716C',
                textAlign: 'center',
                mb: 4,
              }}
            >
              Sign up to start bidding on exclusive items
            </Typography>

            {registerMutation.isError && (
              <Alert severity="error" sx={{ mb: 3 }}>
                Registration failed. Please check your information and try again.
              </Alert>
            )}

            <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Google />}
                onClick={() => handleSocialLogin('google')}
                sx={{
                  py: 1.25,
                  borderColor: '#E5E5E5',
                  color: '#44403C',
                  textTransform: 'none',
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: '#1C1917',
                    bgcolor: '#FAFAF9',
                  },
                }}
              >
                Google
              </Button>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<GitHub />}
                onClick={() => handleSocialLogin('github')}
                sx={{
                  py: 1.25,
                  borderColor: '#E5E5E5',
                  color: '#44403C',
                  textTransform: 'none',
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: '#1C1917',
                    bgcolor: '#FAFAF9',
                  },
                }}
              >
                GitHub
              </Button>
            </Stack>

            <Divider sx={{ my: 3 }}>
              <Typography sx={{ color: '#78716C', fontSize: '0.875rem' }}>
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
                    InputProps={{
                      endAdornment: usernameQuery.isLoading ? (
                        <InputAdornment position="end">
                          <CircularProgress size={20} />
                        </InputAdornment>
                      ) : usernameValue && !errors.username && debouncedUsername === usernameValue ? (
                        <InputAdornment position="end">
                          <CheckCircle sx={{ color: '#22C55E' }} />
                        </InputAdornment>
                      ) : null,
                    }}
                    sx={{ mb: 2 }}
                  />
                )}
              />

              <TextField
                fullWidth
                label="Email Address"
                type="email"
                {...register('email')}
                error={!!errors.email}
                helperText={errors.email?.message}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Password"
                type={showPassword ? 'text' : 'password'}
                {...register('password')}
                error={!!errors.password}
                helperText={errors.password?.message}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                        size="small"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Confirm Password"
                type={showConfirmPassword ? 'text' : 'password'}
                {...register('confirmPassword')}
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword?.message}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                        size="small"
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <Box sx={{ mb: 3 }}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={agreedToTerms}
                      onChange={(e) => setAgreedToTerms(e.target.checked)}
                      size="small"
                      sx={{ color: '#78716C' }}
                    />
                  }
                  label={
                    <Typography sx={{ fontSize: '0.875rem', color: '#44403C' }}>
                      I agree to the{' '}
                      <Typography
                        component={Link}
                        to="/terms"
                        sx={{ color: '#CA8A04', textDecoration: 'none' }}
                      >
                        Terms of Service
                      </Typography>{' '}
                      and{' '}
                      <Typography
                        component={Link}
                        to="/privacy"
                        sx={{ color: '#CA8A04', textDecoration: 'none' }}
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
                sx={{
                  bgcolor: '#1C1917',
                  py: 1.5,
                  fontSize: '1rem',
                  fontWeight: 600,
                  textTransform: 'none',
                  '&:hover': { bgcolor: '#44403C' },
                  '&.Mui-disabled': { bgcolor: '#E5E5E5' },
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
                color: '#78716C',
                fontSize: '0.9375rem',
              }}
            >
              Already have an account?{' '}
              <Typography
                component={Link}
                to="/login"
                sx={{
                  color: '#CA8A04',
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
        </Container>
      </Box>
    </Box>
  )
}
