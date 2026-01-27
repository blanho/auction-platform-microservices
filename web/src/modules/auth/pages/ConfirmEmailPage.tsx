import { useEffect, useState } from 'react'
import { useSearchParams, Link, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Typography,
  Box,
  Card,
  Stack,
  Button,
  CircularProgress,
  Alert,
} from '@mui/material'
import {
  CheckCircle,
  Error as ErrorIcon,
  Email,
  ArrowForward,
  Refresh,
} from '@mui/icons-material'
import { useMutation } from '@tanstack/react-query'
import { http } from '@/services/http'
import { fadeInUp, staggerContainer, staggerItem, scaleIn } from '@/shared/lib/animations'

interface ConfirmEmailResponse {
  message: string
}

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const token = searchParams.get('token')
  const email = searchParams.get('email')

  const initialStatus = token ? 'loading' : 'no-token'
  const [status, setStatus] = useState<'loading' | 'success' | 'error' | 'no-token'>(initialStatus)

  const confirmMutation = useMutation({
    mutationFn: async (confirmToken: string) => {
      const response = await http.post<ConfirmEmailResponse>('/auth/confirm-email', {
        token: confirmToken,
      })
      return response.data
    },
    onSuccess: () => {
      setStatus('success')
    },
    onError: () => {
      setStatus('error')
    },
  })

  const resendMutation = useMutation({
    mutationFn: async (userEmail: string) => {
      const response = await http.post<{ message: string }>('/auth/resend-confirmation', {
        email: userEmail,
      })
      return response.data
    },
  })

  useEffect(() => {
    if (token && !confirmMutation.isPending && !confirmMutation.isSuccess && !confirmMutation.isError) {
      confirmMutation.mutate(token)
    }
  }, [token, confirmMutation])

  const handleResend = () => {
    if (email) {
      resendMutation.mutate(email)
    }
  }

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Card
            sx={{
              p: { xs: 4, md: 6 },
              textAlign: 'center',
              borderRadius: 3,
              boxShadow: '0 8px 40px rgba(0,0,0,0.08)',
            }}
          >
            {status === 'loading' && (
              <Stack spacing={3} alignItems="center">
                <CircularProgress size={64} thickness={4} sx={{ color: 'primary.main' }} />
                <Typography variant="h5" fontWeight={600}>
                  Verifying your email...
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  Please wait while we confirm your email address
                </Typography>
              </Stack>
            )}

            {status === 'success' && (
              <motion.div variants={scaleIn} initial="initial" animate="animate">
                <Stack spacing={3} alignItems="center">
                  <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ type: 'spring', stiffness: 200, damping: 15, delay: 0.2 }}
                  >
                    <Box
                      sx={{
                        width: 80,
                        height: 80,
                        borderRadius: '50%',
                        bgcolor: 'success.light',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                      }}
                    >
                      <CheckCircle sx={{ fontSize: 48, color: 'success.main' }} />
                    </Box>
                  </motion.div>

                  <Box>
                    <Typography
                      variant="h4"
                      sx={{
                        fontFamily: '"Playfair Display", serif',
                        fontWeight: 700,
                        color: 'primary.main',
                        mb: 1,
                      }}
                    >
                      Email Verified!
                    </Typography>
                    <Typography variant="body1" color="text.secondary">
                      Your email has been successfully verified. You can now access all features.
                    </Typography>
                  </Box>

                  <Button
                    variant="contained"
                    size="large"
                    endIcon={<ArrowForward />}
                    onClick={() => navigate('/login')}
                    sx={{
                      mt: 2,
                      px: 4,
                      py: 1.5,
                      bgcolor: 'primary.main',
                      '&:hover': { bgcolor: 'primary.dark' },
                    }}
                  >
                    Continue to Login
                  </Button>
                </Stack>
              </motion.div>
            )}

            {status === 'error' && (
              <motion.div variants={staggerItem}>
                <Stack spacing={3} alignItems="center">
                  <Box
                    sx={{
                      width: 80,
                      height: 80,
                      borderRadius: '50%',
                      bgcolor: 'error.light',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <ErrorIcon sx={{ fontSize: 48, color: 'error.main' }} />
                  </Box>

                  <Box>
                    <Typography variant="h5" fontWeight={600} gutterBottom>
                      Verification Failed
                    </Typography>
                    <Typography variant="body1" color="text.secondary">
                      The verification link may have expired or is invalid.
                    </Typography>
                  </Box>

                  {email && (
                    <>
                      <Alert severity="info" sx={{ width: '100%', textAlign: 'left' }}>
                        If you need a new verification link, click the button below to resend.
                      </Alert>

                      <Button
                        variant="contained"
                        startIcon={resendMutation.isPending ? <CircularProgress size={20} /> : <Refresh />}
                        onClick={handleResend}
                        disabled={resendMutation.isPending || resendMutation.isSuccess}
                        sx={{
                          bgcolor: 'primary.main',
                          '&:hover': { bgcolor: 'primary.dark' },
                        }}
                      >
                        {resendMutation.isPending
                          ? 'Sending...'
                          : resendMutation.isSuccess
                            ? 'Email Sent!'
                            : 'Resend Verification Email'}
                      </Button>

                      {resendMutation.isSuccess && (
                        <Alert severity="success" sx={{ width: '100%' }}>
                          A new verification email has been sent to {email}
                        </Alert>
                      )}

                      {resendMutation.isError && (
                        <Alert severity="error" sx={{ width: '100%' }}>
                          Failed to resend verification email. Please try again.
                        </Alert>
                      )}
                    </>
                  )}

                  <Button variant="outlined" component={Link} to="/login">
                    Back to Login
                  </Button>
                </Stack>
              </motion.div>
            )}

            {status === 'no-token' && (
              <motion.div variants={staggerItem}>
                <Stack spacing={3} alignItems="center">
                  <Box
                    sx={{
                      width: 80,
                      height: 80,
                      borderRadius: '50%',
                      bgcolor: 'grey.100',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Email sx={{ fontSize: 48, color: 'grey.500' }} />
                  </Box>

                  <Box>
                    <Typography variant="h5" fontWeight={600} gutterBottom>
                      Check Your Email
                    </Typography>
                    <Typography variant="body1" color="text.secondary">
                      We've sent you a verification link. Please check your inbox and click the link to verify your email.
                    </Typography>
                  </Box>

                  <Alert severity="info" sx={{ width: '100%', textAlign: 'left' }}>
                    <Typography variant="body2">
                      <strong>Didn't receive the email?</strong>
                    </Typography>
                    <Typography variant="body2">
                      • Check your spam folder
                      <br />
                      • Make sure you entered the correct email
                      <br />
                      • Wait a few minutes and try again
                    </Typography>
                  </Alert>

                  <Stack direction="row" spacing={2}>
                    <Button variant="outlined" component={Link} to="/login">
                      Back to Login
                    </Button>
                    <Button variant="contained" component={Link} to="/register">
                      Register Again
                    </Button>
                  </Stack>
                </Stack>
              </motion.div>
            )}
          </Card>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Box sx={{ mt: 4, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              Having trouble?{' '}
              <Button
                component="a"
                href="mailto:support@auction.com"
                sx={{ textTransform: 'none', p: 0, minWidth: 'auto' }}
              >
                Contact Support
              </Button>
            </Typography>
          </Box>
        </motion.div>
      </motion.div>
    </Container>
  )
}
