import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate, useLocation, useSearchParams } from 'react-router-dom'
import { motion } from 'framer-motion'
import { fadeInUp, fadeInLeft, staggerContainer, staggerItem } from '@/shared/lib/animations'
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
} from '@mui/material'
import { Visibility, VisibilityOff, Google, GitHub, Warning } from '@mui/icons-material'
import { loginSchema } from '../schemas'
import { useLogin, useLoginWith2FA } from '../hooks'
import type { LoginRequest } from '../types'

const SESSION_MESSAGES: Record<string, { severity: 'warning' | 'error' | 'info'; message: string }> = {
  expired: {
    severity: 'warning',
    message: 'Your session has expired. Please log in again.',
  },
  security: {
    severity: 'error',
    message: 'Your session was terminated due to suspicious activity. All devices have been logged out for your security. Please log in again and consider changing your password.',
  },
  logout: {
    severity: 'info',
    message: 'You have been logged out successfully.',
  },
}

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/'

  const sessionParam = searchParams.get('session')
  const initialSessionMessage = sessionParam && SESSION_MESSAGES[sessionParam] 
    ? SESSION_MESSAGES[sessionParam] 
    : null

  const [showPassword, setShowPassword] = useState(false)
  const [rememberMe, setRememberMe] = useState(false)
  const [requires2FA, setRequires2FA] = useState(false)
  const [twoFactorToken, setTwoFactorToken] = useState('')
  const [twoFactorCode, setTwoFactorCode] = useState('')
  const [sessionMessage, setSessionMessage] = useState<{ severity: 'warning' | 'error' | 'info'; message: string } | null>(initialSessionMessage)

  useEffect(() => {
    if (sessionParam && SESSION_MESSAGES[sessionParam]) {
      searchParams.delete('session')
      setSearchParams(searchParams, { replace: true })
    }
  }, [sessionParam, searchParams, setSearchParams])

  const login = useLogin()
  const login2FA = useLoginWith2FA()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginRequest>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      usernameOrEmail: '',
      password: '',
    },
  })

  const onSubmit = async (data: LoginRequest) => {
    try {
      const response = await login.mutateAsync(data)

      if (response.requiresTwoFactor && response.twoFactorStateToken) {
        setRequires2FA(true)
        setTwoFactorToken(response.twoFactorStateToken)
      } else {
        navigate(from, { replace: true })
      }
    } catch {
      /* Error handled by mutation */
    }
  }

  const handle2FASubmit = async () => {
    try {
      await login2FA.mutateAsync({
        twoFactorStateToken: twoFactorToken,
        code: twoFactorCode,
      })
      navigate(from, { replace: true })
    } catch {
      /* Error handled by mutation */
    }
  }

  const handleSocialLogin = (provider: 'google' | 'github') => {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    window.location.href = `${baseUrl}/api/auth/external-login?provider=${provider}`
  }

  if (requires2FA) {
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
              Two-Factor Authentication
            </Typography>

            <Typography
              sx={{
                color: '#78716C',
                textAlign: 'center',
                mb: 4,
              }}
            >
              Enter the code from your authenticator app
            </Typography>

            {login2FA.isError && (
              <Alert severity="error" sx={{ mb: 3 }}>
                Invalid verification code. Please try again.
              </Alert>
            )}

            <TextField
              fullWidth
              label="Verification Code"
              value={twoFactorCode}
              onChange={(e) => setTwoFactorCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              placeholder="000000"
              inputProps={{
                maxLength: 6,
                style: { textAlign: 'center', letterSpacing: '0.5em', fontSize: '1.5rem' },
              }}
              sx={{ mb: 3 }}
            />

            <Button
              fullWidth
              variant="contained"
              onClick={handle2FASubmit}
              disabled={twoFactorCode.length !== 6 || login2FA.isPending}
              sx={{
                bgcolor: '#1C1917',
                py: 1.5,
                fontSize: '1rem',
                fontWeight: 600,
                textTransform: 'none',
                '&:hover': { bgcolor: '#44403C' },
              }}
            >
              {login2FA.isPending ? <CircularProgress size={24} color="inherit" /> : 'Verify'}
            </Button>

            <Button
              fullWidth
              variant="text"
              onClick={() => setRequires2FA(false)}
              sx={{
                mt: 2,
                color: '#78716C',
                textTransform: 'none',
              }}
            >
              Use a different account
            </Button>
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
            backgroundImage: 'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            opacity: 0.4,
          }}
        />
        <motion.div
          initial="initial"
          animate="animate"
          variants={fadeInLeft}
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
            Discover unique treasures and rare finds from collectors worldwide
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
        }}
      >
        <Container maxWidth="sm">
          <motion.div
            initial="initial"
            animate="animate"
            variants={staggerContainer}
          >
          <Box sx={{ mb: 4, textAlign: 'center' }}>
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
              Welcome Back
            </Typography>

            <Typography
              sx={{
                color: '#78716C',
                textAlign: 'center',
                mb: 4,
              }}
            >
              Sign in to continue to your account
            </Typography>

            {sessionMessage && (
              <Alert 
                severity={sessionMessage.severity} 
                icon={sessionMessage.severity === 'error' ? <Warning /> : undefined}
                onClose={() => setSessionMessage(null)}
                sx={{ mb: 3 }}
              >
                {sessionMessage.message}
              </Alert>
            )}

            {login.isError && (
              <Alert severity="error" sx={{ mb: 3 }}>
                Invalid email or password. Please try again.
              </Alert>
            )}

            <form onSubmit={handleSubmit(onSubmit)}>
              <TextField
                fullWidth
                label="Email or Username"
                {...register('usernameOrEmail')}
                error={!!errors.usernameOrEmail}
                helperText={errors.usernameOrEmail?.message}
                sx={{ mb: 2.5 }}
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

              <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
                sx={{ mb: 3 }}
              >
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={rememberMe}
                      onChange={(e) => setRememberMe(e.target.checked)}
                      size="small"
                      sx={{ color: '#78716C' }}
                    />
                  }
                  label={
                    <Typography sx={{ fontSize: '0.875rem', color: '#44403C' }}>
                      Remember me
                    </Typography>
                  }
                />
                <Typography
                  component={Link}
                  to="/forgot-password"
                  sx={{
                    fontSize: '0.875rem',
                    color: '#CA8A04',
                    textDecoration: 'none',
                    fontWeight: 500,
                    '&:hover': { textDecoration: 'underline' },
                  }}
                >
                  Forgot password?
                </Typography>
              </Stack>

              <Button
                fullWidth
                type="submit"
                variant="contained"
                disabled={isSubmitting || login.isPending}
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
                {isSubmitting || login.isPending ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  'Sign In'
                )}
              </Button>
            </form>

            <Divider sx={{ my: 3 }}>
              <Typography sx={{ color: '#78716C', fontSize: '0.875rem' }}>
                or continue with
              </Typography>
            </Divider>

            <Stack direction="row" spacing={2}>
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

            <Typography
              sx={{
                mt: 4,
                textAlign: 'center',
                color: '#78716C',
                fontSize: '0.9375rem',
              }}
            >
              Don't have an account?{' '}
              <Typography
                component={Link}
                to="/register"
                sx={{
                  color: '#CA8A04',
                  textDecoration: 'none',
                  fontWeight: 600,
                  '&:hover': { textDecoration: 'underline' },
                }}
              >
                Create account
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
