import { useState, useCallback, useEffect } from 'react'
import {
  Box,
  IconButton,
  Stack,
  Skeleton,
  Modal,
  Fade,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import {
  ChevronLeft,
  ChevronRight,
  Close,
  ZoomIn,
  Favorite,
  FavoriteBorder,
  Share,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { AuctionImage } from '../types'

interface ImageGalleryProps {
  images: AuctionImage[]
  title: string
  isFavorite?: boolean
  onToggleFavorite?: () => void
  onShare?: () => void
}

export function ImageGallery({
  images,
  title,
  isFavorite = false,
  onToggleFavorite,
  onShare,
}: ImageGalleryProps) {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [isLightboxOpen, setIsLightboxOpen] = useState(false)
  const [isZoomed, setIsZoomed] = useState(false)
  const [zoomPosition, setZoomPosition] = useState({ x: 50, y: 50 })

  const selectedImage = images[selectedIndex] || images[0]

  const handlePrevious = useCallback(() => {
    setSelectedIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1))
  }, [images.length])

  const handleNext = useCallback(() => {
    setSelectedIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1))
  }, [images.length])

  const handleThumbnailClick = (index: number) => {
    setSelectedIndex(index)
  }

  const handleMainImageClick = () => {
    if (!isMobile) {
      setIsLightboxOpen(true)
    }
  }

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!isZoomed) {return}
    const rect = e.currentTarget.getBoundingClientRect()
    const x = ((e.clientX - rect.left) / rect.width) * 100
    const y = ((e.clientY - rect.top) / rect.height) * 100
    setZoomPosition({ x, y })
  }

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (isLightboxOpen) {
        if (e.key === 'ArrowLeft') {handlePrevious()}
        if (e.key === 'ArrowRight') {handleNext()}
        if (e.key === 'Escape') {setIsLightboxOpen(false)}
      }
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isLightboxOpen, handlePrevious, handleNext])

  return (
    <>
      <Box sx={{ position: 'relative' }}>
        <Box
          sx={{
            position: 'relative',
            width: '100%',
            aspectRatio: '1/1',
            borderRadius: 2,
            overflow: 'hidden',
            bgcolor: palette.neutral[50],
            cursor: isZoomed ? 'zoom-out' : 'zoom-in',
          }}
          onClick={handleMainImageClick}
          onMouseMove={handleMouseMove}
          onMouseEnter={() => !isMobile && setIsZoomed(true)}
          onMouseLeave={() => setIsZoomed(false)}
        >
          <Box
            component="img"
            src={selectedImage?.url}
            alt={selectedImage?.alt || title}
            sx={{
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              transition: 'transform 0.1s ease-out',
              transformOrigin: `${zoomPosition.x}% ${zoomPosition.y}%`,
              transform: isZoomed ? 'scale(2)' : 'scale(1)',
            }}
          />

          {images.length > 1 && (
            <>
              <IconButton
                onClick={(e) => {
                  e.stopPropagation()
                  handlePrevious()
                }}
                sx={{
                  position: 'absolute',
                  left: 12,
                  top: '50%',
                  transform: 'translateY(-50%)',
                  bgcolor: 'rgba(255,255,255,0.9)',
                  '&:hover': { bgcolor: 'white' },
                  width: 40,
                  height: 40,
                }}
              >
                <ChevronLeft />
              </IconButton>
              <IconButton
                onClick={(e) => {
                  e.stopPropagation()
                  handleNext()
                }}
                sx={{
                  position: 'absolute',
                  right: 12,
                  top: '50%',
                  transform: 'translateY(-50%)',
                  bgcolor: 'rgba(255,255,255,0.9)',
                  '&:hover': { bgcolor: 'white' },
                  width: 40,
                  height: 40,
                }}
              >
                <ChevronRight />
              </IconButton>
            </>
          )}

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
            {images.map((_, idx) => (
              <Box
                key={idx}
                sx={{
                  width: idx === selectedIndex ? 24 : 8,
                  height: 8,
                  borderRadius: 4,
                  bgcolor: idx === selectedIndex ? palette.neutral[900] : 'rgba(255,255,255,0.8)',
                  transition: 'all 0.2s ease',
                }}
              />
            ))}
          </Box>

          <IconButton
            onClick={(e) => {
              e.stopPropagation()
              handleMainImageClick()
            }}
            sx={{
              position: 'absolute',
              bottom: 12,
              right: 12,
              bgcolor: 'rgba(255,255,255,0.9)',
              '&:hover': { bgcolor: 'white' },
            }}
          >
            <ZoomIn fontSize="small" />
          </IconButton>
        </Box>

        <Stack
          direction="row"
          sx={{
            position: 'absolute',
            top: 12,
            right: 12,
            gap: 1,
          }}
        >
          {onToggleFavorite && (
            <IconButton
              onClick={(e) => {
                e.stopPropagation()
                onToggleFavorite()
              }}
              sx={{
                bgcolor: 'rgba(255,255,255,0.9)',
                '&:hover': { bgcolor: 'white' },
              }}
            >
              {isFavorite ? (
                <Favorite sx={{ color: palette.semantic.error }} />
              ) : (
                <FavoriteBorder />
              )}
            </IconButton>
          )}
          {onShare && (
            <IconButton
              onClick={(e) => {
                e.stopPropagation()
                onShare()
              }}
              sx={{
                bgcolor: 'rgba(255,255,255,0.9)',
                '&:hover': { bgcolor: 'white' },
              }}
            >
              <Share fontSize="small" />
            </IconButton>
          )}
        </Stack>

        <Stack
          direction="row"
          spacing={1}
          sx={{
            mt: 2,
            overflowX: 'auto',
            pb: 1,
            '&::-webkit-scrollbar': {
              height: 4,
            },
            '&::-webkit-scrollbar-thumb': {
              bgcolor: palette.neutral[100],
              borderRadius: 2,
            },
          }}
        >
          {images.map((image, idx) => (
            <Box
              key={image.id}
              onClick={() => handleThumbnailClick(idx)}
              sx={{
                flexShrink: 0,
                width: 80,
                height: 80,
                borderRadius: 1,
                overflow: 'hidden',
                cursor: 'pointer',
                border:
                  idx === selectedIndex
                    ? `2px solid ${palette.neutral[900]}`
                    : '2px solid transparent',
                opacity: idx === selectedIndex ? 1 : 0.7,
                transition: 'all 0.2s ease',
                '&:hover': {
                  opacity: 1,
                  borderColor: palette.neutral[700],
                },
              }}
            >
              <Box
                component="img"
                src={image.url}
                alt={image.alt}
                sx={{
                  width: '100%',
                  height: '100%',
                  objectFit: 'cover',
                }}
              />
            </Box>
          ))}
        </Stack>
      </Box>

      <Modal open={isLightboxOpen} onClose={() => setIsLightboxOpen(false)} closeAfterTransition>
        <Fade in={isLightboxOpen}>
          <Box
            sx={{
              position: 'fixed',
              inset: 0,
              bgcolor: 'rgba(0,0,0,0.95)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              p: 2,
            }}
          >
            <IconButton
              onClick={() => setIsLightboxOpen(false)}
              sx={{
                position: 'absolute',
                top: 16,
                right: 16,
                color: 'white',
              }}
            >
              <Close />
            </IconButton>

            <IconButton
              onClick={handlePrevious}
              sx={{
                position: 'absolute',
                left: 16,
                color: 'white',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.1)' },
              }}
            >
              <ChevronLeft sx={{ fontSize: 40 }} />
            </IconButton>

            <Box
              component="img"
              src={selectedImage?.url}
              alt={selectedImage?.alt}
              sx={{
                maxWidth: '90vw',
                maxHeight: '90vh',
                objectFit: 'contain',
              }}
            />

            <IconButton
              onClick={handleNext}
              sx={{
                position: 'absolute',
                right: 16,
                color: 'white',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.1)' },
              }}
            >
              <ChevronRight sx={{ fontSize: 40 }} />
            </IconButton>

            <Stack
              direction="row"
              spacing={1}
              sx={{
                position: 'absolute',
                bottom: 24,
                left: '50%',
                transform: 'translateX(-50%)',
              }}
            >
              {images.map((_, idx) => (
                <Box
                  key={idx}
                  onClick={() => setSelectedIndex(idx)}
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    bgcolor: idx === selectedIndex ? 'white' : 'rgba(255,255,255,0.4)',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    '&:hover': {
                      bgcolor: 'rgba(255,255,255,0.8)',
                    },
                  }}
                />
              ))}
            </Stack>
          </Box>
        </Fade>
      </Modal>
    </>
  )
}

export function ImageGallerySkeleton() {
  return (
    <Box>
      <Skeleton
        variant="rectangular"
        sx={{
          width: '100%',
          aspectRatio: '1/1',
          borderRadius: 2,
        }}
      />
      <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
        {[1, 2, 3, 4].map((i) => (
          <Skeleton key={i} variant="rectangular" sx={{ width: 80, height: 80, borderRadius: 1 }} />
        ))}
      </Stack>
    </Box>
  )
}
