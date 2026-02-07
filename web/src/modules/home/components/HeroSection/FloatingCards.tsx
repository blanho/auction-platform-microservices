import { Box, Typography, Chip } from '@mui/material'
import { Timer } from '@mui/icons-material'
import { GlassCard } from '@/shared/components/ui'
import { FloatingElement } from '../shared'
import { colors, typography, shadows, transitions } from '@/shared/theme/tokens'
import { useFeaturedAuctions } from '@/modules/auctions/hooks/useAuctions'
import { formatCurrency, formatTimeLeft } from '@/modules/auctions/utils'
import { useMemo } from 'react'

export const FloatingCards = () => {
  const { data: featuredData } = useFeaturedAuctions(2)
  const featuredAuctions = useMemo(() => featuredData?.items ?? [], [featuredData?.items])

  if (featuredAuctions.length === 0) {
    return null
  }

  const primary = featuredAuctions[0]
  const secondary = featuredAuctions[1]

  return (
    <Box sx={{ position: 'relative', height: 600 }}>
      {primary && (
        <FloatingElement delay={0}>
        <GlassCard
          sx={{
            position: 'absolute',
            top: 0,
            right: 40,
            width: 320,
            overflow: 'hidden',
            cursor: 'pointer',
            transition: `transform ${transitions.duration.normal}s ease, box-shadow ${transitions.duration.normal}s ease`,
            '&:hover': {
              transform: 'translateY(-8px)',
              boxShadow: shadows.card,
            },
          }}
        >
          <Box sx={{ position: 'relative' }}>
            {primary.primaryImageUrl ? (
              <Box
                component="img"
                src={primary.primaryImageUrl}
                alt={primary.title}
                sx={{ width: '100%', height: 240, objectFit: 'cover' }}
              />
            ) : (
              <Box
                sx={{
                  width: '100%',
                  height: 240,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: colors.background.secondary,
                  color: colors.text.primary,
                  fontFamily: typography.fontFamily.display,
                  fontSize: '2rem',
                  fontWeight: typography.fontWeight.semibold,
                }}
              >
                {primary.title.charAt(0)}
              </Box>
            )}
            <Chip
              icon={<Timer sx={{ fontSize: 14 }} />}
              label={formatTimeLeft(primary.endTime)}
              size="small"
              sx={{
                position: 'absolute',
                bottom: 12,
                left: 12,
                bgcolor: 'rgba(0,0,0,0.75)',
                color: colors.gold.light,
                fontWeight: typography.fontWeight.semibold,
                backdropFilter: 'blur(10px)',
              }}
            />
          </Box>
          <Box sx={{ p: 2.5 }}>
            <Typography variant="caption" sx={{ color: colors.text.disabled }}>
              {primary.categoryName}
            </Typography>
            <Typography
              variant="subtitle1"
              sx={{ color: colors.text.primary, fontWeight: typography.fontWeight.semibold, my: 1 }}
            >
              {primary.title}
            </Typography>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="caption" sx={{ color: colors.text.disabled }}>
                  Current Bid
                </Typography>
                <Typography
                  variant="h6"
                  sx={{ color: colors.gold.primary, fontWeight: typography.fontWeight.bold }}
                >
                  {formatCurrency(primary.currentBid || primary.startingPrice)}
                </Typography>
              </Box>
              <Chip
                label={`${primary.bidCount} bids`}
                size="small"
                sx={{ bgcolor: colors.background.elevated, color: colors.text.primary }}
              />
            </Box>
          </Box>
        </GlassCard>
      </FloatingElement>
      )}

      {secondary && (
        <FloatingElement delay={1}>
        <GlassCard
          sx={{
            position: 'absolute',
            top: 180,
            left: 0,
            width: 280,
            overflow: 'hidden',
            cursor: 'pointer',
            transition: `transform ${transitions.duration.normal}s ease`,
            '&:hover': { transform: 'translateY(-8px)' },
          }}
        >
          <Box sx={{ position: 'relative' }}>
            {secondary.primaryImageUrl ? (
              <Box
                component="img"
                src={secondary.primaryImageUrl}
                alt={secondary.title}
                sx={{ width: '100%', height: 180, objectFit: 'cover' }}
              />
            ) : (
              <Box
                sx={{
                  width: '100%',
                  height: 180,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: colors.background.secondary,
                  color: colors.text.primary,
                  fontFamily: typography.fontFamily.display,
                  fontSize: '1.5rem',
                  fontWeight: typography.fontWeight.semibold,
                }}
              >
                {secondary.title.charAt(0)}
              </Box>
            )}
          </Box>
          <Box sx={{ p: 2 }}>
            <Typography variant="caption" sx={{ color: colors.text.disabled }}>
              {secondary.categoryName}
            </Typography>
            <Typography
              variant="subtitle2"
              sx={{
                color: colors.text.primary,
                fontWeight: typography.fontWeight.semibold,
                mt: 0.5,
              }}
            >
              {secondary.title}
            </Typography>
            <Typography
              variant="h6"
              sx={{ color: colors.gold.primary, fontWeight: typography.fontWeight.bold, mt: 1 }}
            >
              {formatCurrency(secondary.currentBid || secondary.startingPrice)}
            </Typography>
          </Box>
        </GlassCard>
      </FloatingElement>
      )}
    </Box>
  )
}
