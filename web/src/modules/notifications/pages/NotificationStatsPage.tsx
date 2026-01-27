import {
  Box,
  Container,
  Typography,
  Card,
  Grid,
} from '@mui/material'
import {
  Notifications,
  MarkEmailUnread,
  Today,
  TrendingUp,
} from '@mui/icons-material'
import { useNotificationStats } from '../hooks'
import { Skeleton } from '@mui/material'

export function NotificationStatsPage() {
  const { data: stats, isLoading } = useNotificationStats()

  const statCards = [
    {
      title: 'Total Notifications',
      value: stats?.totalNotifications || 0,
      icon: <Notifications sx={{ fontSize: 32, color: '#7C3AED' }} />,
      color: '#7C3AED',
      bgcolor: '#F3E8FF',
    },
    {
      title: 'Unread',
      value: stats?.unreadNotifications || 0,
      icon: <MarkEmailUnread sx={{ fontSize: 32, color: '#F59E0B' }} />,
      color: '#F59E0B',
      bgcolor: '#FEF3C7',
    },
    {
      title: 'Today',
      value: stats?.todayCount || 0,
      icon: <Today sx={{ fontSize: 32, color: '#10B981' }} />,
      color: '#10B981',
      bgcolor: '#D1FAE5',
    },
    {
      title: 'Engagement Rate',
      value: stats?.totalNotifications
        ? `${((1 - (stats.unreadNotifications / stats.totalNotifications)) * 100).toFixed(1)}%`
        : '0%',
      icon: <TrendingUp sx={{ fontSize: 32, color: '#F97316' }} />,
      color: '#F97316',
      bgcolor: '#FFEDD5',
    },
  ]

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Inter", sans-serif',
            fontWeight: 600,
            color: '#4C1D95',
            mb: 1,
          }}
        >
          Notification Statistics
        </Typography>
        <Typography sx={{ color: '#78716C', fontFamily: '"Inter", sans-serif' }}>
          Overview of notification performance and engagement
        </Typography>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statCards.map((stat) => (
          <Grid key={stat.title} size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                p: 3,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: `1px solid ${stat.bgcolor}`,
              }}
            >
              {isLoading ? (
                <Box>
                  <Skeleton width={120} height={20} />
                  <Skeleton width={80} height={40} sx={{ my: 1 }} />
                </Box>
              ) : (
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography sx={{ color: '#78716C', fontSize: '0.875rem', mb: 1 }}>
                      {stat.title}
                    </Typography>
                    <Typography
                      variant="h4"
                      sx={{
                        fontFamily: '"Inter", sans-serif',
                        fontWeight: 700,
                        color: stat.color,
                      }}
                    >
                      {stat.value}
                    </Typography>
                  </Box>
                  <Box
                    sx={{
                      bgcolor: stat.bgcolor,
                      p: 1.5,
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    {stat.icon}
                  </Box>
                </Box>
              )}
            </Card>
          </Grid>
        ))}
      </Grid>

      <Card sx={{ p: 4, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <Typography
          variant="h6"
          sx={{
            fontWeight: 600,
            color: '#4C1D95',
            mb: 3,
            fontFamily: '"Inter", sans-serif',
          }}
        >
          Notifications by Type
        </Typography>

        <Grid container spacing={2}>
          {isLoading ? (
            Array.from({ length: 6 }).map((_, i) => (
              <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                <Skeleton height={60} />
              </Grid>
            ))
          ) : (
            Object.entries(stats?.byType || {}).map(([type, count]) => (
              <Grid key={type} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card
                  sx={{
                    p: 2,
                    bgcolor: '#FAF5FF',
                    border: '1px solid #E9D5FF',
                    borderRadius: 1,
                  }}
                >
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography
                      sx={{
                        fontSize: '0.875rem',
                        color: '#78716C',
                        textTransform: 'capitalize',
                      }}
                    >
                      {type.replace('_', ' ')}
                    </Typography>
                    <Typography
                      sx={{
                        fontWeight: 700,
                        color: '#7C3AED',
                        fontSize: '1.25rem',
                      }}
                    >
                      {count as number}
                    </Typography>
                  </Box>
                </Card>
              </Grid>
            ))
          )}
        </Grid>

        {!isLoading && Object.keys(stats?.byType || {}).length === 0 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography sx={{ color: '#78716C' }}>
              No notification data available
            </Typography>
          </Box>
        )}
      </Card>
    </Container>
  )
}
