import { useMemo, useRef } from 'react'
import { Link } from 'react-router-dom'
import { motion, useScroll, useTransform } from 'framer-motion'
import { Box, Container, Typography, Button, Stack } from '@mui/material'
import { East } from '@mui/icons-material'
import { HeroStats } from './HeroStats'
import { useHomeMetrics } from '../../hooks/useHomeMetrics'
import { typography } from '@/shared/theme/tokens'

export const HeroSection = () => {
  const heroRef = useRef<HTMLDivElement>(null)
  const { scrollYProgress } = useScroll({ target: heroRef, offset: ['start start', 'end start'] })
  const heroOpacity = useTransform(scrollYProgress, [0, 0.8], [1, 0])
  const heroY = useTransform(scrollYProgress, [0, 1], [0, 80])
  const { activeAuctionsCount, categoriesCount, featuredBrandsCount } = useHomeMetrics()

  const stats = useMemo(
    () => [
      { label: 'Live Auctions', value: activeAuctionsCount, suffix: '+' },
      { label: 'Categories', value: categoriesCount },
      { label: 'Trusted Brands', value: featuredBrandsCount, suffix: '+' },
    ],
    [activeAuctionsCount, categoriesCount, featuredBrandsCount]
  )

  const handleScrollDown = () => {
    window.scrollTo({ top: window.innerHeight, behavior: 'smooth' })
  }

  const headerHeight = {
    xs: 56,
    md: 32 + 64 + 44,
  }

  return (
    <Box
      ref={heroRef}
      sx={{
        position: 'relative',
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        overflow: 'hidden',
        bgcolor: '#1C1917',
        pt: { xs: `${headerHeight.xs}px`, md: `${headerHeight.md}px` },
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          backgroundImage: 'url(https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=1920&q=80)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          opacity: 0.35,
        }}
      />

      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          background: 'linear-gradient(to bottom, rgba(28,25,23,0.3) 0%, rgba(28,25,23,0.6) 100%)',
        }}
      />

      <Container maxWidth="xl" sx={{ position: 'relative', zIndex: 1 }}>
        <motion.div style={{ opacity: heroOpacity, y: heroY }}>
          <Box
            sx={{
              maxWidth: 720,
              mx: { xs: 0, md: 'auto' },
              textAlign: { xs: 'left', md: 'center' },
              py: { xs: 4, md: 8 },
            }}
          >
            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
            >
              <Typography
                sx={{
                  color: 'rgba(255,255,255,0.6)',
                  fontWeight: typography.fontWeight.medium,
                  letterSpacing: '0.2em',
                  fontSize: '0.6875rem',
                  textTransform: 'uppercase',
                  mb: 4,
                }}
              >
                Authenticated Luxury Consignment
              </Typography>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.9, delay: 0.1, ease: [0.16, 1, 0.3, 1] }}
            >
              <Typography
                variant="h1"
                sx={{
                  fontFamily: typography.fontFamily.display,
                  fontSize: { xs: '2.5rem', sm: '3.25rem', md: '4rem', lg: '4.75rem' },
                  fontWeight: typography.fontWeight.regular,
                  lineHeight: 1.1,
                  color: '#FFFFFF',
                  mb: 4,
                  letterSpacing: '-0.02em',
                }}
              >
                The Art of
                <br />
                Collecting
              </Typography>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.8, delay: 0.3, ease: [0.16, 1, 0.3, 1] }}
            >
              <Typography
                sx={{
                  fontFamily: typography.fontFamily.body,
                  color: 'rgba(255,255,255,0.7)',
                  fontWeight: typography.fontWeight.regular,
                  lineHeight: 1.7,
                  mb: 6,
                  maxWidth: 520,
                  mx: { xs: 0, md: 'auto' },
                  fontSize: { xs: '0.9rem', md: '1rem' },
                }}
              >
                A curated marketplace where extraordinary meets exceptional.
                Discover rare timepieces, masterpiece artworks, and vintage
                collectibles — all authenticated by world-class experts.
              </Typography>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.45, ease: [0.16, 1, 0.3, 1] }}
            >
              <Stack
                direction={{ xs: 'column', sm: 'row' }}
                spacing={2}
                justifyContent={{ xs: 'flex-start', md: 'center' }}
                sx={{ mb: 10 }}
              >
                <Button
                  variant="contained"
                  size="large"
                  endIcon={<East />}
                  component={Link}
                  to="/auctions"
                  sx={{
                    bgcolor: '#FFFFFF',
                    color: '#1C1917',
                    px: 5,
                    py: 1.75,
                    fontSize: '0.8125rem',
                    fontWeight: typography.fontWeight.medium,
                    fontFamily: typography.fontFamily.body,
                    textTransform: 'uppercase',
                    letterSpacing: '0.1em',
                    borderRadius: 0,
                    boxShadow: 'none',
                    '&:hover': {
                      bgcolor: '#F5F5F4',
                      boxShadow: 'none',
                    },
                  }}
                >
                  Explore Auctions
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  component={Link}
                  to="/how-it-works"
                  sx={{
                    borderColor: 'rgba(255,255,255,0.3)',
                    color: '#FFFFFF',
                    px: 5,
                    py: 1.75,
                    fontSize: '0.8125rem',
                    fontWeight: typography.fontWeight.medium,
                    fontFamily: typography.fontFamily.body,
                    textTransform: 'uppercase',
                    letterSpacing: '0.1em',
                    borderRadius: 0,
                    '&:hover': {
                      borderColor: '#FFFFFF',
                      bgcolor: 'rgba(255,255,255,0.05)',
                    },
                  }}
                >
                  How It Works
                </Button>
              </Stack>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.6, ease: [0.16, 1, 0.3, 1] }}
            >
              <Box sx={{ display: 'flex', justifyContent: { xs: 'flex-start', md: 'center' } }}>
                <HeroStats stats={stats} />
              </Box>
            </motion.div>
          </Box>
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
          zIndex: 2,
        }}
        onClick={handleScrollDown}
      >
        <motion.div
          animate={{ y: [0, 6, 0] }}
          transition={{ duration: 2, repeat: Infinity, ease: 'easeInOut' }}
        >
          <Box
            sx={{
              width: 28,
              height: 44,
              border: '1px solid rgba(255,255,255,0.25)',
              borderRadius: 14,
              display: 'flex',
              justifyContent: 'center',
              pt: 1.5,
            }}
          >
            <motion.div
              animate={{ y: [0, 12, 0], opacity: [1, 0.3, 1] }}
              transition={{ duration: 2, repeat: Infinity, ease: 'easeInOut' }}
            >
              <Box
                sx={{
                  width: 2,
                  height: 8,
                  bgcolor: 'rgba(255,255,255,0.6)',
                  borderRadius: 4,
                }}
              />
            </motion.div>
          </Box>
        </motion.div>
      </Box>
    </Box>
  )
}
