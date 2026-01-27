import { Box, Container, Typography, Grid, Button, Stack } from '@mui/material'
import { Link } from 'react-router-dom'
import {
  PersonAdd,
  Search,
  Gavel,
  EmojiEvents,
  Payment,
  LocalShipping,
  ArrowForward,
} from '@mui/icons-material'

const steps = [
  {
    icon: <PersonAdd sx={{ fontSize: 48 }} />,
    step: '01',
    title: 'Create Your Account',
    description:
      'Sign up in minutes with your email. Verify your identity to start bidding or selling on our platform.',
  },
  {
    icon: <Search sx={{ fontSize: 48 }} />,
    step: '02',
    title: 'Discover & Browse',
    description:
      'Explore thousands of curated items across categories. Use filters to find exactly what you are looking for.',
  },
  {
    icon: <Gavel sx={{ fontSize: 48 }} />,
    step: '03',
    title: 'Place Your Bids',
    description:
      'Set your maximum bid and let our system bid automatically, or bid manually in real-time auctions.',
  },
  {
    icon: <EmojiEvents sx={{ fontSize: 48 }} />,
    step: '04',
    title: 'Win the Auction',
    description:
      'Get notified instantly when you win. Review your purchase details and proceed to checkout.',
  },
  {
    icon: <Payment sx={{ fontSize: 48 }} />,
    step: '05',
    title: 'Secure Payment',
    description:
      'Pay securely through our protected payment system. Multiple payment options available.',
  },
  {
    icon: <LocalShipping sx={{ fontSize: 48 }} />,
    step: '06',
    title: 'Receive Your Item',
    description:
      'Track your shipment in real-time. All items are insured and professionally packaged.',
  },
]

const sellerSteps = [
  {
    number: '01',
    title: 'List Your Item',
    description: 'Upload photos, write a description, and set your starting price and reserve.',
  },
  {
    number: '02',
    title: 'Get Verified',
    description: 'Our experts review and authenticate your item for buyer confidence.',
  },
  {
    number: '03',
    title: 'Watch Bids Come In',
    description: 'Monitor your auction in real-time and engage with potential buyers.',
  },
  {
    number: '04',
    title: 'Complete the Sale',
    description: 'Ship the item using our partnered carriers and receive your payment securely.',
  },
]

