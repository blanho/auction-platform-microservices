import { useEffect, useState } from 'react'
import { useSearchParams, useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Button,
  CircularProgress,
  Stack,
  Divider,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import { CheckCircle, Receipt, LocalShipping, ArrowForward, Home } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '../api'
import { formatCurrency } from '@/shared/utils/formatters'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

export function PaymentSuccessPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const orderId = searchParams.get('order_id')
  const sessionId = searchParams.get('session_id')
  const [countdown, setCountdown] = useState(10)

  const {
    data: order,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['order', orderId],
    queryFn: () => {
      if (!orderId) {
        throw new Error('Order ID is required')
      }
      return ordersApi.getOrderById(orderId)
    },
    enabled: !!orderId,
    retry: 3,
    retryDelay: 1000,
  })

  useEffect(() => {
    if (!orderId && !sessionId) {
      navigate('/orders')
    }
  }, [orderId, sessionId, navigate])

  useEffect(() => {
    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer)
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [])

  if (isLoading) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Card sx={{ p: 6, textAlign: 'center' }}>
          <CircularProgress size={48} sx={{ color: palette.brand.primary, mb: 3 }} />
          <Typography variant="h6">Confirming your payment...</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Please wait while we verify your transaction
          </Typography>
        </Card>
      </Container>
    )
  }

  if (error) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Card sx={{ p: 6, textAlign: 'center' }}>
          <InlineAlert severity="warning" sx={{ mb: 3 }}>
            We couldn't retrieve your order details, but your payment may have been processed.
          </InlineAlert>
          <Button
            variant="contained"
            component={Link}
            to="/orders"
            sx={{ bgcolor: palette.brand.primary, '&:hover': { bgcolor: '#A16207' } }}
          >
            View My Orders
          </Button>
        </Card>
      </Container>
    )
  }

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', py: 8 }}>
      <Container maxWidth="sm">
        <motion.div variants={staggerContainer} initial="initial" animate="animate">
          <motion.div variants={fadeInUp}>
            <Card
              sx={{
                p: 6,
                textAlign: 'center',
                borderRadius: 3,
                boxShadow: '0 8px 32px rgba(0,0,0,0.08)',
              }}
            >
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ type: 'spring', duration: 0.6, delay: 0.2 }}
              >
                <Box
                  sx={{
                    width: 80,
                    height: 80,
                    borderRadius: '50%',
                    bgcolor: palette.semantic.successLight,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <CheckCircle sx={{ fontSize: 48, color: palette.semantic.success }} />
                </Box>
              </motion.div>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Payment Successful!
              </Typography>

              <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                Thank you for your purchase. Your order has been confirmed.
              </Typography>

              {order && (
                <motion.div variants={staggerItem}>
                  <Card
                    variant="outlined"
                    sx={{
                      p: 3,
                      mb: 4,
                      bgcolor: palette.neutral[50],
                      borderColor: palette.neutral[200],
                      textAlign: 'left',
                    }}
                  >
                    <Stack spacing={2}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Order Number
                        </Typography>
                        <Typography variant="body2" fontWeight={600}>
                          #{order.id.slice(0, 8).toUpperCase()}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Item
                        </Typography>
                        <Typography
                          variant="body2"
                          fontWeight={500}
                          sx={{ maxWidth: 200, textAlign: 'right' }}
                        >
                          {order.auctionTitle || order.itemTitle}
                        </Typography>
                      </Box>
                      <Divider />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Total Paid
                        </Typography>
                        <Typography variant="h6" fontWeight={700} color={palette.brand.primary}>
                          {formatCurrency(order.totalAmount)}
                        </Typography>
                      </Box>
                    </Stack>
                  </Card>
                </motion.div>
              )}

              <motion.div variants={staggerItem}>
                <Box
                  sx={{
                    p: 3,
                    bgcolor: palette.semantic.warningLight,
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 2,
                    mb: 4,
                  }}
                >
                  <LocalShipping sx={{ color: palette.brand.primary }} />
                  <Box sx={{ textAlign: 'left' }}>
                    <Typography variant="subtitle2" fontWeight={600}>
                      What happens next?
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      The seller will be notified and will ship your item soon.
                    </Typography>
                  </Box>
                </Box>
              </motion.div>

              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
                <Button
                  variant="contained"
                  component={Link}
                  to={orderId ? `/orders/${orderId}` : '/orders'}
                  startIcon={<Receipt />}
                  sx={{
                    bgcolor: palette.brand.primary,
                    px: 4,
                    py: 1.5,
                    fontWeight: 600,
                    '&:hover': { bgcolor: '#A16207' },
                  }}
                >
                  View Order Details
                </Button>
                <Button
                  variant="outlined"
                  component={Link}
                  to="/auctions"
                  endIcon={<ArrowForward />}
                  sx={{
                    borderColor: palette.neutral[900],
                    color: palette.neutral[900],
                    px: 4,
                    py: 1.5,
                    fontWeight: 600,
                    '&:hover': { borderColor: palette.neutral[700], bgcolor: palette.neutral[100] },
                  }}
                >
                  Continue Shopping
                </Button>
              </Stack>

              {countdown > 0 && (
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{ display: 'block', mt: 3 }}
                >
                  Redirecting to order details in {countdown}s...
                </Typography>
              )}
            </Card>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box sx={{ textAlign: 'center', mt: 4 }}>
              <Button
                component={Link}
                to="/"
                startIcon={<Home />}
                sx={{ color: palette.neutral[500] }}
              >
                Back to Home
              </Button>
            </Box>
          </motion.div>
        </motion.div>
      </Container>
    </Box>
  )
}
