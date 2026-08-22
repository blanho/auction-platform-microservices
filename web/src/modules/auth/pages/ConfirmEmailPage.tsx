import { getErrorMessage } from '@/services/http'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { CheckCircle, East, Email, Error as ErrorIcon, MarkEmailRead } from '@mui/icons-material'
import { Box, Button, CircularProgress, Stack, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router-dom'
import { useConfirmEmail, useResendConfirmation } from '../hooks'

export function ConfirmEmailPage() {
  const { t } = useTranslation('auth')
  const [searchParams] = useSearchParams()
  const [resendSuccess, setResendSuccess] = useState(false)
  const confirmEmail = useConfirmEmail()
  const resendConfirmation = useResendConfirmation()
  const processedConfirmationRef = useRef<string | null>(null)

  const token = searchParams.get('token')
  const userId = searchParams.get('userId')
  const email = searchParams.get('email')

  useEffect(() => {
    if (!token || !userId) {
      return
    }

    const confirmationKey = `${userId}:${token}`
    if (processedConfirmationRef.current === confirmationKey) {
      return
    }

    processedConfirmationRef.current = confirmationKey
    confirmEmail.mutate({ token, userId })
  }, [confirmEmail, token, userId])

  const handleResend = () => {
    if (!email) {
      return
    }

    setResendSuccess(false)
    resendConfirmation.mutate(email, {
      onSuccess: () => {
        setResendSuccess(true)
      },
    })
  }

  if (!token || !userId) {
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
                'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 2,
                }}
              >
                {t('confirmEmail.title')}
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                {t('confirmEmail.checkInbox')}
              </Typography>

              {email && !resendSuccess && (
                <Button
                  fullWidth
                  variant="outlined"
                  onClick={handleResend}
                  disabled={resendConfirmation.isPending}
                  sx={{
                    mb: 2,
                    py: 1.5,
                    borderColor: palette.neutral[300],
                    color: palette.neutral[900],
                    textTransform: 'none',
                    borderRadius: 0,
                    '&:hover': {
                      borderColor: palette.neutral[900],
                      bgcolor: 'transparent',
                    },
                  }}
                >
                  {resendConfirmation.isPending ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    t('confirmEmail.resend')
                  )}
                </Button>
              )}

              {resendSuccess && (
                <Box
                  sx={{
                    mb: 3,
                    p: 2,
                    border: `1px solid ${palette.semantic.success}`,
                    bgcolor: 'rgba(22, 163, 74, 0.05)',
                  }}
                >
                  <Stack direction="row" spacing={1} alignItems="center" justifyContent="center">
                    <CheckCircle sx={{ color: palette.semantic.success, fontSize: 20 }} />
                    <Typography sx={{ color: palette.semantic.success, fontSize: '0.875rem' }}>
                      {t('confirmEmail.resendSuccess')}
                    </Typography>
                  </Stack>
                </Box>
              )}

              {resendConfirmation.isError && (
                <Typography color="error" sx={{ mb: 3, fontSize: '0.875rem' }}>
                  {getErrorMessage(resendConfirmation.error)}
                </Typography>
              )}

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
                {t('confirmEmail.goToLogin')}
              </Button>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  if (confirmEmail.isPending) {
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
                'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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
              <Box sx={{ mb: 4 }}>
                <CircularProgress size={60} sx={{ color: palette.neutral[900] }} />
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
                {t('confirmEmail.verifying')}
              </Typography>

              <Typography sx={{ color: palette.neutral[500] }}>
                {t('confirmEmail.pleaseWait')}
              </Typography>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  if (confirmEmail.isSuccess) {
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
                'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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
                  {t('confirmEmail.welcomeTo')}
                  <br />
                  TheAuction
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
                  {t('confirmEmail.heroDescription')}
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
                  <MarkEmailRead sx={{ fontSize: 40, color: 'white' }} />
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
                {t('confirmEmail.success')}
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                {t('confirmEmail.successMessage')}
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
                {t('login.submit')}
              </Button>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  if (confirmEmail.isError) {
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
                'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
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
                {t('confirmEmail.error')}
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                {t('confirmEmail.errorMessage')}
              </Typography>

              {email && !resendSuccess && (
                <Button
                  fullWidth
                  variant="outlined"
                  onClick={handleResend}
                  disabled={resendConfirmation.isPending}
                  sx={{
                    mb: 2,
                    py: 1.5,
                    borderColor: palette.neutral[300],
                    color: palette.neutral[900],
                    textTransform: 'none',
                    borderRadius: 0,
                    '&:hover': {
                      borderColor: palette.neutral[900],
                      bgcolor: 'transparent',
                    },
                  }}
                >
                  {resendConfirmation.isPending ? (
                    <CircularProgress size={20} color="inherit" />
                  ) : (
                    t('confirmEmail.resend')
                  )}
                </Button>
              )}

              {resendSuccess && (
                <Box
                  sx={{
                    mb: 3,
                    p: 2,
                    border: `1px solid ${palette.semantic.success}`,
                    bgcolor: 'rgba(22, 163, 74, 0.05)',
                  }}
                >
                  <Stack direction="row" spacing={1} alignItems="center" justifyContent="center">
                    <CheckCircle sx={{ color: palette.semantic.success, fontSize: 20 }} />
                    <Typography sx={{ color: palette.semantic.success, fontSize: '0.875rem' }}>
                      {t('confirmEmail.resendSuccess')}
                    </Typography>
                  </Stack>
                </Box>
              )}

              {resendConfirmation.isError && (
                <Typography color="error" sx={{ mb: 3, fontSize: '0.875rem' }}>
                  {getErrorMessage(resendConfirmation.error)}
                </Typography>
              )}

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
                {t('confirmEmail.backToLogin')}
              </Button>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  return null
}
