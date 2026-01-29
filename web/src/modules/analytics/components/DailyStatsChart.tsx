import { Box, Typography, Stack, Skeleton, useTheme } from '@mui/material'
import { useMemo } from 'react'
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import { useDailyStats } from '../hooks/useAnalytics'
import { formatNumber, formatCurrency } from '@/shared/utils/formatters'
import type { ChartDataPoint } from '../types'
import { palette } from '@/shared/theme/tokens'

interface CustomTooltipProps {
  active?: boolean
  payload?: {
    value: number
    dataKey: string
    color: string
    payload: ChartDataPoint
  }[]
  label?: string
}

function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (active && payload && payload.length) {
    const data = payload[0].payload
    return (
      <Box
        sx={{
          bgcolor: 'background.paper',
          p: 2,
          borderRadius: 1,
          boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          border: '1px solid',
          borderColor: 'divider',
          minWidth: 160,
        }}
      >
        <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1 }}>
          {label}
        </Typography>
        <Stack spacing={0.5}>
          <Typography variant="caption" sx={{ color: palette.semantic.info }}>
            Auctions: {formatNumber(data.auctions)}
          </Typography>
          <Typography variant="caption" sx={{ color: palette.semantic.success }}>
            Bids: {formatNumber(data.bids)}
          </Typography>
          <Typography variant="caption" sx={{ color: palette.semantic.warning }}>
            Revenue: {formatCurrency(data.revenue)}
          </Typography>
        </Stack>
      </Box>
    )
  }
  return null
}

export function DailyStatsChart() {
  const theme = useTheme()
  const { data: dailyStats, isLoading } = useDailyStats()

  const chartData = useMemo(() => {
    if (!dailyStats) {return []}

    const dateMap = new Map<string, ChartDataPoint>()

    dailyStats.auctionStats?.forEach((item) => {
      const existing = dateMap.get(item.dateKey) || {
        date: item.dateKey,
        auctions: 0,
        bids: 0,
        revenue: 0,
      }
      existing.auctions += item.eventCount
      dateMap.set(item.dateKey, existing)
    })

    dailyStats.bidStats?.forEach((item) => {
      const existing = dateMap.get(item.dateKey) || {
        date: item.dateKey,
        auctions: 0,
        bids: 0,
        revenue: 0,
      }
      existing.bids = item.totalBids
      dateMap.set(item.dateKey, existing)
    })

    dailyStats.revenueStats?.forEach((item) => {
      const existing = dateMap.get(item.dateKey) || {
        date: item.dateKey,
        auctions: 0,
        bids: 0,
        revenue: 0,
      }
      existing.revenue += item.totalRevenue
      dateMap.set(item.dateKey, existing)
    })

    return Array.from(dateMap.values())
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
      .slice(-14)
      .map((item) => ({
        ...item,
        date: new Date(item.date).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
        }),
      }))
  }, [dailyStats])

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
        <Skeleton variant="text" width={180} height={32} sx={{ mb: 2 }} />
        <Skeleton variant="rectangular" height={280} />
      </Box>
    )
  }

  if (!chartData.length) {
    return (
      <Box
        sx={{
          p: 3,
          borderRadius: 2,
          bgcolor: 'background.paper',
          boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          border: '1px solid',
          borderColor: 'divider',
          textAlign: 'center',
        }}
      >
        <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
          Daily Activity
        </Typography>
        <Typography color="text.secondary">No daily stats available</Typography>
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
        Daily Activity (Last 14 Days)
      </Typography>

      <Box sx={{ width: '100%', height: 280 }}>
        <ResponsiveContainer>
          <AreaChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
            <defs>
              <linearGradient id="colorAuctions" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={palette.semantic.info} stopOpacity={0.3} />
                <stop offset="95%" stopColor={palette.semantic.info} stopOpacity={0} />
              </linearGradient>
              <linearGradient id="colorBids" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={palette.semantic.success} stopOpacity={0.3} />
                <stop offset="95%" stopColor={palette.semantic.success} stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} vertical={false} />
            <XAxis
              dataKey="date"
              stroke={theme.palette.text.secondary}
              tick={{ fontSize: 11 }}
              tickLine={false}
              axisLine={{ stroke: theme.palette.divider }}
            />
            <YAxis
              stroke={theme.palette.text.secondary}
              tick={{ fontSize: 11 }}
              tickLine={false}
              axisLine={{ stroke: theme.palette.divider }}
              tickFormatter={(value) => formatNumber(value)}
            />
            <Tooltip content={<CustomTooltip />} />
            <Area
              type="monotone"
              dataKey="auctions"
              stroke={palette.semantic.info}
              strokeWidth={2}
              fill="url(#colorAuctions)"
            />
            <Area
              type="monotone"
              dataKey="bids"
              stroke={palette.semantic.success}
              strokeWidth={2}
              fill="url(#colorBids)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </Box>

      <Stack direction="row" spacing={3} justifyContent="center" sx={{ mt: 2 }}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <Box sx={{ width: 12, height: 12, borderRadius: 0.5, bgcolor: palette.semantic.info }} />
          <Typography variant="caption" color="text.secondary">
            Auctions
          </Typography>
        </Stack>
        <Stack direction="row" alignItems="center" spacing={1}>
          <Box
            sx={{ width: 12, height: 12, borderRadius: 0.5, bgcolor: palette.semantic.success }}
          />
          <Typography variant="caption" color="text.secondary">
            Bids
          </Typography>
        </Stack>
      </Stack>
    </Box>
  )
}
