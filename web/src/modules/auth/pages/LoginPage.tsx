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
  alpha,
} from '@mui/material'
import { Google, GitHub, ArrowForward, Gavel } from '@mui/icons-material'
import { loginSchema } from '../schemas'
import { useLogin, useLoginWith2FA } from '../hooks'
import type { LoginRequest } from '../types'
import { palette, colors, gradients } from '@/shared/theme/tokens'
import { InlineAlert, FormField } from '@/shared/ui'

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

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/'

  const sessionParam = searchParams.get('session')
  const sessionMessage = useMemo(
    () => (sessionParam && SESSION_MESSAGES[sessionParam] ? SESSION_MESSAGES[sessionParam] : null),
    [sessionParam]
  )

  const [rememberMe, setRememberMe] = useState(false)
  const [requires2FA, setRequires2FA] = useState(false)
  const [twoFactorToken, setTwoFactorToken] = useState('')
  const [twoFactorCode, setTwoFactorCode] = useState('')
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
    globalThis.location.href = `${baseUrl}/api/auth/external-login?provider=${provider}`
  }

  if (requires2FA) {
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

        <Box sx={{ position: 'relative', width: '100%', maxWidth: 440, px: 3 }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
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
                Two-Factor Authentication
              </Typography>

              <Typography
                sx={{
                  color: 'rgba(255, 255, 255, 0.6)',
                  textAlign: 'center',
                  mb: 4,
                }}
              >
                Enter the code from your authenticator app
              </Typography>

              {login2FA.isError && (
                <InlineAlert severity="error" sx={{ mb: 3 }}>
                  Invalid verification code. Please try again.
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
                sx={{ ...inputStyles, mb: 3 }}
              />

              <Button
                fullWidth
                variant="contained"
                onClick={handle2FASubmit}
                disabled={twoFactorCode.length !== 6 || login2FA.isPending}
                endIcon={!login2FA.isPending && <ArrowForward />}
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
                {login2FA.isPending ? <CircularProgress size={24} color="inherit" /> : 'Verify'}
              </Button>

              <Button
                fullWidth
                variant="text"
                onClick={() => setRequires2FA(false)}
                sx={{
                  mt: 2,
                  color: 'rgba(255, 255, 255, 0.5)',
                  textTransform: 'none',
                  '&:hover': {
                    color: 'rgba(255, 255, 255, 0.8)',
                    background: 'transparent',
                  },
                }}
              >
                Use a different account
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
              Discover unique treasures and rare finds from collectors worldwide
            </Typography>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box sx={{ mt: 6, display: 'flex', justifyContent: 'center', flexWrap: 'wrap', gap: { xs: 3, xl: 5 } }}>
              {[
                { label: 'Active Auctions', value: '2,500+' },
                { label: 'Trusted Sellers', value: '10K+' },
                { label: 'Items Sold', value: '50K+' },
              ].map((stat) => (
                <Box key={stat.label} sx={{ textAlign: 'center', minWidth: 100 }}>
                  <Typography
                    sx={{
                      fontFamily: '"Playfair Display", serif',
                      fontSize: { xs: '1.5rem', xl: '1.75rem' },
                      fontWeight: 600,
                      color: palette.brand.primary,
                    }}
                  >
                    {stat.value}
                  </Typography>
                  <Typography sx={{ fontSize: '0.75rem', color: 'rgba(255,255,255,0.4)', mt: 0.5, whiteSpace: 'nowrap' }}>
                    {stat.label}
                  </Typography>
                </Box>
              ))}
            </Box>
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
        }}
      >
        <Box sx={{ width: '100%', maxWidth: 480, px: { xs: 1, sm: 0 } }}>
          <motion.div initial="initial" animate="animate" variants={staggerContainer}>
            <motion.div variants={staggerItem}>
              <Box sx={{ mb: 4, textAlign: 'center' }}>
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
                  Welcome Back
                </Typography>

                <Typography
                  sx={{
                    color: 'rgba(255, 255, 255, 0.5)',
                    textAlign: 'center',
                    mb: 4,
                  }}
                >
                  Sign in to continue to your account
                </Typography>

                {sessionMessage && (
                  <InlineAlert
                    severity={sessionMessage.severity}
                    sx={{ mb: 3 }}
                  >
                    {sessionMessage.message}
                  </InlineAlert>
                )}

                {login.isError && (
                  <InlineAlert severity="error" sx={{ mb: 3 }}>
                    Invalid email or password. Please try again.
                  </InlineAlert>
                )}

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
                    sx={{ mb: 3 }}
                  >
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={rememberMe}
                          onChange={(e) => setRememberMe(e.target.checked)}
                          size="small"
                          sx={{
                            color: 'rgba(255, 255, 255, 0.3)',
                            '&.Mui-checked': { color: palette.brand.primary },
                          }}
                        />
                      }
                      label={
                        <Typography sx={{ fontSize: '0.875rem', color: 'rgba(255, 255, 255, 0.6)' }}>
                          Remember me
                        </Typography>
                      }
                    />
                    <Typography
                      component={Link}
                      to="/forgot-password"
                      sx={{
                        fontSize: '0.875rem',
                        color: palette.brand.primary,
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
                    endIcon={!isSubmitting && !login.isPending && <ArrowForward />}
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
                    {isSubmitting || login.isPending ? (
                      <CircularProgress size={24} color="inherit" />
                    ) : (
                      'Sign In'
                    )}
                  </Button>
                </form>

                <Divider
                  sx={{
                    my: 3,
                    '&::before, &::after': {
                      borderColor: 'rgba(255, 255, 255, 0.1)',
                    },
                  }}
                >
                  <Typography sx={{ color: 'rgba(255, 255, 255, 0.4)', fontSize: '0.875rem' }}>
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
                    Google
                  </Button>
                  <Button
                    fullWidth
                    variant="outlined"
                    startIcon={<GitHub />}
                    onClick={() => handleSocialLogin('github')}
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
                    GitHub
                  </Button>
                </Stack>

                <Typography
                  sx={{
                    mt: 4,
                    textAlign: 'center',
                    color: 'rgba(255, 255, 255, 0.5)',
                    fontSize: '0.9375rem',
                  }}
                >
                  Don't have an account?{' '}
                  <Typography
                    component={Link}
                    to="/register"
                    sx={{
                      color: palette.brand.primary,
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
        </Box>
      </Box>
    </Box>
  )
}
