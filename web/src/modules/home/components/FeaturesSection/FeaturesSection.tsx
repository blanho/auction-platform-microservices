import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { Shield, Gavel, LocalShipping, TrendingUp } from '@mui/icons-material'
import { GlassCard } from '@/shared/components/ui'
import { useMemo } from 'react'
import { useHomeMetrics } from '../../hooks/useHomeMetrics'
import { colors, typography, transitions } from '@/shared/theme/tokens'

export const FeaturesSection = () => {
  const { totalAuctionsCount, categoriesCount, activeAuctionsCount, brandsCount } = useHomeMetrics()

  const features = useMemo(
    () => [
      {
        key: 'trust',
        icon: <Shield sx={{ fontSize: 32 }} />,
        title: 'Trusted Marketplace',
        description: `${brandsCount} verified brands available for bidding.`,
        gradient: 'linear-gradient(135deg, #3B82F6 0%, #8B5CF6 100%)',
      },
      {
        key: 'live',
        icon: <Gavel sx={{ fontSize: 32 }} />,
        title: 'Live Auctions',
        description: `${activeAuctionsCount} auctions live right now.`,
        gradient: 'linear-gradient(135deg, #22C55E 0%, #10B981 100%)',
      },
      {
        key: 'catalog',
        icon: <LocalShipping sx={{ fontSize: 32 }} />,
        title: 'Curated Catalog',
        description: `${categoriesCount} categories across the marketplace.`,
        gradient: 'linear-gradient(135deg, #F59E0B 0%, #EF4444 100%)',
      },
      {
        key: 'growth',
        icon: <TrendingUp sx={{ fontSize: 32 }} />,
        title: 'Marketplace Growth',
        description: `${totalAuctionsCount} total auctions listed.`,
        gradient: 'linear-gradient(135deg, #EC4899 0%, #8B5CF6 100%)',
      },
    ],
    [activeAuctionsCount, brandsCount, categoriesCount, totalAuctionsCount]
  )

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: colors.background.primary, position: 'relative' }}>
      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: transitions.duration.normal }}
        >
          <Box sx={{ textAlign: 'center', mb: 8 }}>
            <Typography
              variant="overline"
              sx={{
                color: colors.gold.primary,
                letterSpacing: 4,
                display: 'block',
                mb: 1,
                fontFamily: typography.fontFamily.body,
              }}
            >
              WHY CHOOSE US
            </Typography>
            <Typography
              variant="h2"
              sx={{
                fontFamily: typography.fontFamily.display,
                color: colors.text.primary,
                fontWeight: typography.fontWeight.regular,
                fontSize: { xs: '2rem', md: '3rem' },
              }}
            >
              The Premier Auction Experience
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={4}>
          {features.map((feature, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={feature.key}>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1, duration: transitions.duration.normal }}
              >
                <GlassCard
                  sx={{
                    p: 4,
                    height: '100%',
                    textAlign: 'center',
                    cursor: 'pointer',
                    transition: `transform ${transitions.duration.normal}s ease`,
                    '&:hover': { transform: 'translateY(-8px)' },
                    '&:hover .feature-icon': { transform: 'scale(1.1)' },
                  }}
                >
                  <Box
                    className="feature-icon"
                    sx={{
                      width: 64,
                      height: 64,
                      borderRadius: 2,
                      background: feature.gradient,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: colors.text.primary,
                      mx: 'auto',
                      mb: 3,
                      transition: `transform ${transitions.duration.normal}s ease`,
                    }}
                  >
                    {feature.icon}
                  </Box>
                  <Typography
                    variant="h6"
                    sx={{
                      color: colors.text.primary,
                      fontWeight: typography.fontWeight.semibold,
                      fontFamily: typography.fontFamily.body,
                      mb: 1.5,
                    }}
                  >
                    {feature.title}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{
                      color: colors.text.subtle,
                      lineHeight: 1.7,
                    }}
                  >
                    {feature.description}
                  </Typography>
                </GlassCard>
              </motion.div>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  )
}
