import { Box, Typography, Chip } from '@mui/material'
import { Timer, North } from '@mui/icons-material'
import { GlassCard, FloatingElement } from '../shared'
import { colors, typography, shadows, transitions } from '@/shared/theme/tokens'

export const FloatingCards = () => {
  return (
    <Box sx={{ position: 'relative', height: 600 }}>
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
            <Box
              component="img"
              src="https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5?w=640"
              alt="Featured artwork"
              sx={{ width: '100%', height: 240, objectFit: 'cover' }}
            />
            <Chip
              icon={<Timer sx={{ fontSize: 14 }} />}
              label="2h 34m"
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
              Contemporary Art
            </Typography>
            <Typography
              variant="subtitle1"
              sx={{ color: colors.text.primary, fontWeight: typography.fontWeight.semibold, my: 1 }}
            >
              Abstract Composition No. 47
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
                  $12,400
                </Typography>
              </Box>
              <Chip
                label="23 bids"
                size="small"
                sx={{ bgcolor: colors.background.elevated, color: colors.text.primary }}
              />
            </Box>
          </Box>
        </GlassCard>
      </FloatingElement>

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
            <Box
              component="img"
              src="https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=640"
              alt="Luxury watch"
              sx={{ width: '100%', height: 180, objectFit: 'cover' }}
            />
          </Box>
          <Box sx={{ p: 2 }}>
            <Typography variant="caption" sx={{ color: colors.text.disabled }}>
              Luxury Watches
            </Typography>
            <Typography
              variant="subtitle2"
              sx={{
                color: colors.text.primary,
                fontWeight: typography.fontWeight.semibold,
                mt: 0.5,
              }}
            >
              Vintage Patek Philippe
            </Typography>
            <Typography
              variant="h6"
              sx={{ color: colors.gold.primary, fontWeight: typography.fontWeight.bold, mt: 1 }}
            >
              $45,800
            </Typography>
          </Box>
        </GlassCard>
      </FloatingElement>

      <FloatingElement delay={2}>
        <GlassCard
          sx={{
            position: 'absolute',
            bottom: 40,
            right: 80,
            px: 3,
            py: 2,
            display: 'flex',
            alignItems: 'center',
            gap: 2,
          }}
        >
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: '50%',
              bgcolor: `${colors.success}33`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <North sx={{ color: colors.success }} />
          </Box>
          <Box>
            <Typography variant="caption" sx={{ color: colors.text.disabled }}>
              Just Sold
            </Typography>
            <Typography
              variant="subtitle2"
              sx={{ color: colors.text.primary, fontWeight: typography.fontWeight.semibold }}
            >
              Rare Ming Vase
            </Typography>
            <Typography
              variant="body2"
              sx={{ color: colors.success, fontWeight: typography.fontWeight.bold }}
            >
              $128,500
            </Typography>
          </Box>
        </GlassCard>
      </FloatingElement>
    </Box>
  )
}
