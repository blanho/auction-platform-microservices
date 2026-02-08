import { useState, useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate, useLocation, useSearchParams } from 'react-router-dom'
import { motion } from 'framer-motion'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import {
  Box,
  Typography,
  Button,
  Divider,
  Checkbox,
  FormControlLabel,
  CircularProgress,
  Stack,
  TextField,
} from '@mui/material'
import { Google, East } from '@mui/icons-material'
import { loginSchema } from '../schemas'
import { useAuth } from '@/app/providers'
import type { LoginRequest } from '../types'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert, FormField } from '@/shared/ui'
import { getErrorMessage } from '@/services/http'
import { getAndClearRedirectUrl } from '@/shared/components/auth'

const SESSION_MESSAGES: Record<
  string,
  { severity: 'warning' | 'error' | 'info'; message: string }
> = {
  expired: {
    severity: 'warning',
    message: 'Your session has expired. Please log in again.',
  },
  security: {
    severity: 'error',
    message:
      'Your session was terminated due to suspicious activity. All devices have been logged out for your security. Please log in again and consider changing your password.',
  },
  logout: {
    severity: 'info',
    message: 'You have been logged out successfully.',
  },
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

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const savedRedirect = getAndClearRedirectUrl()
  const stateFrom = (location.state as { from?: { pathname: string } })?.from?.pathname
  const from = savedRedirect || stateFrom || '/'

  const sessionParam = searchParams.get('session')
  const sessionMessage = useMemo(
    () => (sessionParam && SESSION_MESSAGES[sessionParam] ? SESSION_MESSAGES[sessionParam] : null),
    [sessionParam]
  )

  const [rememberMe, setRememberMe] = useState(false)
  const [requires2FA, setRequires2FA] = useState(false)
  const [twoFactorToken, setTwoFactorToken] = useState('')
  const [twoFactorCode, setTwoFactorCode] = useState('')
  const [loginError, setLoginError] = useState<string | null>(null)
  const [isLoggingIn, setIsLoggingIn] = useState(false)

  const { login: authLogin, loginWith2FA: authLoginWith2FA } = useAuth()

  useEffect(() => {
    if (sessionParam && SESSION_MESSAGES[sessionParam]) {
      searchParams.delete('session')
      setSearchParams(searchParams, { replace: true })
    }
  }, [sessionParam, searchParams, setSearchParams])

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
    setLoginError(null)
    setIsLoggingIn(true)
    try {
      const response = await authLogin(data)

      if (response.requiresTwoFactor && response.twoFactorStateToken) {
        setRequires2FA(true)
        setTwoFactorToken(response.twoFactorStateToken)
      } else {
        navigate(from, { replace: true })
      }
    } catch (err) {
      setLoginError(getErrorMessage(err))
    } finally {
      setIsLoggingIn(false)
    }
  }

  const handle2FASubmit = async () => {
    setLoginError(null)
    setIsLoggingIn(true)
    try {
      await authLoginWith2FA({
        twoFactorStateToken: twoFactorToken,
        code: twoFactorCode,
      })
      navigate(from, { replace: true })
    } catch (err) {
      setLoginError(getErrorMessage(err))
    } finally {
      setIsLoggingIn(false)
    }
  }

  const handleGoogleLogin = () => {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const returnUrl = encodeURIComponent(`${globalThis.location.origin}/auth/callback`)
    globalThis.location.href = `${baseUrl}/api/auth/external-login/Google?returnUrl=${returnUrl}`
  }

  if (requires2FA) {
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
              backgroundImage: 'url(https://images.unsplash.com/photo-1600081728723-c8aa2ee3236a?w=1200&q=80)',
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
          <Box sx={{ width: '100%', maxWidth: 440 }}>
            <motion.div initial="initial" animate="animate" variants={fadeInUp}>
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

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Two-Factor Authentication
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                Enter the 6-digit code from your authenticator app
              </Typography>

              {loginError && (
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  {loginError}
                </InlineAlert>
              )}

              <TextField
                fullWidth
                label="Verification Code"
                value={twoFactorCode}
                onChange={(e) => setTwoFactorCode(e.target.value.replaceAll(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                slotProps={{
                  htmlInput: {
                    maxLength: 6,
                    style: { textAlign: 'center', letterSpacing: '0.5em', fontSize: '1.5rem' },
                  },
                }}
                sx={{ ...inputStyles, mb: 4 }}
              />

              <Button
                fullWidth
                variant="contained"
                onClick={handle2FASubmit}
                disabled={twoFactorCode.length !== 6 || isLoggingIn}
                endIcon={!isLoggingIn && <East />}
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
                {isLoggingIn ? <CircularProgress size={20} color="inherit" /> : 'Verify Code'}
              </Button>

              <Button
                fullWidth
                variant="text"
                onClick={() => setRequires2FA(false)}
                sx={{
                  mt: 2,
                  color: palette.neutral[500],
                  textTransform: 'none',
                  '&:hover': {
                    color: palette.neutral[900],
                    bgcolor: 'transparent',
                  },
                }}
              >
                ← Back to login
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
            backgroundImage: 'url(https://images.unsplash.com/photo-1600081728723-c8aa2ee3236a?w=1200&q=80)',
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
                Discover Rare
                <br />
                & Exceptional Pieces
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
                Join thousands of collectors finding authenticated luxury items from trusted sellers worldwide.
              </Typography>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Stack direction="row" spacing={6} sx={{ mt: 5 }}>
                {[
                  { value: '50K+', label: 'Items Sold' },
                  { value: '10K+', label: 'Trusted Sellers' },
                  { value: '99%', label: 'Authenticity Rate' },
                ].map((stat) => (
                  <Box key={stat.label}>
                    <Typography
                      sx={{
                        fontFamily: '"Playfair Display", serif',
                        fontSize: '1.75rem',
                        fontWeight: 500,
                        color: palette.neutral[0],
                      }}
                    >
                      {stat.value}
                    </Typography>
                    <Typography
                      sx={{
                        fontSize: '0.75rem',
                        color: 'rgba(255,255,255,0.5)',
                        textTransform: 'uppercase',
                        letterSpacing: '0.1em',
                        mt: 0.5,
                      }}
                    >
                      {stat.label}
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
                Welcome Back
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                Sign in to your account to continue
              </Typography>
            </motion.div>

            {sessionMessage && (
              <motion.div variants={staggerItem}>
                <InlineAlert
                  severity={sessionMessage.severity}
                  sx={{ mb: 3, borderRadius: 0 }}
                >
                  {sessionMessage.message}
                </InlineAlert>
              </motion.div>
            )}

            {loginError && (
              <motion.div variants={staggerItem}>
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  {loginError}
                </InlineAlert>
              </motion.div>
            )}

            <motion.div variants={staggerItem}>
              <form onSubmit={handleSubmit(onSubmit)}>
                <FormField
                  name="usernameOrEmail"
                  register={register}
                  errors={errors}
                  fullWidth
                  label="Email or Username"
                  sx={{ ...inputStyles, mb: 2.5 }}
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

                <Stack
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                  sx={{ mb: 4 }}
                >
                  <FormControlLabel
                    control={
                      <Checkbox
                        checked={rememberMe}
                        onChange={(e) => setRememberMe(e.target.checked)}
                        size="small"
                        sx={{
                          color: palette.neutral[400],
                          '&.Mui-checked': { color: palette.neutral[900] },
                        }}
                      />
                    }
                    label={
                      <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[600] }}>
                        Remember me
                      </Typography>
                    }
                  />
                  <Typography
                    component={Link}
                    to="/forgot-password"
                    sx={{
                      fontSize: '0.875rem',
                      color: palette.neutral[900],
                      textDecoration: 'underline',
                      textUnderlineOffset: 3,
                      '&:hover': { color: palette.neutral[600] },
                    }}
                  >
                    Forgot password?
                  </Typography>
                </Stack>

                <Button
                  fullWidth
                  type="submit"
                  variant="contained"
                  disabled={isSubmitting || isLoggingIn}
                  endIcon={!isSubmitting && !isLoggingIn && <East />}
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
                  {isSubmitting || isLoggingIn ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    'Sign In'
                  )}
                </Button>
              </form>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Divider
                sx={{
                  my: 4,
                  '&::before, &::after': {
                    borderColor: palette.neutral[200],
                  },
                }}
              >
                <Typography sx={{ color: palette.neutral[400], fontSize: '0.8125rem' }}>
                  or
                </Typography>
              </Divider>

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
                  '&:hover': {
                    borderColor: palette.neutral[900],
                    bgcolor: 'transparent',
                  },
                }}
              >
                Continue with Google
              </Button>
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
                Don't have an account?{' '}
                <Typography
                  component={Link}
                  to="/register"
                  sx={{
                    color: palette.neutral[900],
                    textDecoration: 'underline',
                    textUnderlineOffset: 3,
                    fontWeight: 500,
                    '&:hover': { color: palette.neutral[600] },
                  }}
                >
                  Create account
                </Typography>
              </Typography>
            </motion.div>
          </motion.div>
        </Box>
      </Box>
    </Box>
  )
}
