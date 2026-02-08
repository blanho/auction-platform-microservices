import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, Stack } from '@mui/material'
import {
  Error as ErrorIcon,
  East,
  Email,
  MarkEmailRead,
  CheckCircle,
} from '@mui/icons-material'
import { useConfirmEmail, useResendConfirmation } from '../hooks'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams()
  const [resendSuccess, setResendSuccess] = useState(false)
  const confirmEmail = useConfirmEmail()
  const resendConfirmation = useResendConfirmation()

  const token = searchParams.get('token')
  const userId = searchParams.get('userId')
  const email = searchParams.get('email')

  useEffect(() => {
    if (token && userId) {
      confirmEmail.mutate({ token, userId })
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, userId])

  const handleResend = async () => {
    if (email) {
      try {
        await resendConfirmation.mutateAsync(email)
        setResendSuccess(true)
      } catch {
        /* Error handled by mutation */
      }
    }
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
                Verify Your Email
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                Please check your inbox and click the verification link we sent to complete your
                registration.
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
                    'Resend Verification Email'
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
                      Verification email sent! Check your inbox.
                    </Typography>
                  </Stack>
                </Box>
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
                Go to Login
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
                Verifying Email...
              </Typography>

              <Typography sx={{ color: palette.neutral[500] }}>
                Please wait while we confirm your email address.
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
                  Welcome to
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
                  Your account is now verified. Start exploring exclusive auctions and rare finds.
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
                Email Verified!
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                Your email has been successfully verified. You can now sign in to your account.
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
                Sign In
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
                Verification Failed
              </Typography>

              <Typography sx={{ color: palette.neutral[500], mb: 5 }}>
                We couldn't verify your email. The link may have expired or is invalid.
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
                    'Resend Verification Email'
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
                      Verification email sent! Check your inbox.
                    </Typography>
                  </Stack>
                </Box>
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
                Back to Login
              </Button>
            </motion.div>
          </Box>
        </Box>
      </Box>
    )
  }

  return null
}
