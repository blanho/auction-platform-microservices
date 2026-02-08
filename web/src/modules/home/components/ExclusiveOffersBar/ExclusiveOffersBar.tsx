import { useState } from 'react'
import { Box, Typography, IconButton } from '@mui/material'
import { KeyboardArrowUp, Close } from '@mui/icons-material'
import { motion, AnimatePresence } from 'framer-motion'
import { typography } from '@/shared/theme/tokens'

interface Offer {
  id: string
  title: string
  description: string
  code?: string
}

const defaultOffers: Offer[] = [
  {
    id: '1',
    title: 'Welcome Offer',
    description: 'Get 10% off your first purchase with code WELCOME10',
    code: 'WELCOME10',
  },
  {
    id: '2',
    title: 'Free Shipping',
    description: 'Free shipping on orders over $500',
  },
]

interface ExclusiveOffersBarProps {
  offers?: Offer[]
}

export const ExclusiveOffersBar = ({ offers = defaultOffers }: ExclusiveOffersBarProps) => {
  const [isExpanded, setIsExpanded] = useState(false)
  const [isVisible, setIsVisible] = useState(true)

  if (!isVisible || offers.length === 0) {
    return null
  }

  return (
    <Box
      sx={{
        position: 'fixed',
        bottom: 0,
        left: 0,
        right: 0,
        zIndex: 1100,
      }}
    >
      <Box
        onClick={() => setIsExpanded(!isExpanded)}
        sx={{
          bgcolor: '#1C1917',
          color: '#FFFFFF',
          py: 1.5,
          px: 3,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          cursor: 'pointer',
          '&:hover': {
            bgcolor: '#292524',
          },
        }}
      >
        <Typography
          sx={{
            fontFamily: typography.fontFamily.body,
            fontSize: '0.875rem',
            fontWeight: typography.fontWeight.medium,
          }}
        >
          <Box component="span" sx={{ color: '#E8E4B8', mr: 1 }}>
            NEW:
          </Box>
          Your Exclusive Offers
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography
            sx={{
              fontFamily: typography.fontFamily.body,
              fontSize: '0.875rem',
            }}
          >
            {offers.length} Offer{offers.length !== 1 ? 's' : ''}
          </Typography>
          <IconButton
            size="small"
            sx={{
              color: '#FFFFFF',
              transform: isExpanded ? 'rotate(180deg)' : 'rotate(0deg)',
              transition: 'transform 0.3s ease',
            }}
          >
            <KeyboardArrowUp fontSize="small" />
          </IconButton>
        </Box>
      </Box>

      <AnimatePresence>
        {isExpanded && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <Box
              sx={{
                bgcolor: '#FFFFFF',
                borderTop: '1px solid #E7E5E4',
                boxShadow: '0 -4px 20px rgba(0,0,0,0.1)',
              }}
            >
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'flex-end',
                  p: 1,
                  borderBottom: '1px solid #F5F5F4',
                }}
              >
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation()
                    setIsVisible(false)
                  }}
                  sx={{ color: '#78716C' }}
                >
                  <Close fontSize="small" />
                </IconButton>
              </Box>

              {offers.map((offer, index) => (
                <Box
                  key={offer.id}
                  sx={{
                    p: 3,
                    borderBottom: index < offers.length - 1 ? '1px solid #F5F5F4' : 'none',
                    '&:hover': {
                      bgcolor: '#FAFAF9',
                    },
                  }}
                >
                  <Typography
                    sx={{
                      fontFamily: typography.fontFamily.body,
                      fontSize: '0.9375rem',
                      fontWeight: typography.fontWeight.medium,
                      color: '#1C1917',
                      mb: 0.5,
                    }}
                  >
                    {offer.title}
                  </Typography>
                  <Typography
                    sx={{
                      fontFamily: typography.fontFamily.body,
                      fontSize: '0.875rem',
                      color: '#57534E',
                    }}
                  >
                    {offer.description}
                  </Typography>
                  {offer.code && (
                    <Box
                      sx={{
                        display: 'inline-block',
                        mt: 1.5,
                        px: 2,
                        py: 0.75,
                        bgcolor: '#F5F5F4',
                        border: '1px dashed #D6D3D1',
                      }}
                    >
                      <Typography
                        sx={{
                          fontFamily: typography.fontFamily.body,
                          fontSize: '0.75rem',
                          fontWeight: typography.fontWeight.semibold,
                          color: '#1C1917',
                          letterSpacing: '0.05em',
                        }}
                      >
                        {offer.code}
                      </Typography>
                    </Box>
                  )}
                </Box>
              ))}
            </Box>
          </motion.div>
        )}
      </AnimatePresence>
    </Box>
  )
}
