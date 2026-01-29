import { Box, Typography, Stack, Skeleton, Card, CardContent } from '@mui/material'
import { TrendingUp, TrendingDown } from '@mui/icons-material'

export interface StatCardProps {
  title: string
  value: string | number
  icon: React.ReactNode
  color?: string
  iconBg?: string
  iconColor?: string
  change?: number
  changeLabel?: string
  loading?: boolean
  variant?: 'default' | 'compact'
}

export function StatCard({
  title,
  value,
  icon,
  color,
  iconBg,
  iconColor,
  change,
  changeLabel = 'vs last period',
  loading = false,
  variant = 'default',
}: StatCardProps) {
  const effectiveIconBg = iconBg || (color ? `${color}15` : undefined)
  const effectiveIconColor = iconColor || color

  if (loading) {
    return <StatCardSkeleton variant={variant} />
  }

  const isPositive = change !== undefined && change >= 0

  if (variant === 'compact') {
    return (
      <Card sx={{ height: '100%' }}>
        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: effectiveIconBg,
              color: effectiveIconColor,
            }}
          >
            {icon}
          </Box>
          <Box>
            <Typography variant="h5" fontWeight={700}>
              {value}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {title}
            </Typography>
          </Box>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card
      sx={{
        p: 3,
        display: 'flex',
        alignItems: 'flex-start',
        gap: 2,
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: '0 8px 24px rgba(0,0,0,0.12)',
        },
      }}
    >
      <Box
        sx={{
          width: 48,
          height: 48,
          borderRadius: 2,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: effectiveIconBg,
          color: effectiveIconColor,
          flexShrink: 0,
        }}
      >
        {icon}
      </Box>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography
          sx={{
            fontSize: '0.8125rem',
            color: 'text.secondary',
            mb: 0.5,
            fontWeight: 500,
          }}
        >
          {title}
        </Typography>
        <Typography
          sx={{
            fontSize: '1.75rem',
            fontWeight: 700,
            color: 'text.primary',
            lineHeight: 1.2,
            fontFamily: '"Fira Sans", sans-serif',
          }}
        >
          {value}
        </Typography>
        {change !== undefined && (
          <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mt: 1 }}>
            {isPositive ? (
              <TrendingUp sx={{ fontSize: 16, color: 'success.main' }} />
            ) : (
              <TrendingDown sx={{ fontSize: 16, color: 'error.main' }} />
            )}
            <Typography
              sx={{
                fontSize: '0.8125rem',
                color: isPositive ? 'success.main' : 'error.main',
                fontWeight: 600,
              }}
            >
              {isPositive ? '+' : ''}
              {change}%
            </Typography>
            <Typography sx={{ fontSize: '0.75rem', color: 'text.disabled' }}>
              {changeLabel}
            </Typography>
          </Stack>
        )}
      </Box>
    </Card>
  )
}

export function StatCardSkeleton({ variant = 'default' }: { variant?: 'default' | 'compact' }) {
  if (variant === 'compact') {
    return (
      <Card sx={{ height: '100%' }}>
        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Skeleton variant="rounded" width={48} height={48} />
          <Box>
            <Skeleton width={60} height={32} />
            <Skeleton width={80} height={16} />
          </Box>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card sx={{ p: 3 }}>
      <Stack direction="row" spacing={2} alignItems="flex-start">
        <Skeleton variant="rounded" width={48} height={48} />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width="60%" height={16} sx={{ mb: 1 }} />
          <Skeleton variant="text" width="40%" height={32} />
          <Skeleton variant="text" width="50%" height={16} sx={{ mt: 1 }} />
        </Box>
      </Stack>
    </Card>
  )
}
