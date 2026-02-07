import { useState, useEffect, useMemo } from 'react'
import { Box, Typography, IconButton, Container } from '@mui/material'
import { Close, ChevronLeft, ChevronRight } from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { palette } from '@/shared/theme/tokens'
import { useFeaturedAuctions } from '@/modules/auctions/hooks/useAuctions'

interface Promotion {
  id: string
  text: string
  link?: string
  linkText?: string
  bgColor?: string
}

interface PromoBannerProps {
  promotions?: Promotion[]
  autoRotate?: boolean
  rotateInterval?: number
}

export const PromoBanner = ({
  promotions,
  autoRotate = true,
  rotateInterval = 5000,
}: PromoBannerProps) => {
  const { data: featuredData } = useFeaturedAuctions(3)

  const apiPromotions = useMemo(() => {
    const items = featuredData?.items ?? []
    const colors = [palette.neutral[900], palette.brand.primary, palette.neutral[800]]
    return items.map((auction, index) => ({
      id: auction.id,
      text: `Live Now: ${auction.title}`,
      link: `/auctions/${auction.id}`,
      linkText: 'View Auction',
      bgColor: colors[index % colors.length],
    }))
  }, [featuredData?.items])

  const resolvedPromotions = promotions ?? apiPromotions

  const [currentIndex, setCurrentIndex] = useState(0)
  const [isVisible, setIsVisible] = useState(true)
  const [isPaused, setIsPaused] = useState(false)

  useEffect(() => {
    if (!autoRotate || isPaused || resolvedPromotions.length <= 1) {return}

    const interval = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % resolvedPromotions.length)
    }, rotateInterval)

    return () => clearInterval(interval)
  }, [autoRotate, isPaused, resolvedPromotions.length, rotateInterval])

  useEffect(() => {
    if (currentIndex >= resolvedPromotions.length) {
      setCurrentIndex(0)
    }
  }, [currentIndex, resolvedPromotions.length])

  const handlePrev = () => {
    setCurrentIndex((prev) => (prev - 1 + resolvedPromotions.length) % resolvedPromotions.length)
  }

  const handleNext = () => {
    setCurrentIndex((prev) => (prev + 1) % resolvedPromotions.length)
  }

  if (!isVisible || resolvedPromotions.length === 0) {return null}

  const currentPromo = resolvedPromotions[currentIndex]

  return (
    <Box
      sx={{
        bgcolor: currentPromo.bgColor || palette.neutral[900],
        color: palette.neutral[50],
        py: 1.25,
        position: 'relative',
      }}
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
    >
      <Container maxWidth="xl">
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 2,
            position: 'relative',
          }}
        >
          {resolvedPromotions.length > 1 && (
            <IconButton
              size="small"
              onClick={handlePrev}
              sx={{
                color: palette.neutral[50],
                opacity: 0.6,
                p: 0.5,
                '&:hover': { opacity: 1, bgcolor: 'transparent' },
              }}
            >
              <ChevronLeft fontSize="small" />
            </IconButton>
          )}

          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 1.5,
              minHeight: 24,
            }}
          >
            <Typography
              variant="body2"
              sx={{
                fontSize: '0.8rem',
                fontWeight: 400,
                letterSpacing: 0.5,
                textAlign: 'center',
              }}
            >
              {currentPromo.text}
            </Typography>

            {currentPromo.link && currentPromo.linkText && (
              <Typography
                component={Link}
                to={currentPromo.link}
                sx={{
                  color: palette.brand.primary,
                  fontSize: '0.8rem',
                  fontWeight: 500,
                  textDecoration: 'underline',
                  textUnderlineOffset: 2,
                  whiteSpace: 'nowrap',
                  '&:hover': {
                    color: palette.brand.secondary,
                  },
                }}
              >
                {currentPromo.linkText}
              </Typography>
            )}
          </Box>

          {resolvedPromotions.length > 1 && (
            <IconButton
              size="small"
              onClick={handleNext}
              sx={{
                color: palette.neutral[50],
                opacity: 0.6,
                p: 0.5,
                '&:hover': { opacity: 1, bgcolor: 'transparent' },
              }}
            >
              <ChevronRight fontSize="small" />
            </IconButton>
          )}

          <IconButton
            size="small"
            onClick={() => setIsVisible(false)}
            sx={{
              position: 'absolute',
              right: 0,
              color: palette.neutral[50],
              opacity: 0.5,
              p: 0.5,
              '&:hover': { opacity: 1, bgcolor: 'transparent' },
            }}
          >
            <Close sx={{ fontSize: 18 }} />
          </IconButton>
        </Box>

        {resolvedPromotions.length > 1 && (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              gap: 0.75,
              mt: 1,
            }}
          >
            {resolvedPromotions.map((promo, index) => (
              <Box
                key={promo.id}
                onClick={() => setCurrentIndex(index)}
                sx={{
                  width: 5,
                  height: 5,
                  borderRadius: '50%',
                  bgcolor: index === currentIndex ? palette.brand.primary : 'rgba(250,250,249,0.3)',
                  cursor: 'pointer',
                  transition: 'background-color 0.2s ease',
                }}
              />
            ))}
          </Box>
        )}
      </Container>
    </Box>
  )
}
