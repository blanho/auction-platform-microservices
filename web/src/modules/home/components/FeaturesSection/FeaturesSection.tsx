import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { VerifiedUser, Speed, Visibility, AccountBalanceWallet } from '@mui/icons-material'
import { useMemo } from 'react'
import { typography } from '@/shared/theme/tokens'

const featureData = [
  {
    key: 'authenticated',
    icon: <VerifiedUser sx={{ fontSize: 28 }} />,
    title: 'Expert Authentication',
    description: 'Every item verified by certified specialists before listing. Bid with absolute confidence.',
  },
  {
    key: 'realtime',
    icon: <Speed sx={{ fontSize: 28 }} />,
    title: 'Real-Time Bidding',
    description: 'Millisecond-precise auction engine. Never miss a moment with instant bid updates.',
  },
  {
    key: 'transparent',
    icon: <Visibility sx={{ fontSize: 28 }} />,
    title: 'Full Transparency',
    description: 'Complete bid history, provenance tracking, and condition reports on every piece.',
  },
  {
    key: 'secure',
    icon: <AccountBalanceWallet sx={{ fontSize: 28 }} />,
    title: 'Secure Payments',
    description: 'Escrow protection on every transaction. Your investment is safeguarded end-to-end.',
  },
]

export const FeaturesSection = () => {
  const features = useMemo(() => featureData, [])

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FAFAF9', position: 'relative' }}>
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
              Why Choose Us
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
              Built for Discerning Collectors
            </Typography>
            <Typography
              sx={{
                color: '#78716C',
                maxWidth: 520,
                mx: 'auto',
                fontSize: '0.9rem',
                lineHeight: 1.7,
              }}
            >
              Every detail of our platform is crafted to deliver a premium auction experience
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={4}>
          {features.map((feature, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={feature.key}>
              <motion.div
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true, margin: '-50px' }}
                transition={{ delay: index * 0.1, duration: 0.5 }}
              >
                <Box
                  sx={{
                    p: 4,
                    height: '100%',
                    textAlign: 'center',
                    borderTop: '1px solid #E7E5E4',
                    transition: 'border-color 0.3s ease',
                    '&:hover': {
                      borderTopColor: '#1C1917',
                    },
                  }}
                >
                  <Box
                    sx={{
                      width: 56,
                      height: 56,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: '#1C1917',
                      mx: 'auto',
                      mb: 3,
                    }}
                  >
                    {feature.icon}
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
                    {feature.title}
                  </Typography>
                  <Typography
                    sx={{
                      color: '#78716C',
                      lineHeight: 1.7,
                      fontSize: '0.8125rem',
                    }}
                  >
                    {feature.description}
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
