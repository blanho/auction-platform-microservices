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

  const safeCurrentIndex = currentIndex >= resolvedPromotions.length ? 0 : currentIndex

  const handlePrev = () => {
    setCurrentIndex((prev) => (prev - 1 + resolvedPromotions.length) % resolvedPromotions.length)
  }

  const handleNext = () => {
    setCurrentIndex((prev) => (prev + 1) % resolvedPromotions.length)
  }

  if (!isVisible) {
    return null
  }

  const hasPromotions = resolvedPromotions.length > 0
  
  if (!hasPromotions) {
    return null
  }

  const currentPromo = resolvedPromotions[safeCurrentIndex]
  const displayText = currentPromo.text
  const displayLink = currentPromo.link
  const displayLinkText = currentPromo.linkText || 'Shop Now'
  const displayBgColor = currentPromo.bgColor || palette.neutral[900]

  return (
    <Box
      sx={{
        bgcolor: displayBgColor,
        color: palette.neutral[50],
        py: 1.25,
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 1201,
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
          {hasPromotions && resolvedPromotions.length > 1 && (
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
              {displayText}
            </Typography>

            {displayLink && (
              <Typography
                component={Link}
                to={displayLink}
                sx={{
                  color: palette.neutral[50],
                  fontSize: '0.8rem',
                  fontWeight: 500,
                  textDecoration: 'underline',
                  textUnderlineOffset: 2,
                  whiteSpace: 'nowrap',
                  '&:hover': {
                    color: palette.neutral[200],
                  },
                }}
              >
                {displayLinkText}
              </Typography>
            )}
          </Box>

          {hasPromotions && resolvedPromotions.length > 1 && (
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

        {hasPromotions && resolvedPromotions.length > 1 && (
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
                  bgcolor: index === safeCurrentIndex ? palette.neutral[50] : 'rgba(250,250,249,0.3)',
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
