import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { Shield, Gavel, LocalShipping, TrendingUp } from '@mui/icons-material'
import { GlassCard } from '../shared'
import { PLATFORM_FEATURES } from '../../data'
import { colors, typography, transitions } from '@/shared/theme/tokens'

const iconMap = {
  Shield: <Shield sx={{ fontSize: 32 }} />,
  Gavel: <Gavel sx={{ fontSize: 32 }} />,
  LocalShipping: <LocalShipping sx={{ fontSize: 32 }} />,
  TrendingUp: <TrendingUp sx={{ fontSize: 32 }} />,
}

export const FeaturesSection = () => {
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
          {PLATFORM_FEATURES.map((feature, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={index}>
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
                    {iconMap[feature.icon as keyof typeof iconMap]}
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
