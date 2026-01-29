import { Box, Typography, Skeleton, useTheme } from '@mui/material'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts'
import type { CategoryBreakdown } from '../types'
import { formatCurrency, formatNumber } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

interface CategoryChartProps {
  data: CategoryBreakdown[]
  isLoading?: boolean
  title?: string
  height?: number
  dataKey?: 'revenue' | 'auctionCount' | 'bidCount'
}

const COLORS = [
  palette.semantic.info,
  palette.semantic.success,
  palette.semantic.warning,
  palette.semantic.error,
  palette.purple.primary,
  '#EC4899',
  '#06B6D4',
  palette.brand.secondary,
]

export function CategoryChart({
  data,
  isLoading,
  title = 'Category Performance',
  height = 300,
  dataKey = 'revenue',
}: CategoryChartProps) {
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
        <Skeleton variant="text" width={180} height={28} sx={{ mb: 2 }} />
        <Skeleton variant="rectangular" height={height} sx={{ borderRadius: 1 }} />
      </Box>
    )
  }

  const chartData = data.slice(0, 8).map((cat) => ({
    ...cat,
    name: cat.categoryName.length > 12 ? `${cat.categoryName.slice(0, 12)}...` : cat.categoryName,
  }))

  const formatValue = (value: number) => {
    if (dataKey === 'revenue') {
      return formatCurrency(value)
    }
    return formatNumber(value)
  }

  const getLabel = () => {
    switch (dataKey) {
      case 'revenue':
        return 'Revenue'
      case 'auctionCount':
        return 'Auctions'
      case 'bidCount':
        return 'Bids'
      default:
        return 'Value'
    }
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
        gutterBottom
        sx={{ fontFamily: '"Fira Sans", sans-serif' }}
      >
        {title}
      </Typography>
      <ResponsiveContainer width="100%" height={height}>
        <BarChart
          data={chartData}
          layout="vertical"
          margin={{ top: 5, right: 30, left: 0, bottom: 5 }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            stroke={isDark ? palette.neutral[700] : palette.neutral[200]}
            horizontal={false}
          />
          <XAxis
            type="number"
            axisLine={false}
            tickLine={false}
            tick={{
              fill: isDark ? '#9CA3AF' : '#6B7280',
              fontSize: 11,
              fontFamily: '"Fira Sans", sans-serif',
            }}
            tickFormatter={(value) =>
              dataKey === 'revenue' ? `$${(value / 1000).toFixed(0)}k` : formatNumber(value)
            }
          />
          <YAxis
            type="category"
            dataKey="name"
            axisLine={false}
            tickLine={false}
            tick={{
              fill: isDark ? '#D1D5DB' : '#374151',
              fontSize: 12,
              fontFamily: '"Fira Sans", sans-serif',
            }}
            width={100}
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
            formatter={(value) => [formatValue(value as number), getLabel()]}
            labelFormatter={(label) =>
              chartData.find((c) => c.name === label)?.categoryName || String(label)
            }
            cursor={{ fill: isDark ? 'rgba(255,255,255,0.05)' : 'rgba(0,0,0,0.05)' }}
          />
          <Bar dataKey={dataKey} radius={[0, 4, 4, 0]} maxBarSize={32}>
            {chartData.map((_, index) => (
              <Cell
                key={`cell-${index}`}
                fill={COLORS[index % COLORS.length]}
                style={{ cursor: 'pointer' }}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </Box>
  )
}
