import { useRef } from 'react'
import { Link } from 'react-router-dom'
import { motion, useScroll, useTransform } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { PlayArrow, East } from '@mui/icons-material'
import { GlassCard } from '../shared'
import { HeroStats } from './HeroStats'
import { FloatingCards } from './FloatingCards'
import { LIVE_STATS } from '../../data'
import { colors, gradients, typography, shadows, transitions } from '@/shared/theme/tokens'

export const HeroSection = () => {
  const heroRef = useRef<HTMLDivElement>(null)
  const { scrollYProgress } = useScroll({ target: heroRef, offset: ['start start', 'end start'] })
  const heroOpacity = useTransform(scrollYProgress, [0, 1], [1, 0])
  const heroScale = useTransform(scrollYProgress, [0, 1], [1, 0.95])

  const handleScrollDown = () => {
    window.scrollTo({ top: window.innerHeight, behavior: 'smooth' })
  }

  return (
    <Box
      ref={heroRef}
      sx={{
        position: 'relative',
        minHeight: 'calc(100vh - 50px)',
        display: 'flex',
        alignItems: 'center',
        overflow: 'hidden',
        pt: { xs: '80px', md: '72px' },
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          background: gradients.radial.gold,
        }}
      />
      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          background: gradients.radial.purple,
        }}
      />

      <Box
        sx={{
          position: 'absolute',
          top: '10%',
          right: '5%',
          width: 400,
          height: 400,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${colors.gold.primary}14 0%, transparent 70%)`,
          filter: 'blur(40px)',
        }}
      />
      <Box
        sx={{
          position: 'absolute',
          bottom: '20%',
          left: '10%',
          width: 300,
          height: 300,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${colors.purple.primary}14 0%, transparent 70%)`,
          filter: 'blur(40px)',
        }}
      />

      <Container maxWidth="xl" sx={{ position: 'relative', zIndex: 1 }}>
        <motion.div style={{ opacity: heroOpacity, scale: heroScale }}>
          <Grid container spacing={6} alignItems="center">
            <Grid size={{ xs: 12, lg: 6 }}>
              <motion.div
                initial={{ opacity: 0, y: 40 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: transitions.duration.slow, ease: [0.16, 1, 0.3, 1] }}
              >
                <GlassCard
                  sx={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: 1,
                    px: 2,
                    py: 0.75,
                    mb: 4,
                  }}
                >
                  <Box
                    sx={{
                      width: 8,
                      height: 8,
                      borderRadius: '50%',
                      bgcolor: colors.success,
                      animation: 'pulse 2s infinite',
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{
                      color: colors.text.muted,
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: 1,
                    }}
                  >
                    {LIVE_STATS[1].value} LIVE AUCTIONS NOW
                  </Typography>
                </GlassCard>

                <Typography
                  variant="h1"
                  sx={{
                    fontFamily: typography.fontFamily.display,
                    fontSize: { xs: '3rem', sm: '4rem', md: '5rem', lg: '6rem' },
                    fontWeight: typography.fontWeight.light,
                    lineHeight: 1,
                    color: colors.text.primary,
                    mb: 3,
                    letterSpacing: '-0.02em',
                  }}
                >
                  Where Rarity
                  <Box
                    component="span"
                    sx={{
                      display: 'block',
                      fontWeight: typography.fontWeight.semibold,
                      background: gradients.gold,
                      backgroundClip: 'text',
                      WebkitBackgroundClip: 'text',
                      WebkitTextFillColor: 'transparent',
                      backgroundSize: '200% auto',
                      animation: 'shimmer 3s linear infinite',
                      '@keyframes shimmer': {
                        '0%': { backgroundPosition: '0% center' },
                        '100%': { backgroundPosition: '200% center' },
                      },
                    }}
                  >
                    Meets Desire
                  </Box>
                </Typography>

                <Typography
                  variant="h6"
                  sx={{
                    fontFamily: typography.fontFamily.body,
                    color: colors.text.subtle,
                    fontWeight: typography.fontWeight.regular,
                    lineHeight: 1.7,
                    mb: 5,
                    maxWidth: 480,
                    fontSize: { xs: '1rem', md: '1.125rem' },
                  }}
                >
                  Curated auctions for discerning collectors. From masterpiece artworks to rare
                  timepieces, discover extraordinary pieces authenticated by experts.
                </Typography>

                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 6 }}>
                  <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                    <Button
                      variant="contained"
                      size="large"
                      endIcon={<East />}
                      component={Link}
                      to="/auctions"
                      sx={{
                        background: gradients.goldButton,
                        color: colors.background.primary,
                        px: 4,
                        py: 2,
                        fontSize: '1rem',
                        fontWeight: typography.fontWeight.semibold,
                        fontFamily: typography.fontFamily.body,
                        textTransform: 'none',
                        borderRadius: 2,
                        boxShadow: shadows.gold,
                        '&:hover': {
                          background: gradients.goldButtonHover,
                          boxShadow: shadows.goldHover,
                        },
                      }}
                    >
                      Explore Auctions
                    </Button>
                  </motion.div>
                  <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                    <Button
                      variant="outlined"
                      size="large"
                      startIcon={<PlayArrow />}
                      sx={{
                        borderColor: colors.border.light,
                        color: colors.text.primary,
                        px: 4,
                        py: 2,
                        fontSize: '1rem',
                        fontWeight: typography.fontWeight.medium,
                        fontFamily: typography.fontFamily.body,
                        textTransform: 'none',
                        borderRadius: 2,
                        backdropFilter: 'blur(10px)',
                        '&:hover': {
                          borderColor: colors.border.medium,
                          bgcolor: colors.background.glass,
                        },
                      }}
                    >
                      Watch How It Works
                    </Button>
                  </motion.div>
                </Box>

                <HeroStats stats={LIVE_STATS} />
              </motion.div>
            </Grid>

            <Grid size={{ xs: 12, lg: 6 }} sx={{ display: { xs: 'none', lg: 'block' } }}>
              <FloatingCards />
            </Grid>
          </Grid>
        </motion.div>
      </Container>

      <Box
        sx={{
          position: 'absolute',
          bottom: 40,
          left: '50%',
          transform: 'translateX(-50%)',
          display: { xs: 'none', md: 'flex' },
          flexDirection: 'column',
          alignItems: 'center',
          cursor: 'pointer',
        }}
        onClick={handleScrollDown}
      >
        <motion.div animate={{ y: [0, 8, 0] }} transition={{ duration: 1.5, repeat: Infinity }}>
          <Box
            sx={{
              width: 28,
              height: 44,
              border: '2px solid rgba(255,255,255,0.3)',
              borderRadius: 14,
              display: 'flex',
              justifyContent: 'center',
              pt: 1,
            }}
          >
            <motion.div
              animate={{ y: [0, 12, 0], opacity: [1, 0, 1] }}
              transition={{ duration: 1.5, repeat: Infinity }}
            >
              <Box sx={{ width: 4, height: 8, bgcolor: colors.gold.primary, borderRadius: 2 }} />
            </motion.div>
          </Box>
        </motion.div>
      </Box>
    </Box>
  )
}
