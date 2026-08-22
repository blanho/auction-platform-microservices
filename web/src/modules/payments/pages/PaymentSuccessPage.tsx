import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { formatCurrency } from '@/shared/utils/formatters'
import { ArrowForward, CheckCircle, Home, LocalShipping, Receipt } from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  CircularProgress,
  Container,
  Divider,
  Stack,
  Typography,
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ordersApi } from '../api'

export function PaymentSuccessPage() {
  const { t } = useTranslation('payments')
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const orderId = searchParams.get('order_id')
  const [countdown, setCountdown] = useState(10)
  const [verificationTimedOut, setVerificationTimedOut] = useState(false)

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
    refetchInterval: (query) =>
      verificationTimedOut || query.state.data?.paymentStatus === 'completed' ? false : 1000,
  })

  const paymentConfirmed = order?.paymentStatus === 'completed'

  useEffect(() => {
    if (!orderId) {
      navigate('/orders')
    }
  }, [orderId, navigate])

  useEffect(() => {
    if (paymentConfirmed || error || !orderId) {
      return
    }

    const timer = setTimeout(() => setVerificationTimedOut(true), 20_000)
    return () => clearTimeout(timer)
  }, [paymentConfirmed, error, orderId])

  useEffect(() => {
    if (!paymentConfirmed) {
      return
    }

    if (countdown === 0) {
      navigate(orderId ? `/orders/${orderId}` : '/orders', { replace: true })
      return
    }

    const timer = setTimeout(() => setCountdown((previous) => previous - 1), 1000)
    return () => clearTimeout(timer)
  }, [countdown, navigate, orderId, paymentConfirmed])

  if (isLoading || (!paymentConfirmed && !verificationTimedOut && !error)) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Card sx={{ p: 6, textAlign: 'center' }}>
          <CircularProgress size={48} sx={{ color: palette.brand.primary, mb: 3 }} />
          <Typography variant="h6">{t('success.confirming')}</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {t('success.verifying')}
          </Typography>
        </Card>
      </Container>
    )
  }

  if (error || verificationTimedOut || !paymentConfirmed) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Card sx={{ p: 6, textAlign: 'center' }}>
          <InlineAlert severity="warning" sx={{ mb: 3 }}>
            {error ? t('success.orderLookupFailed') : t('success.verificationPending')}
          </InlineAlert>
          <Button
            variant="contained"
            component={Link}
            to="/orders"
            sx={{ bgcolor: palette.brand.primary, '&:hover': { bgcolor: '#A16207' } }}
          >
            {t('success.viewOrders')}
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
                {t('success.title')}
              </Typography>

              <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                {t('success.description')}
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
                          {t('success.orderNumber')}
                        </Typography>
                        <Typography variant="body2" fontWeight={600}>
                          #{order.id.slice(0, 8).toUpperCase()}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          {t('success.item')}
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
                          {t('success.totalPaid')}
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
                      {t('success.nextTitle')}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {t('success.nextDescription')}
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
                  {t('success.viewOrder')}
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
                  {t('success.continueShopping')}
                </Button>
              </Stack>

              {countdown > 0 && (
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{ display: 'block', mt: 3 }}
                >
                  {t('success.redirecting', { count: countdown })}
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
                {t('success.backHome')}
              </Button>
            </Box>
          </motion.div>
        </motion.div>
      </Container>
    </Box>
  )
}
