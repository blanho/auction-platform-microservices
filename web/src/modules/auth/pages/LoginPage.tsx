import { useAuth } from '@/app/providers'
import { getApiUrl, getErrorMessage } from '@/services/http'
import { getAndClearRedirectUrl } from '@/shared/components/auth'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { FormField, InlineAlert } from '@/shared/ui'
import { zodResolver } from '@hookform/resolvers/zod'
import { East, Google } from '@mui/icons-material'
import {
  Box,
  Button,
  Checkbox,
  CircularProgress,
  Divider,
  FormControlLabel,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { createLoginSchema } from '../schemas'
import type { LoginRequest } from '../types'

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
  const { t } = useTranslation('auth')
  const loginSchema = createLoginSchema(t)
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const savedRedirect = getAndClearRedirectUrl()
  const stateFrom = (location.state as { from?: { pathname: string } })?.from?.pathname
  const from = savedRedirect || stateFrom || '/'

  const sessionParam = searchParams.get('session')
  const sessionMessages = useMemo<
    Record<string, { severity: 'warning' | 'error' | 'info'; message: string }>
  >(
    () => ({
      expired: { severity: 'warning', message: t('login.session.expired') },
      security: { severity: 'error', message: t('login.session.security') },
      logout: { severity: 'info', message: t('login.session.logout') },
    }),
    [t]
  )
  const sessionMessage = sessionParam ? (sessionMessages[sessionParam] ?? null) : null

  const [rememberMe, setRememberMe] = useState(false)
  const [requires2FA, setRequires2FA] = useState(false)
  const [twoFactorToken, setTwoFactorToken] = useState('')
  const [twoFactorCode, setTwoFactorCode] = useState('')
  const [loginError, setLoginError] = useState<string | null>(null)
  const [isLoggingIn, setIsLoggingIn] = useState(false)

  const { login: authLogin, loginWith2FA: authLoginWith2FA } = useAuth()

  useEffect(() => {
    if (sessionParam && sessionMessages[sessionParam]) {
      const nextSearchParams = new URLSearchParams(searchParams)
      nextSearchParams.delete('session')
      setSearchParams(nextSearchParams, { replace: true })
    }
  }, [sessionParam, sessionMessages, searchParams, setSearchParams])

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
    const returnUrl = encodeURIComponent(`${globalThis.location.origin}/auth/callback`)
    globalThis.location.href = getApiUrl(`/auth/external-login/Google?returnUrl=${returnUrl}`)
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
              backgroundImage:
                'url(https://images.unsplash.com/photo-1600081728723-c8aa2ee3236a?w=1200&q=80)',
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
                {t('twoFactor.title')}
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                {t('twoFactor.subtitle')}
              </Typography>

              {loginError && (
                <InlineAlert severity="error" sx={{ mb: 3, borderRadius: 0 }}>
                  {loginError}
                </InlineAlert>
              )}

              <TextField
                fullWidth
                label={t('twoFactor.codeLabel')}
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
                {isLoggingIn ? (
                  <CircularProgress size={20} color="inherit" />
                ) : (
                  t('twoFactor.submit')
                )}
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
                {t('twoFactor.backToLogin')}
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
            backgroundImage:
              'url(https://images.unsplash.com/photo-1600081728723-c8aa2ee3236a?w=1200&q=80)',
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
                {t('login.heroTitleLine1')}
                <br />
                {t('login.heroTitleLine2')}
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
                {t('login.heroDescription')}
              </Typography>
            </motion.div>

            <motion.div variants={staggerItem}>
              <Stack direction="row" spacing={6} sx={{ mt: 5 }}>
                {[
                  { value: '50K+', label: t('login.stats.itemsSold') },
                  { value: '10K+', label: t('login.stats.trustedSellers') },
                  { value: '99%', label: t('login.stats.authenticityRate') },
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
                {t('login.title')}
              </Typography>

              <Typography
                sx={{
                  color: palette.neutral[500],
                  mb: 4,
                }}
              >
                {t('login.subtitle')}
              </Typography>
            </motion.div>

            {sessionMessage && (
              <motion.div variants={staggerItem}>
                <InlineAlert severity={sessionMessage.severity} sx={{ mb: 3, borderRadius: 0 }}>
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
                  label={t('login.emailLabel')}
                  sx={{ ...inputStyles, mb: 2.5 }}
                />

                <FormField
                  name="password"
                  register={register}
                  errors={errors}
                  fullWidth
                  label={t('login.passwordLabel')}
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
                        {t('login.rememberMe')}
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
                    {t('login.forgotPassword')}
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
                    t('login.submit')
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
                  {t('login.orContinueWith')}
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
                {t('login.google')}
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
                {t('login.noAccount')}{' '}
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
                  {t('login.signUp')}
                </Typography>
              </Typography>
            </motion.div>
          </motion.div>
        </Box>
      </Box>
    </Box>
  )
}
