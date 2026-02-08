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
      <Box
        sx={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          width: 300,
          height: 300,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${colors.gold.primary}0A 0%, transparent 70%)`,
          filter: 'blur(40px)',
          pointerEvents: 'none',
        }}
      />

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
              transition: `all ${transitions.slow}`,
              '&:hover': {
                transform: 'translateY(-8px) scale(1.02)',
                boxShadow: shadows.card,
                border: `1px solid ${colors.gold.primary}33`,
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
                    background: `linear-gradient(135deg, ${colors.background.secondary}, ${colors.background.elevated})`,
                    color: colors.gold.primary,
                    fontFamily: typography.fontFamily.display,
                    fontSize: '3rem',
                    fontWeight: typography.fontWeight.light,
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
              <Typography variant="caption" sx={{ color: colors.text.disabled, letterSpacing: 1, textTransform: 'uppercase', fontSize: '0.65rem' }}>
                {primary.categoryName}
              </Typography>
              <Typography
                variant="subtitle1"
                sx={{ color: colors.text.primary, fontWeight: typography.fontWeight.semibold, my: 1, lineHeight: 1.3 }}
              >
                {primary.title}
              </Typography>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box>
                  <Typography variant="caption" sx={{ color: colors.text.disabled, fontSize: '0.65rem' }}>
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
                  sx={{ bgcolor: 'rgba(255,255,255,0.06)', color: colors.text.secondary, fontSize: '0.7rem' }}
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
              top: 200,
              left: 0,
              width: 260,
              overflow: 'hidden',
              cursor: 'pointer',
              transition: `all ${transitions.slow}`,
              '&:hover': {
                transform: 'translateY(-8px)',
                border: `1px solid ${colors.gold.primary}33`,
              },
            }}
          >
            <Box sx={{ position: 'relative' }}>
              {secondary.primaryImageUrl ? (
                <Box
                  component="img"
                  src={secondary.primaryImageUrl}
                  alt={secondary.title}
                  sx={{ width: '100%', height: 160, objectFit: 'cover' }}
                />
              ) : (
                <Box
                  sx={{
                    width: '100%',
                    height: 160,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    background: `linear-gradient(135deg, ${colors.background.secondary}, ${colors.background.elevated})`,
                    color: colors.gold.primary,
                    fontFamily: typography.fontFamily.display,
                    fontSize: '2rem',
                    fontWeight: typography.fontWeight.light,
                  }}
                >
                  {secondary.title.charAt(0)}
                </Box>
              )}
            </Box>
            <Box sx={{ p: 2 }}>
              <Typography variant="caption" sx={{ color: colors.text.disabled, letterSpacing: 1, textTransform: 'uppercase', fontSize: '0.6rem' }}>
                {secondary.categoryName}
              </Typography>
              <Typography
                variant="subtitle2"
                sx={{ color: colors.text.primary, fontWeight: typography.fontWeight.semibold, mt: 0.5, lineHeight: 1.3 }}
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
