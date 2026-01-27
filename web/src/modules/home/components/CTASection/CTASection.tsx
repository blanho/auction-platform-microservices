import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button } from '@mui/material'
import { GlassCard } from '../shared'
import { colors, gradients, typography, transitions } from '@/shared/theme/tokens'

export const CTASection = () => {
  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: colors.background.primary }}>
      <Container maxWidth="md">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          transition={{ duration: transitions.duration.normal }}
        >
          <GlassCard
            sx={{
              p: { xs: 4, md: 8 },
              textAlign: 'center',
              background: gradients.cardOverlay,
              border: `1px solid ${colors.gold.primary}33`,
            }}
          >
            <Typography
              variant="h3"
              sx={{
                fontFamily: typography.fontFamily.display,
                color: colors.text.primary,
                fontWeight: typography.fontWeight.regular,
                mb: 2,
                fontSize: { xs: '2rem', md: '2.5rem' },
              }}
            >
              Ready to Discover Your Next Treasure?
            </Typography>
            <Typography
              variant="body1"
              sx={{
                color: colors.text.subtle,
                mb: 5,
                maxWidth: 480,
                mx: 'auto',
              }}
            >
              Join 50,000+ collectors and start bidding on extraordinary pieces today.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Button
                  variant="contained"
                  size="large"
                  component={Link}
                  to="/register"
                  sx={{
                    background: gradients.goldButton,
                    color: colors.background.primary,
                    px: 5,
                    py: 2,
                    fontSize: '1rem',
                    fontWeight: typography.fontWeight.semibold,
                    fontFamily: typography.fontFamily.body,
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': {
                      background: gradients.goldButtonHover,
                    },
                  }}
                >
                  Create Free Account
                </Button>
              </motion.div>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Button
                  variant="outlined"
                  size="large"
                  component={Link}
                  to="/auctions"
                  sx={{
                    borderColor: colors.border.medium,
                    color: colors.text.primary,
                    px: 5,
                    py: 2,
                    fontSize: '1rem',
                    fontWeight: typography.fontWeight.medium,
                    fontFamily: typography.fontFamily.body,
                    textTransform: 'none',
                    borderRadius: 2,
                    '&:hover': {
                      borderColor: colors.border.strong,
                      bgcolor: colors.background.glass,
                    },
                  }}
                >
                  Browse Auctions
                </Button>
              </motion.div>
            </Box>
          </GlassCard>
        </motion.div>
      </Container>
    </Box>
  )
}
