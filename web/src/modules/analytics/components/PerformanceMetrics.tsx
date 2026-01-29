import { Box, Typography, Stack, LinearProgress, Skeleton } from '@mui/material'
import type { MetricItem } from '../types'

interface PerformanceMetricsProps {
  metrics: MetricItem[]
  isLoading?: boolean
  title?: string
}

export function PerformanceMetrics({
  metrics,
  isLoading,
  title = 'Performance Breakdown',
}: PerformanceMetricsProps) {
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
        <Stack spacing={2.5}>
          {Array.from({ length: 4 }).map((_, i) => (
            <Box key={i}>
              <Skeleton variant="text" width="60%" height={20} sx={{ mb: 0.5 }} />
              <Skeleton variant="rectangular" height={8} sx={{ borderRadius: 1 }} />
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
        gutterBottom
        sx={{ fontFamily: '"Fira Sans", sans-serif', mb: 2 }}
      >
        {title}
      </Typography>

      <Stack spacing={2.5}>
        {metrics.map((metric, index) => {
          const percentage = metric.total > 0 ? (metric.value / metric.total) * 100 : 0

          return (
            <Box key={index}>
              <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
                sx={{ mb: 0.75 }}
              >
                <Typography
                  sx={{
                    fontSize: '0.875rem',
                    color: 'text.primary',
                    fontWeight: 500,
                  }}
                >
                  {metric.label}
                </Typography>
                <Typography
                  sx={{
                    fontSize: '0.8125rem',
                    color: 'text.secondary',
                    fontFamily: '"Fira Code", monospace',
                  }}
                >
                  {metric.value.toLocaleString()} / {metric.total.toLocaleString()}
                </Typography>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={Math.min(percentage, 100)}
                color={metric.color}
                sx={{
                  height: 8,
                  borderRadius: 1,
                  bgcolor: 'action.hover',
                  '& .MuiLinearProgress-bar': {
                    borderRadius: 1,
                  },
                }}
              />
            </Box>
          )
        })}
      </Stack>
    </Box>
  )
}
