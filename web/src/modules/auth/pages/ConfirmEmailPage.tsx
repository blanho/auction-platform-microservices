import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Typography, Button, CircularProgress, alpha } from '@mui/material'
import {
  Error as ErrorIcon,
  ArrowForward,
  Email,
  MarkEmailRead,
} from '@mui/icons-material'
import { useConfirmEmail, useResendConfirmation } from '../hooks'
import { palette, colors, gradients } from '@/shared/theme/tokens'
import { fadeInUp } from '@/shared/lib/animations'

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

  const glassCardStyles = {
    background: 'rgba(255, 255, 255, 0.03)',
    backdropFilter: 'blur(20px)',
    WebkitBackdropFilter: 'blur(20px)',
    border: '1px solid rgba(255, 255, 255, 0.08)',
    borderRadius: 3,
  }

  if (!token || !userId) {
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

        <Box sx={{ position: 'relative', width: '100%', maxWidth: 480, px: 3 }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
            <Box sx={{ ...glassCardStyles, p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
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
                  mb: 3,
                  boxShadow: `0 0 40px ${alpha(palette.brand.primary, 0.4)}`,
                }}
              >
                <Email sx={{ fontSize: 40, color: palette.neutral[900] }} />
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
                Verify Your Email
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 4 }}>
                Please check your inbox and click the verification link we sent to complete your registration.
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
                    borderColor: 'rgba(255, 255, 255, 0.2)',
                    color: palette.neutral[50],
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': {
                      borderColor: palette.brand.primary,
                      bgcolor: alpha(palette.brand.primary, 0.05),
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
                    borderRadius: 2,
                    bgcolor: alpha(palette.semantic.success, 0.1),
                    border: `1px solid ${alpha(palette.semantic.success, 0.3)}`,
                  }}
                >
                  <Typography sx={{ color: palette.semantic.success, fontSize: '0.875rem' }}>
                    Verification email sent! Check your inbox.
                  </Typography>
                </Box>
              )}

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
                Go to Login
              </Button>
            </Box>
          </motion.div>
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

        <Box sx={{ position: 'relative', width: '100%', maxWidth: 480, px: 3 }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
            <Box sx={{ ...glassCardStyles, p: { xs: 3, sm: 5 }, textAlign: 'center' }}>
              <Box sx={{ mb: 3 }}>
                <CircularProgress size={60} sx={{ color: palette.brand.primary }} />
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
                Verifying Email...
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)' }}>
                Please wait while we confirm your email address.
              </Typography>
            </Box>
          </motion.div>
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
                  <MarkEmailRead sx={{ fontSize: 40, color: 'white' }} />
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
                Email Verified!
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 4 }}>
                Your email has been successfully verified. You can now sign in to your account.
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
                Sign In
              </Button>
            </Box>
          </motion.div>
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
                Verification Failed
              </Typography>

              <Typography sx={{ color: 'rgba(255, 255, 255, 0.5)', mb: 4 }}>
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
                    borderColor: 'rgba(255, 255, 255, 0.2)',
                    color: palette.neutral[50],
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': {
                      borderColor: palette.brand.primary,
                      bgcolor: alpha(palette.brand.primary, 0.05),
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
                    borderRadius: 2,
                    bgcolor: alpha(palette.semantic.success, 0.1),
                    border: `1px solid ${alpha(palette.semantic.success, 0.3)}`,
                  }}
                >
                  <Typography sx={{ color: palette.semantic.success, fontSize: '0.875rem' }}>
                    Verification email sent! Check your inbox.
                  </Typography>
                </Box>
              )}

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
                Back to Login
              </Button>
            </Box>
          </motion.div>
        </Box>
      </Box>
    )
  }

  return null
}
