import { Box, Typography, Stack, Skeleton, useTheme, alpha } from '@mui/material'
import { People, Gavel, TrendingUp, AttachMoney } from '@mui/icons-material'
import { motion } from 'framer-motion'
import { useRealTimeStats } from '../hooks/useAnalytics'
import { formatNumber, formatCurrency } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

interface StatItemProps {
  icon: React.ReactNode
  label: string
  value: string | number
  color: string
  pulse?: boolean
}

function StatItem({ icon, label, value, color, pulse }: StatItemProps) {
  const theme = useTheme()

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 2,
        p: 2,
        borderRadius: 2,
        bgcolor: alpha(color, 0.08),
        border: '1px solid',
        borderColor: alpha(color, 0.2),
        transition: 'all 0.2s',
        cursor: 'default',
        '&:hover': {
          bgcolor: alpha(color, 0.12),
          transform: 'translateY(-2px)',
        },
      }}
    >
      <Box
        component={motion.div}
        animate={pulse ? { scale: [1, 1.1, 1] } : {}}
        transition={{ repeat: Infinity, duration: 2 }}
        sx={{
          width: 44,
          height: 44,
          borderRadius: '50%',
          bgcolor: alpha(color, 0.15),
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: color,
        }}
      >
        {icon}
      </Box>
      <Box>
        <Typography
          sx={{
            fontSize: '1.5rem',
            fontWeight: 700,
            color: theme.palette.mode === 'dark' ? palette.neutral[100] : palette.neutral[800],
            fontFamily: '"Fira Sans", sans-serif',
            lineHeight: 1.2,
          }}
        >
          {value}
        </Typography>
        <Typography
          sx={{
            fontSize: '0.75rem',
            color: 'text.secondary',
            textTransform: 'uppercase',
            letterSpacing: '0.5px',
            fontWeight: 500,
          }}
        >
          {label}
        </Typography>
      </Box>
    </Box>
  )
}

export function RealTimeStatsCard() {
  const { data: stats, isLoading } = useRealTimeStats()

  if (isLoading) {
    return (
      <Box
        sx={{
          p: 3,
          borderRadius: 2,
          bgcolor: 'background.paper',
          boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          border: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
          <Box
            sx={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              bgcolor: 'success.main',
            }}
          />
          <Skeleton variant="text" width={100} height={24} />
        </Stack>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ flexWrap: 'wrap' }}>
          {[1, 2, 3, 4].map((i) => (
            <Box key={i} sx={{ flex: '1 1 200px' }}>
              <Skeleton variant="rounded" height={80} />
            </Box>
          ))}
        </Stack>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        p: 3,
        borderRadius: 2,
        bgcolor: 'background.paper',
        boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
        border: '1px solid',
        borderColor: 'divider',
      }}
    >
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <Box
          component={motion.div}
          animate={{ opacity: [1, 0.5, 1] }}
          transition={{ repeat: Infinity, duration: 1.5 }}
          sx={{
            width: 8,
            height: 8,
            borderRadius: '50%',
            bgcolor: 'success.main',
          }}
        />
        <Typography variant="h6" fontWeight={600} sx={{ fontFamily: '"Fira Sans", sans-serif' }}>
          Real-Time Stats
        </Typography>
      </Stack>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ flexWrap: 'wrap' }}>
        <Box sx={{ flex: '1 1 200px' }}>
          <StatItem
            icon={<People />}
            label="Online Users"
            value={formatNumber(stats?.onlineUsers ?? 0)}
            color={palette.semantic.info}
            pulse
          />
        </Box>
        <Box sx={{ flex: '1 1 200px' }}>
          <StatItem
            icon={<Gavel />}
            label="Active Auctions"
            value={formatNumber(stats?.activeAuctions ?? 0)}
            color={palette.brand.primary}
          />
        </Box>
        <Box sx={{ flex: '1 1 200px' }}>
          <StatItem
            icon={<TrendingUp />}
            label="Bids Last Hour"
            value={formatNumber(stats?.bidsLastHour ?? 0)}
            color={palette.semantic.success}
          />
        </Box>
        <Box sx={{ flex: '1 1 200px' }}>
          <StatItem
            icon={<AttachMoney />}
            label="Revenue Last Hour"
            value={formatCurrency(stats?.revenueLastHour ?? 0)}
            color={palette.purple.primary}
          />
        </Box>
      </Stack>
    </Box>
  )
}
