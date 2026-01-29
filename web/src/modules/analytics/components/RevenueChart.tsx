import { Box, Typography, Skeleton, useTheme } from '@mui/material'
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import type { TrendDataPoint } from '../types'
import { formatCurrency } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

interface RevenueChartProps {
  data: TrendDataPoint[]
  isLoading?: boolean
  title?: string
  height?: number
}

export function RevenueChart({
  data,
  isLoading,
  title = 'Revenue Trend',
  height = 300,
}: RevenueChartProps) {
  const theme = useTheme()
  const isDark = theme.palette.mode === 'dark'

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
        <Skeleton variant="text" width={150} height={28} sx={{ mb: 2 }} />
        <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 1 }} />
      </Box>
    )
  }

  const chartData = data.map((point) => ({
    ...point,
    displayDate: new Date(point.date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
    }),
  }))

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
        gutterBottom
        sx={{ fontFamily: '"Fira Sans", sans-serif' }}
      >
        {title}
      </Typography>
      <ResponsiveContainer width="100%" height={height}>
        <AreaChart data={chartData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
          <defs>
            <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor={palette.semantic.info} stopOpacity={0.3} />
              <stop offset="95%" stopColor={palette.semantic.info} stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid
            strokeDasharray="3 3"
            stroke={isDark ? palette.neutral[700] : palette.neutral[200]}
            vertical={false}
          />
          <XAxis
            dataKey="displayDate"
            axisLine={false}
            tickLine={false}
            tick={{
              fill: isDark ? '#9CA3AF' : '#6B7280',
              fontSize: 12,
              fontFamily: '"Fira Sans", sans-serif',
            }}
            dy={10}
          />
          <YAxis
            axisLine={false}
            tickLine={false}
            tick={{
              fill: isDark ? '#9CA3AF' : '#6B7280',
              fontSize: 12,
              fontFamily: '"Fira Sans", sans-serif',
            }}
            tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
            dx={-10}
          />
          <Tooltip
            contentStyle={{
              backgroundColor: isDark ? palette.neutral[800] : palette.neutral[0],
              border: `1px solid ${isDark ? palette.neutral[700] : palette.neutral[200]}`,
              borderRadius: 8,
              boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
              fontFamily: '"Fira Sans", sans-serif',
            }}
            labelStyle={{
              color: isDark ? palette.neutral[100] : palette.neutral[800],
              fontWeight: 600,
              marginBottom: 4,
            }}
            formatter={(value) => [formatCurrency(value as number), 'Revenue']}
          />
          <Area
            type="monotone"
            dataKey="value"
            stroke={palette.semantic.info}
            strokeWidth={2}
            fill="url(#revenueGradient)"
            dot={false}
            activeDot={{
              r: 6,
              fill: palette.semantic.info,
              stroke: palette.neutral[0],
              strokeWidth: 2,
            }}
          />
        </AreaChart>
      </ResponsiveContainer>
    </Box>
  )
}
