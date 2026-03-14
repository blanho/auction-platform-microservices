import { Box, Typography, Stack, Skeleton, useTheme, alpha, Divider } from '@mui/material'
import { People, Gavel, AttachMoney, BarChart, TrendingUp, TrendingDown } from '@mui/icons-material'
import { useBidMetrics } from '../hooks/useAnalytics'
import { formatNumber, formatCurrency } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

interface MetricItemProps {
  icon: React.ReactNode
  label: string
  value: string | number
  subValue?: string
  trend?: number
  color: string
}

function MetricItem({ icon, label, value, subValue, trend, color }: MetricItemProps) {
  const theme = useTheme()
  const isPositive = trend !== undefined && trend >= 0

  return (
    <Box
      sx={{
        p: 2.5,
        borderRadius: 2,
        bgcolor: theme.palette.mode === 'dark' ? alpha('#1E293B', 0.5) : alpha('#F8FAFC', 0.8),
        border: '1px solid',
        borderColor: theme.palette.divider,
        flex: '1 1 200px',
        minWidth: 0,
        transition: 'all 0.2s',
        '&:hover': {
          boxShadow: `0 4px 12px ${alpha(color, 0.15)}`,
          borderColor: alpha(color, 0.3),
        },
      }}
    >
      <Stack direction="row" alignItems="flex-start" justifyContent="space-between">
        <Box
          sx={{
            width: 40,
            height: 40,
            borderRadius: 1.5,
            bgcolor: alpha(color, 0.12),
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: color,
          }}
        >
          {icon}
        </Box>
        {trend !== undefined && (
          <Stack direction="row" alignItems="center" spacing={0.5}>
            {isPositive ? (
              <TrendingUp sx={{ fontSize: 16, color: palette.semantic.success }} />
            ) : (
              <TrendingDown sx={{ fontSize: 16, color: palette.semantic.error }} />
            )}
            <Typography
              variant="caption"
              fontWeight={600}
              sx={{ color: isPositive ? palette.semantic.success : palette.semantic.error }}
            >
              {Math.abs(trend)}%
            </Typography>
          </Stack>
        )}
      </Stack>
      <Typography
        sx={{
          mt: 2,
          fontSize: '1.75rem',
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
          fontSize: '0.875rem',
          color: 'text.secondary',
          fontWeight: 500,
          mt: 0.5,
        }}
      >
        {label}
      </Typography>
      {subValue && (
        <Typography
          variant="caption"
          sx={{
            color: 'text.disabled',
            display: 'block',
            mt: 0.5,
          }}
        >
          {subValue}
        </Typography>
      )}
    </Box>
  )
}

export function BidMetricsCard() {
  const { data: metrics, isLoading } = useBidMetrics()

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
        <Skeleton variant="text" width={150} height={32} sx={{ mb: 3 }} />
        <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap', gap: 2 }}>
          {[1, 2, 3, 4].map((i) => (
            <Box key={i} sx={{ flex: '1 1 200px' }}>
              <Skeleton variant="rounded" height={130} />
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
      <Typography
        variant="h6"
        fontWeight={600}
        sx={{ mb: 3, fontFamily: '"Fira Sans", sans-serif' }}
      >
        Bidding Analytics
      </Typography>

      <Stack
        direction="row"
        sx={{
          flexWrap: 'wrap',
          gap: 2,
          mb: 3,
        }}
      >
        <MetricItem
          icon={<Gavel />}
          label="Total Bids"
          value={formatNumber(metrics?.totalBids ?? 0)}
          subValue="All time"
          color={palette.semantic.info}
        />
        <MetricItem
          icon={<People />}
          label="Unique Bidders"
          value={formatNumber(metrics?.uniqueBidders ?? 0)}
          subValue="Active participants"
          color={palette.purple.primary}
        />
        <MetricItem
          icon={<AttachMoney />}
          label="Avg Bid Amount"
          value={formatCurrency(metrics?.averageBidAmount ?? 0)}
          subValue="Per bid"
          color={palette.semantic.success}
        />
        <MetricItem
          icon={<BarChart />}
          label="Avg Bids/Auction"
          value={formatNumber(metrics?.averageBidsPerAuction ?? 0)}
          subValue="Engagement metric"
          color={palette.semantic.warning}
        />
      </Stack>

      <Divider sx={{ my: 2 }} />

      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={3}
        divider={
          <Divider orientation="vertical" flexItem sx={{ display: { xs: 'none', sm: 'block' } }} />
        }
      >
        <Box>
          <Typography variant="caption" color="text.secondary">
            Bids Today
          </Typography>
          <Typography variant="h6" fontWeight={600} color="success.main">
            {formatNumber(metrics?.bidsToday ?? 0)}
          </Typography>
        </Box>
        <Box>
          <Typography variant="caption" color="text.secondary">
            This Week
          </Typography>
          <Typography variant="h6" fontWeight={600} color="info.main">
            {formatNumber(metrics?.bidsThisWeek ?? 0)}
          </Typography>
        </Box>
        <Box>
          <Typography variant="caption" color="text.secondary">
            This Month
          </Typography>
          <Typography variant="h6" fontWeight={600} color="warning.main">
            {formatNumber(metrics?.bidsThisMonth ?? 0)}
          </Typography>
        </Box>
      </Stack>
    </Box>
  )
}
