import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Box, Typography, IconButton, Chip, Skeleton } from '@mui/material'
import { Favorite, FavoriteBorder, Timer, Verified } from '@mui/icons-material'
import { formatTimeLeft, formatCurrency } from '../utils'

interface AuctionProductCardProps {
  id: string
  title: string
  currentBid: number
  startingPrice?: number
  endTime: string
  bidCount: number
  images: string[]
  seller: {
    name: string
    verified: boolean
  }
  isNew?: boolean
  isFeatured?: boolean
  onFavoriteToggle?: (id: string) => void
  isFavorited?: boolean
}

export const AuctionProductCard = ({
  id,
  title,
  currentBid,
  endTime,
  bidCount,
  images,
  seller,
  isNew,
  isFeatured,
  onFavoriteToggle,
  isFavorited = false,
}: AuctionProductCardProps) => {
  const [currentImageIndex, setCurrentImageIndex] = useState(0)
  const [isImageLoaded, setIsImageLoaded] = useState(false)
  const [isHovered, setIsHovered] = useState(false)
  const [timeLeft, setTimeLeft] = useState('')
  const [isEndingSoon, setIsEndingSoon] = useState(false)

  useEffect(() => {
    const updateTimeLeft = () => {
      const now = Date.now()
      const endMs = new Date(endTime).getTime()
      setTimeLeft(formatTimeLeft(endTime))
      setIsEndingSoon(endMs - now < 24 * 60 * 60 * 1000)
    }
    updateTimeLeft()
    const interval = setInterval(updateTimeLeft, 60000)
    return () => clearInterval(interval)
  }, [endTime])

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (images.length <= 1) return
    const rect = e.currentTarget.getBoundingClientRect()
    const x = e.clientX - rect.left
    const segmentWidth = rect.width / images.length
    const newIndex = Math.min(Math.floor(x / segmentWidth), images.length - 1)
    if (newIndex !== currentImageIndex) {
      setCurrentImageIndex(newIndex)
    }
  }

  const handleMouseLeave = () => {
    setCurrentImageIndex(0)
    setIsHovered(false)
  }

  return (
    <Box
      sx={{
        position: 'relative',
        cursor: 'pointer',
        '&:hover .card-actions': { opacity: 1 },
      }}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={handleMouseLeave}
    >
      <Box
        component={Link}
        to={`/auctions/${id}`}
        sx={{
          display: 'block',
          textDecoration: 'none',
          color: 'inherit',
        }}
      >
        <Box
          sx={{
            position: 'relative',
            aspectRatio: '4/5',
            overflow: 'hidden',
            bgcolor: '#F5F5F4',
            mb: 2,
          }}
          onMouseMove={handleMouseMove}
        >
          {!isImageLoaded && (
            <Skeleton
              variant="rectangular"
              sx={{
                position: 'absolute',
                inset: 0,
                bgcolor: '#E7E5E4',
              }}
              animation="wave"
            />
          )}

          <Box
            component="img"
            src={images[currentImageIndex] || `https://picsum.photos/600/750?random=${id}`}
            alt={title}
            onLoad={() => setIsImageLoaded(true)}
            sx={{
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              opacity: isImageLoaded ? 1 : 0,
              transition: 'opacity 0.3s ease, transform 0.5s ease',
              transform: isHovered ? 'scale(1.03)' : 'scale(1)',
            }}
          />

          {images.length > 1 && (
            <Box
              sx={{
                position: 'absolute',
                bottom: 12,
                left: '50%',
                transform: 'translateX(-50%)',
                display: 'flex',
                gap: 0.5,
              }}
            >
              {images.map((_, index) => (
                <Box
                  key={index}
                  sx={{
                    width: 6,
                    height: 6,
                    borderRadius: '50%',
                    bgcolor: index === currentImageIndex ? '#1C1917' : 'rgba(255,255,255,0.7)',
                    transition: 'background-color 0.2s ease',
                  }}
                />
              ))}
            </Box>
          )}

          <Box
            sx={{
              position: 'absolute',
              top: 12,
              left: 12,
              display: 'flex',
              flexDirection: 'column',
              gap: 1,
            }}
          >
            {isNew && (
              <Chip
                label="NEW"
                size="small"
                sx={{
                  bgcolor: '#1C1917',
                  color: '#FAFAF9',
                  fontWeight: 600,
                  fontSize: '0.65rem',
                  height: 22,
                  letterSpacing: 1,
                }}
              />
            )}
            {isFeatured && (
              <Chip
                label="FEATURED"
                size="small"
                sx={{
                  bgcolor: '#CA8A04',
                  color: '#FAFAF9',
                  fontWeight: 600,
                  fontSize: '0.65rem',
                  height: 22,
                  letterSpacing: 1,
                }}
              />
            )}
          </Box>

          <Chip
            icon={<Timer sx={{ fontSize: 14 }} />}
            label={timeLeft}
            size="small"
            sx={{
              position: 'absolute',
              bottom: 12,
              left: 12,
              bgcolor: isEndingSoon ? '#DC2626' : 'rgba(28,25,23,0.85)',
              color: '#FAFAF9',
              fontWeight: 500,
              fontSize: '0.75rem',
              '& .MuiChip-icon': { color: isEndingSoon ? '#FAFAF9' : '#CA8A04' },
              animation: isEndingSoon ? 'pulse 2s infinite' : 'none',
              '@keyframes pulse': {
                '0%, 100%': { opacity: 1 },
                '50%': { opacity: 0.7 },
              },
            }}
          />
        </Box>

        <Box sx={{ px: 0.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 0.75 }}>
            <Typography
              variant="caption"
              sx={{
                color: '#78716C',
                fontSize: '0.7rem',
                textTransform: 'uppercase',
                letterSpacing: 0.5,
              }}
            >
              {seller.name}
            </Typography>
            {seller.verified && <Verified sx={{ fontSize: 12, color: '#CA8A04' }} />}
          </Box>

          <Typography
            variant="body1"
            sx={{
              color: '#1C1917',
              fontWeight: 400,
              fontSize: '0.95rem',
              lineHeight: 1.4,
              mb: 1.5,
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              minHeight: '2.6em',
              transition: 'color 0.2s ease',
              '&:hover': { color: '#CA8A04' },
            }}
          >
            {title}
          </Typography>

          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
            <Box>
              <Typography
                variant="caption"
                sx={{
                  color: '#78716C',
                  fontSize: '0.65rem',
                  textTransform: 'uppercase',
                  letterSpacing: 0.5,
                  display: 'block',
                  mb: 0.25,
                }}
              >
                Current Bid
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  color: '#1C1917',
                  fontWeight: 600,
                  fontSize: '1.1rem',
                  fontFamily: '"Playfair Display", serif',
                }}
              >
                {formatCurrency(currentBid)}
              </Typography>
            </Box>
            <Typography
              variant="caption"
              sx={{
                color: '#78716C',
                fontSize: '0.75rem',
              }}
            >
              {bidCount} {bidCount === 1 ? 'bid' : 'bids'}
            </Typography>
          </Box>
        </Box>
      </Box>

      <IconButton
        className="card-actions"
        onClick={(e) => {
          e.preventDefault()
          e.stopPropagation()
          onFavoriteToggle?.(id)
        }}
        sx={{
          position: 'absolute',
          top: 12,
          right: 12,
          bgcolor: 'rgba(255,255,255,0.9)',
          opacity: { xs: 1, md: 0 },
          transition: 'opacity 0.2s ease, background-color 0.2s ease',
          '&:hover': {
            bgcolor: '#FFFFFF',
          },
        }}
      >
        {isFavorited ? (
          <Favorite sx={{ color: '#DC2626', fontSize: 20 }} />
        ) : (
          <FavoriteBorder sx={{ color: '#44403C', fontSize: 20 }} />
        )}
      </IconButton>
    </Box>
  )
}

export const AuctionProductCardSkeleton = () => (
  <Box>
    <Skeleton
      variant="rectangular"
      sx={{ aspectRatio: '4/5', mb: 2, bgcolor: '#E7E5E4' }}
      animation="wave"
    />
    <Skeleton variant="text" sx={{ width: '40%', height: 16, mb: 0.5 }} animation="wave" />
    <Skeleton variant="text" sx={{ width: '90%', height: 20, mb: 0.5 }} animation="wave" />
    <Skeleton variant="text" sx={{ width: '70%', height: 20, mb: 1.5 }} animation="wave" />
    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
      <Box>
        <Skeleton variant="text" sx={{ width: 60, height: 12 }} animation="wave" />
        <Skeleton variant="text" sx={{ width: 80, height: 24 }} animation="wave" />
      </Box>
      <Skeleton variant="text" sx={{ width: 40, height: 16 }} animation="wave" />
    </Box>
  </Box>
)
