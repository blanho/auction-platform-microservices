import {
  Box,
  Typography,
  Stack,
  Skeleton,
  ToggleButtonGroup,
  ToggleButton,
  useTheme,
} from '@mui/material'
import { useState, useMemo } from 'react'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts'
import { useAuctionTrends, useRevenueTrends } from '../hooks/useAnalytics'
import { formatCurrency, formatNumber } from '@/shared/utils/formatters'
import { getDateRange, type TimeRange } from '../utils/date.utils'
import { palette } from '@/shared/theme/tokens'

interface CustomTooltipProps {
  active?: boolean
  payload?: {
    value: number
    name: string
    color: string
  }[]
  label?: string
}

function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (active && payload && payload.length) {
    return (
      <Box
        sx={{
          bgcolor: 'background.paper',
          p: 1.5,
          borderRadius: 1,
          boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          border: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Typography variant="caption" fontWeight={600} sx={{ display: 'block', mb: 0.5 }}>
          {label}
        </Typography>
        {payload.map((entry, index) => (
          <Typography key={index} variant="caption" sx={{ display: 'block', color: entry.color }}>
            {entry.name}:{' '}
            {entry.name === 'Revenue' ? formatCurrency(entry.value) : formatNumber(entry.value)}
          </Typography>
        ))}
      </Box>
    )
  }
  return null
}

export function TrendChart() {
  const theme = useTheme()
  const [timeRange, setTimeRange] = useState<TimeRange>('30d')
  const [chartType, setChartType] = useState<'auctions' | 'revenue' | 'combined'>('combined')

  const dateRange = useMemo(() => getDateRange(timeRange), [timeRange])

  const { data: auctionTrends, isLoading: auctionLoading } = useAuctionTrends({
    startDate: dateRange.startDate,
    endDate: dateRange.endDate,
  })
  const { data: revenueTrends, isLoading: revenueLoading } = useRevenueTrends({
    startDate: dateRange.startDate,
    endDate: dateRange.endDate,
  })

  const isLoading = auctionLoading || revenueLoading

  const chartData = useMemo(() => {
    if (!auctionTrends?.length && !revenueTrends?.length) {return []}

    const dateMap = new Map<
      string,
      {
        date: string
        auctions?: number
        revenue?: number
      }
    >()

    auctionTrends?.forEach((item) => {
      const dateKey = item.date.split('T')[0]
      const existing = dateMap.get(dateKey) || { date: dateKey }
      dateMap.set(dateKey, {
        ...existing,
        auctions: item.value,
      })
    })

    revenueTrends?.forEach((item) => {
      const dateKey = item.date.split('T')[0]
      const existing = dateMap.get(dateKey) || { date: dateKey }
      dateMap.set(dateKey, {
        ...existing,
        revenue: item.value,
      })
    })

    return Array.from(dateMap.values())
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
      .map((item) => ({
        ...item,
        date: new Date(item.date).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
        }),
      }))
  }, [auctionTrends, revenueTrends])

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
        <Skeleton variant="text" width={200} height={32} sx={{ mb: 2 }} />
        <Skeleton variant="rectangular" height={350} />
      </Box>
    )
  }

  const primaryColor = palette.semantic.info
  const tertiaryColor = palette.semantic.warning

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
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        alignItems={{ xs: 'flex-start', sm: 'center' }}
        spacing={2}
        sx={{ mb: 3 }}
      >
        <Typography variant="h6" fontWeight={600} sx={{ fontFamily: '"Fira Sans", sans-serif' }}>
          Platform Trends
        </Typography>
        <Stack direction="row" spacing={1}>
          <ToggleButtonGroup
            value={chartType}
            exclusive
            onChange={(_, value) => value && setChartType(value)}
            size="small"
            sx={{ mr: 1 }}
          >
            <ToggleButton value="auctions" sx={{ textTransform: 'none' }}>
              Auctions
            </ToggleButton>
            <ToggleButton value="revenue" sx={{ textTransform: 'none' }}>
              Revenue
            </ToggleButton>
            <ToggleButton value="combined" sx={{ textTransform: 'none' }}>
              Combined
            </ToggleButton>
          </ToggleButtonGroup>
          <ToggleButtonGroup
            value={timeRange}
            exclusive
            onChange={(_, value) => value && setTimeRange(value)}
            size="small"
          >
            <ToggleButton value="7d" sx={{ textTransform: 'none' }}>
              7D
            </ToggleButton>
            <ToggleButton value="30d" sx={{ textTransform: 'none' }}>
              30D
            </ToggleButton>
            <ToggleButton value="90d" sx={{ textTransform: 'none' }}>
              90D
            </ToggleButton>
          </ToggleButtonGroup>
        </Stack>
      </Stack>

      <Box sx={{ width: '100%', height: 350 }}>
        <ResponsiveContainer>
          <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} vertical={false} />
            <XAxis
              dataKey="date"
              stroke={theme.palette.text.secondary}
              tick={{ fontSize: 12 }}
              tickLine={false}
              axisLine={{ stroke: theme.palette.divider }}
            />
            <YAxis
              yAxisId="left"
              stroke={theme.palette.text.secondary}
              tick={{ fontSize: 12 }}
              tickLine={false}
              axisLine={{ stroke: theme.palette.divider }}
              tickFormatter={(value) => formatNumber(value)}
            />
            {(chartType === 'revenue' || chartType === 'combined') && (
              <YAxis
                yAxisId="right"
                orientation="right"
                stroke={theme.palette.text.secondary}
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={{ stroke: theme.palette.divider }}
                tickFormatter={(value) => `$${formatNumber(value / 1000)}k`}
              />
            )}
            <Tooltip content={<CustomTooltip />} />
            <Legend wrapperStyle={{ paddingTop: '20px' }} iconType="circle" />
            {(chartType === 'auctions' || chartType === 'combined') && (
              <Line
                yAxisId="left"
                type="monotone"
                dataKey="auctions"
                name="Auctions"
                stroke={primaryColor}
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 6 }}
              />
            )}
            {(chartType === 'revenue' || chartType === 'combined') && (
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="revenue"
                name="Revenue"
                stroke={tertiaryColor}
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 6 }}
              />
            )}
          </LineChart>
        </ResponsiveContainer>
      </Box>
    </Box>
  )
}
