import { Card, CardContent, Typography, Box, Skeleton } from '@mui/material'

interface StatCardProps {
  title: string
  value: number | string
  icon: React.ReactNode
  color: string
  loading?: boolean
}

export function StatCard({ title, value, icon, color, loading }: StatCardProps) {
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
            bgcolor: `${color}15`,
            color: color,
          }}
        >
          {icon}
        </Box>
        <Box>
          {loading ? (
            <Skeleton width={60} height={32} />
          ) : (
            <Typography variant="h5" fontWeight={700}>
              {value}
            </Typography>
          )}
          <Typography variant="caption" color="text.secondary">
            {title}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  )
}
