import { useState, useEffect } from 'react'
import { Box, Typography, IconButton, Container } from '@mui/material'
import { Close, ChevronLeft, ChevronRight } from '@mui/icons-material'
import { Link } from 'react-router-dom'

interface Promotion {
  id: string
  text: string
  link?: string
  linkText?: string
  bgColor?: string
}

const defaultPromotions: Promotion[] = [
  {
    id: '1',
    text: 'New Spring Arrivals',
    link: '/auctions?sort=new',
    linkText: 'Shop Now',
  },
  {
    id: '2',
    text: 'Free Authentication on Orders Over $1,000',
    link: '/how-it-works',
    linkText: 'Learn More',
  },
  {
    id: '3',
    text: 'Ending Soon: Rare Art Collection',
    link: '/collections/rare-art',
    linkText: 'View Collection',
  },
]

interface PromoBannerProps {
  promotions?: Promotion[]
  autoRotate?: boolean
  rotateInterval?: number
}

export const PromoBanner = ({
  promotions = defaultPromotions,
  autoRotate = true,
  rotateInterval = 5000,
}: PromoBannerProps) => {
  const [currentIndex, setCurrentIndex] = useState(0)
  const [isVisible, setIsVisible] = useState(true)
  const [isPaused, setIsPaused] = useState(false)

  useEffect(() => {
    if (!autoRotate || isPaused || promotions.length <= 1) return

    const interval = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % promotions.length)
    }, rotateInterval)

    return () => clearInterval(interval)
  }, [autoRotate, isPaused, promotions.length, rotateInterval])

  const handlePrev = () => {
    setCurrentIndex((prev) => (prev - 1 + promotions.length) % promotions.length)
  }

  const handleNext = () => {
    setCurrentIndex((prev) => (prev + 1) % promotions.length)
  }

  if (!isVisible || promotions.length === 0) return null

  const currentPromo = promotions[currentIndex]

  return (
    <Box
      sx={{
        bgcolor: currentPromo.bgColor || '#1C1917',
        color: '#FAFAF9',
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
          {promotions.length > 1 && (
            <IconButton
              size="small"
              onClick={handlePrev}
              sx={{
                color: '#FAFAF9',
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
                  color: '#CA8A04',
                  fontSize: '0.8rem',
                  fontWeight: 500,
                  textDecoration: 'underline',
                  textUnderlineOffset: 2,
                  whiteSpace: 'nowrap',
                  '&:hover': {
                    color: '#EAB308',
                  },
                }}
              >
                {currentPromo.linkText}
              </Typography>
            )}
          </Box>

          {promotions.length > 1 && (
            <IconButton
              size="small"
              onClick={handleNext}
              sx={{
                color: '#FAFAF9',
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
              color: '#FAFAF9',
              opacity: 0.5,
              p: 0.5,
              '&:hover': { opacity: 1, bgcolor: 'transparent' },
            }}
          >
            <Close sx={{ fontSize: 18 }} />
          </IconButton>
        </Box>

        {promotions.length > 1 && (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              gap: 0.75,
              mt: 1,
            }}
          >
            {promotions.map((_, index) => (
              <Box
                key={index}
                onClick={() => setCurrentIndex(index)}
                sx={{
                  width: 5,
                  height: 5,
                  borderRadius: '50%',
                  bgcolor: index === currentIndex ? '#CA8A04' : 'rgba(250,250,249,0.3)',
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
