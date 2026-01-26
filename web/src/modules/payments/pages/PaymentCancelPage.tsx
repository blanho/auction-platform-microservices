import { useEffect } from 'react'
import { useSearchParams, useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Button,
  Stack,
  Alert,
} from '@mui/material'
import {
  Cancel,
  Refresh,
  SupportAgent,
  Home,
  ShoppingCart,
} from '@mui/icons-material'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

export function PaymentCancelPage() {
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
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh', py: 8 }}>
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
                    bgcolor: '#FEE2E2',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mx: 'auto',
                    mb: 3,
                  }}
                >
                  <Cancel sx={{ fontSize: 48, color: '#EF4444' }} />
                </Box>
              </motion.div>

              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: '#1C1917',
                  mb: 1,
                }}
              >
                Payment Cancelled
              </Typography>

              <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                Your payment was not completed. No charges have been made to your account.
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
                    Why was my payment cancelled?
                  </Typography>
                  <Typography variant="body2">
                    This could happen if you:
                  </Typography>
                  <Box component="ul" sx={{ mt: 1, mb: 0, pl: 2 }}>
                    <li>
                      <Typography variant="body2">Clicked the back button or closed the payment page</Typography>
                    </li>
                    <li>
                      <Typography variant="body2">The payment session expired (usually after 30 minutes)</Typography>
                    </li>
                    <li>
                      <Typography variant="body2">Your bank declined the transaction</Typography>
                    </li>
                  </Box>
                </Alert>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Box
                  sx={{
                    p: 3,
                    bgcolor: '#F5F5F4',
                    borderRadius: 2,
                    mb: 4,
                  }}
                >
                  <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                    Your item is still available!
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Don't worry - your winning bid is still valid. You can try the payment again at any time.
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
                      bgcolor: '#CA8A04',
                      px: 4,
                      py: 1.5,
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#A16207' },
                    }}
                  >
                    Try Again
                  </Button>
                )}
                <Button
                  variant="outlined"
                  component={Link}
                  to="/auctions"
                  startIcon={<ShoppingCart />}
                  sx={{
                    borderColor: '#1C1917',
                    color: '#1C1917',
                    px: 4,
                    py: 1.5,
                    fontWeight: 600,
                    '&:hover': { borderColor: '#44403C', bgcolor: '#F5F5F4' },
                  }}
                >
                  Browse Auctions
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
              <SupportAgent sx={{ color: '#CA8A04', fontSize: 32 }} />
              <Box sx={{ flex: 1 }}>
                <Typography variant="subtitle2" fontWeight={600}>
                  Need help?
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  If you're experiencing issues with payments, our support team is here to help.
                </Typography>
              </Box>
              <Button
                variant="text"
                sx={{ color: '#CA8A04', fontWeight: 600 }}
              >
                Contact Support
              </Button>
            </Card>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box sx={{ textAlign: 'center', mt: 4 }}>
              <Button
                component={Link}
                to="/"
                startIcon={<Home />}
                sx={{ color: '#78716C' }}
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
