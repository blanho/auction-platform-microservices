import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { PersonAdd, Search, Gavel, LocalShipping } from '@mui/icons-material'
import { typography } from '@/shared/theme/tokens'

const steps = [
  {
    key: 'register',
    number: '01',
    icon: <PersonAdd sx={{ fontSize: 24 }} />,
    title: 'Create Account',
    description: 'Sign up in seconds with your email. Verify your identity to unlock full bidding privileges.',
  },
  {
    key: 'discover',
    number: '02',
    icon: <Search sx={{ fontSize: 24 }} />,
    title: 'Discover Pieces',
    description: 'Browse curated collections. Each item includes detailed provenance, condition reports, and expert authentication.',
  },
  {
    key: 'bid',
    number: '03',
    icon: <Gavel sx={{ fontSize: 24 }} />,
    title: 'Place Your Bid',
    description: 'Bid in real-time with confidence. Set automatic bids, receive instant notifications, and track your auctions.',
  },
  {
    key: 'win',
    number: '04',
    icon: <LocalShipping sx={{ fontSize: 24 }} />,
    title: 'Win & Collect',
    description: 'Secure checkout with escrow protection. Fully insured shipping ensures your treasure arrives safely.',
  },
]

export const HowItWorksSection = () => {
  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FFFFFF', position: 'relative' }}>
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, height: '1px', bgcolor: '#E7E5E4' }} />

      <Container maxWidth="lg" sx={{ position: 'relative' }}>
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
        >
          <Box sx={{ textAlign: 'center', mb: 10 }}>
            <Typography
              sx={{
                color: '#78716C',
                letterSpacing: '0.2em',
                display: 'block',
                mb: 2,
                fontFamily: typography.fontFamily.body,
                fontSize: '0.6875rem',
                fontWeight: typography.fontWeight.medium,
                textTransform: 'uppercase',
              }}
            >
              How It Works
            </Typography>
            <Typography
              variant="h2"
              sx={{
                fontFamily: typography.fontFamily.display,
                color: '#1C1917',
                fontWeight: typography.fontWeight.regular,
                fontSize: { xs: '1.75rem', md: '2.5rem' },
                mb: 2,
              }}
            >
              Start Collecting in Four Steps
            </Typography>
            <Typography
              sx={{
                color: '#78716C',
                maxWidth: 460,
                mx: 'auto',
                fontSize: '0.9rem',
                lineHeight: 1.7,
              }}
            >
              From registration to receiving your piece — a seamless experience
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={4}>
          {steps.map((step, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={step.key}>
              <motion.div
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true, margin: '-50px' }}
                transition={{ delay: index * 0.12, duration: 0.5 }}
              >
                <Box sx={{ position: 'relative', textAlign: 'center', p: 4 }}>
                  <Typography
                    sx={{
                      fontFamily: typography.fontFamily.display,
                      fontSize: '4rem',
                      fontWeight: typography.fontWeight.regular,
                      color: '#F5F5F4',
                      position: 'absolute',
                      top: 0,
                      left: '50%',
                      transform: 'translateX(-50%)',
                      lineHeight: 1,
                      pointerEvents: 'none',
                      userSelect: 'none',
                    }}
                  >
                    {step.number}
                  </Typography>

                  <Box
                    sx={{
                      width: 56,
                      height: 56,
                      border: '1px solid #E7E5E4',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: '#1C1917',
                      mx: 'auto',
                      mb: 3,
                      mt: 2,
                      transition: 'all 0.3s ease',
                      '&:hover': {
                        borderColor: '#1C1917',
                      },
                    }}
                  >
                    {step.icon}
                  </Box>

                  <Typography
                    sx={{
                      color: '#1C1917',
                      fontWeight: typography.fontWeight.medium,
                      fontFamily: typography.fontFamily.body,
                      mb: 1.5,
                      fontSize: '0.9375rem',
                    }}
                  >
                    {step.title}
                  </Typography>
                  <Typography
                    sx={{
                      color: '#78716C',
                      lineHeight: 1.7,
                      fontSize: '0.8125rem',
                    }}
                  >
                    {step.description}
                  </Typography>
                </Box>
              </motion.div>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  )
}
