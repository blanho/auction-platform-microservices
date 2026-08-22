import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { Cancel, Home, Refresh, ShoppingCart, SupportAgent } from '@mui/icons-material'
import { Alert, Box, Button, Card, Container, Stack, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'

export function PaymentCancelPage() {
  const { t } = useTranslation('payments')
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const orderId = searchParams.get('order_id')
  const auctionId = searchParams.get('auction_id')

  useEffect(() => {
    if (!orderId && !auctionId) {
      const timer = setTimeout(() => {
        navigate('/auctions')
      }, 10000)
      return () => clearTimeout(timer)
    }
  }, [orderId, auctionId, navigate])

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
                    bgcolor: palette.semantic.errorLight,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <Cancel sx={{ fontSize: 48, color: palette.semantic.error }} />
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
                {t('cancelled.title')}
              </Typography>

              <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                {t('cancelled.description')}
              </Typography>

              <motion.div variants={staggerItem}>
                <Alert
                  severity="info"
                  sx={{
                    mb: 4,
                    textAlign: 'left',
                    '& .MuiAlert-message': { width: '100%' },
                  }}
                >
                  <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                    {t('cancelled.why')}
                  </Typography>
                  <Typography variant="body2">{t('cancelled.possibleReasons')}</Typography>
                  <Box component="ul" sx={{ mt: 1, mb: 0, pl: 2 }}>
                    <li>
                      <Typography variant="body2">{t('cancelled.reasonNavigation')}</Typography>
                    </li>
                    <li>
                      <Typography variant="body2">{t('cancelled.reasonExpired')}</Typography>
                    </li>
                    <li>
                      <Typography variant="body2">{t('cancelled.reasonDeclined')}</Typography>
                    </li>
                  </Box>
                </Alert>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Box
                  sx={{
                    p: 3,
                    bgcolor: palette.neutral[100],
                    borderRadius: 2,
                    mb: 4,
                  }}
                >
                  <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                    {t('cancelled.availableTitle')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {t('cancelled.availableDescription')}
                  </Typography>
                </Box>
              </motion.div>

              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
                {(orderId || auctionId) && (
                  <Button
                    variant="contained"
                    component={Link}
                    to={auctionId ? `/checkout/${auctionId}` : `/orders`}
                    startIcon={<Refresh />}
                    sx={{
                      bgcolor: palette.brand.primary,
                      px: 4,
                      py: 1.5,
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#A16207' },
                    }}
                  >
                    {t('cancelled.tryAgain')}
                  </Button>
                )}
                <Button
                  variant="outlined"
                  component={Link}
                  to="/auctions"
                  startIcon={<ShoppingCart />}
                  sx={{
                    borderColor: palette.neutral[900],
                    color: palette.neutral[900],
                    px: 4,
                    py: 1.5,
                    fontWeight: 600,
                    '&:hover': { borderColor: palette.neutral[700], bgcolor: palette.neutral[100] },
                  }}
                >
                  {t('cancelled.browseAuctions')}
                </Button>
              </Stack>
            </Card>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Card
              sx={{
                p: 3,
                mt: 3,
                borderRadius: 2,
                display: 'flex',
                alignItems: 'center',
                gap: 2,
              }}
            >
              <SupportAgent sx={{ color: palette.brand.primary, fontSize: 32 }} />
              <Box sx={{ flex: 1 }}>
                <Typography variant="subtitle2" fontWeight={600}>
                  {t('cancelled.helpTitle')}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('cancelled.helpDescription')}
                </Typography>
              </Box>
              <Button variant="text" sx={{ color: palette.brand.primary, fontWeight: 600 }}>
                {t('cancelled.contactSupport')}
              </Button>
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
                {t('cancelled.backHome')}
              </Button>
            </Box>
          </motion.div>
        </motion.div>
      </Container>
    </Box>
  )
}