export const HowItWorksPage = () => {
  return (
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh' }}>
      <Box sx={{ bgcolor: '#1C1917', py: { xs: 12, md: 16 }, mb: { xs: 8, md: 12 } }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', maxWidth: 700, mx: 'auto' }}>
            <Typography
              variant="overline"
              sx={{ color: '#CA8A04', letterSpacing: 4, mb: 2, display: 'block' }}
            >
              GETTING STARTED
            </Typography>
            <Typography
              variant="h2"
              sx={{
                color: '#FAFAF9',
                fontWeight: 300,
                mb: 3,
                fontSize: { xs: '2.5rem', md: '3.5rem' },
              }}
            >
              How It Works
            </Typography>
            <Typography variant="h6" sx={{ color: 'rgba(250,250,249,0.7)', fontWeight: 400 }}>
              From discovery to delivery, we make collecting simple and secure.
            </Typography>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="lg" sx={{ mb: { xs: 10, md: 14 } }}>
        <Box sx={{ mb: 6 }}>
          <Typography
            variant="overline"
            sx={{ color: '#44403C', letterSpacing: 3, display: 'block', mb: 1 }}
          >
            FOR BUYERS
          </Typography>
          <Typography variant="h3" sx={{ color: '#1C1917', fontWeight: 400, mb: 2 }}>
            Your Journey to Discovery
          </Typography>
          <Typography variant="body1" sx={{ color: '#44403C', maxWidth: 600 }}>
            Whether you are a seasoned collector or first-time buyer, our platform makes it easy to
            find and acquire extraordinary items.
          </Typography>
        </Box>

        <Grid container spacing={4}>
          {steps.map((step, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={index}>
              <Box
                sx={{
                  p: 4,
                  height: '100%',
                  bgcolor: index % 2 === 0 ? '#FFFFFF' : 'transparent',
                  border: '1px solid rgba(68,64,60,0.1)',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    boxShadow: '0 8px 30px rgba(0,0,0,0.08)',
                    transform: 'translateY(-4px)',
                  },
                }}
              >
                <Typography
                  variant="h4"
                  sx={{ color: 'rgba(202,138,4,0.3)', fontWeight: 700, mb: 2 }}
                >
                  {step.step}
                </Typography>
                <Box sx={{ color: '#CA8A04', mb: 2 }}>{step.icon}</Box>
                <Typography variant="h6" sx={{ color: '#1C1917', fontWeight: 500, mb: 1.5 }}>
                  {step.title}
                </Typography>
                <Typography variant="body2" sx={{ color: '#44403C', lineHeight: 1.7 }}>
                  {step.description}
                </Typography>
              </Box>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box sx={{ bgcolor: '#1C1917', py: { xs: 10, md: 14 } }}>
        <Container maxWidth="lg">
          <Grid container spacing={8} alignItems="center">
            <Grid size={{ xs: 12, md: 5 }}>
              <Typography
                variant="overline"
                sx={{ color: '#CA8A04', letterSpacing: 3, display: 'block', mb: 2 }}
              >
                FOR SELLERS
              </Typography>
              <Typography variant="h3" sx={{ color: '#FAFAF9', fontWeight: 400, mb: 3 }}>
                Turn Treasures into Profit
              </Typography>
              <Typography
                variant="body1"
                sx={{ color: 'rgba(250,250,249,0.7)', mb: 4, lineHeight: 1.8 }}
              >
                Reach a global audience of collectors and enthusiasts. Our platform handles
                authentication, payments, and shipping coordination so you can focus on what matters.
              </Typography>
              <Button
                variant="outlined"
                endIcon={<ArrowForward />}
                component={Link}
                to="/sell"
                sx={{
                  borderColor: '#CA8A04',
                  color: '#CA8A04',
                  px: 4,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  '&:hover': {
                    borderColor: '#CA8A04',
                    bgcolor: 'rgba(202,138,4,0.1)',
                  },
                }}
              >
                Start Selling Today
              </Button>
            </Grid>
            <Grid size={{ xs: 12, md: 7 }}>
              <Stack spacing={0}>
                {sellerSteps.map((step, index) => (
                  <Box
                    key={index}
                    sx={{
                      display: 'flex',
                      gap: 3,
                      py: 3,
                      borderBottom:
                        index < sellerSteps.length - 1
                          ? '1px solid rgba(250,250,249,0.1)'
                          : 'none',
                    }}
                  >
                    <Typography
                      variant="h5"
                      sx={{ color: '#CA8A04', fontWeight: 600, minWidth: 40 }}
                    >
                      {step.number}
                    </Typography>
                    <Box>
                      <Typography variant="h6" sx={{ color: '#FAFAF9', fontWeight: 500, mb: 0.5 }}>
                        {step.title}
                      </Typography>
                      <Typography variant="body2" sx={{ color: 'rgba(250,250,249,0.6)' }}>
                        {step.description}
                      </Typography>
                    </Box>
                  </Box>
                ))}
              </Stack>
            </Grid>
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 10, md: 14 } }}>
        <Container maxWidth="md">
          <Box sx={{ textAlign: 'center', p: { xs: 6, md: 10 }, bgcolor: '#FFFFFF' }}>
            <Typography variant="h4" sx={{ color: '#1C1917', fontWeight: 400, mb: 2 }}>
              Ready to get started?
            </Typography>
            <Typography variant="body1" sx={{ color: '#44403C', mb: 4, maxWidth: 500, mx: 'auto' }}>
              Join our community of collectors and sellers. Create your account today and discover
              your next treasure.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                size="large"
                endIcon={<ArrowForward />}
                component={Link}
                to="/register"
                sx={{
                  bgcolor: '#CA8A04',
                  color: '#FAFAF9',
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': { bgcolor: '#A16207' },
                }}
              >
                Create Account
              </Button>
              <Button
                variant="outlined"
                size="large"
                component={Link}
                to="/auctions"
                sx={{
                  borderColor: '#1C1917',
                  color: '#1C1917',
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: '#1C1917',
                    bgcolor: 'rgba(28,25,23,0.05)',
                  },
                }}
              >
                Browse Auctions
              </Button>
            </Box>
          </Box>
        </Container>
      </Box>
    </Box>
  )
}
