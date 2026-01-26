import { useState } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Grid,
  Card,
  Typography,
  Box,
  Stack,
  LinearProgress,
  Chip,
  List,
  ListItem,
  ListItemText,
  Skeleton,
  ToggleButton,
  ToggleButtonGroup,
  Alert,
  Divider,
} from '@mui/material'
import {
  People,
  Gavel,
  AttachMoney,
  Report as ReportIcon,
  TrendingUp,
  TrendingDown,
  CheckCircle,
  Warning,
  Error as ErrorIcon,
  ShoppingCart,
} from '@mui/icons-material'
import {
  useAdminDashboardStats,
  useDashboardActivity,
  usePlatformHealth,
  usePlatformAnalytics,
  useCategoryPerformance,
  useTopPerformers,
} from '../hooks/useAnalytics'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { formatCurrency, formatNumber, formatPercentage } from '@/shared/utils/formatters'
import type { AdminDashboardStats, CategoryBreakdown } from '../types'

interface StatCardConfig {
  key: keyof AdminDashboardStats
  label: string
  icon: React.ReactNode
  color: string
  format: 'currency' | 'number'
  changeKey: keyof AdminDashboardStats
}

const statCards: StatCardConfig[] = [
  {
    key: 'totalRevenue',
    label: 'Total Revenue',
    icon: <AttachMoney />,
    color: '#10B981',
    format: 'currency',
    changeKey: 'revenueChange',
  },
  {
    key: 'activeUsers',
    label: 'Active Users',
    icon: <People />,
    color: '#3B82F6',
    format: 'number',
    changeKey: 'activeUsersChange',
  },
  {
    key: 'liveAuctions',
    label: 'Live Auctions',
    icon: <Gavel />,
    color: '#CA8A04',
    format: 'number',
    changeKey: 'liveAuctionsChange',
  },
  {
    key: 'pendingReports',
    label: 'Pending Reports',
    icon: <ReportIcon />,
    color: '#EF4444',
    format: 'number',
    changeKey: 'pendingReportsChange',
  },
]

const getHealthIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
    case 'connected':
      return <CheckCircle sx={{ color: 'success.main', fontSize: 20 }} />
    case 'degraded':
    case 'unknown':
      return <Warning sx={{ color: 'warning.main', fontSize: 20 }} />
    default:
      return <ErrorIcon sx={{ color: 'error.main', fontSize: 20 }} />
  }
}

const getHealthColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
    case 'connected':
      return 'success.main'
    case 'degraded':
    case 'unknown':
      return 'warning.main'
    default:
      return 'error.main'
  }
}

export function AdminDashboardPage() {
  const [period, setPeriod] = useState<string>('week')
  const { data: stats, isLoading: statsLoading, error: statsError } = useAdminDashboardStats()
  const { data: activityData, isLoading: activityLoading } = useDashboardActivity(10)
  const { data: health, isLoading: healthLoading } = usePlatformHealth()
  const { data: analytics } = usePlatformAnalytics({ period })
  const { data: categories, isLoading: categoriesLoading } = useCategoryPerformance()
  const { data: topPerformers, isLoading: performersLoading } = useTopPerformers(5, period)

  const handlePeriodChange = (_: React.MouseEvent<HTMLElement>, newPeriod: string | null) => {
    if (newPeriod) {
      setPeriod(newPeriod)
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
              }}
            >
              Admin Dashboard
            </Typography>
            <ToggleButtonGroup
              value={period}
              exclusive
              onChange={handlePeriodChange}
              size="small"
              aria-label="time period"
            >
              <ToggleButton value="day">Day</ToggleButton>
              <ToggleButton value="week">Week</ToggleButton>
              <ToggleButton value="month">Month</ToggleButton>
              <ToggleButton value="year">Year</ToggleButton>
            </ToggleButtonGroup>
          </Box>
        </motion.div>

        {statsError && (
          <Alert severity="error" sx={{ mb: 3 }}>
            Failed to load dashboard statistics. Please try again later.
          </Alert>
        )}

        <Grid container spacing={3} sx={{ mb: 4 }}>
          {statCards.map((card) => {
            const value = stats?.[card.key] as number ?? 0
            const change = stats?.[card.changeKey] as number ?? 0
            return (
              <Grid key={card.key} size={{ xs: 12, sm: 6, md: 3 }}>
                <motion.div variants={staggerItem}>
                  <Card sx={{ p: 3, height: '100%' }}>
                    {statsLoading ? (
                      <Box>
                        <Skeleton width={80} height={20} />
                        <Skeleton width={100} height={40} sx={{ my: 1 }} />
                        <Skeleton width={120} height={20} />
                      </Box>
                    ) : (
                      <>
                        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                          <Box>
                            <Typography variant="body2" color="text.secondary" gutterBottom>
                              {card.label}
                            </Typography>
                            <Typography variant="h4" fontWeight={700}>
                              {card.format === 'currency' ? formatCurrency(value) : formatNumber(value)}
                            </Typography>
                          </Box>
                          <Box
                            sx={{
                              width: 48,
                              height: 48,
                              borderRadius: 2,
                              bgcolor: `${card.color}15`,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              color: card.color,
                            }}
                          >
                            {card.icon}
                          </Box>
                        </Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
                          {change >= 0 ? (
                            <TrendingUp sx={{ fontSize: 16, color: 'success.main' }} />
                          ) : (
                            <TrendingDown sx={{ fontSize: 16, color: 'error.main' }} />
                          )}
                          <Typography
                            variant="body2"
                            sx={{
                              color: change >= 0 ? 'success.main' : 'error.main',
                              fontWeight: 500,
                              ml: 0.5,
                            }}
                          >
                            {formatPercentage(Math.abs(change))}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                            vs last {period}
                          </Typography>
                        </Box>
                      </>
                    )}
                  </Card>
                </motion.div>
              </Grid>
            )
          })}
        </Grid>

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, md: 6 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: '100%' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Orders Overview
                </Typography>
                {statsLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <Skeleton height={60} />
                    <Skeleton height={60} />
                  </Stack>
                ) : (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <ShoppingCart color="primary" />
                        <Typography>Total Orders</Typography>
                      </Box>
                      <Typography variant="h6" fontWeight={600}>
                        {formatNumber(stats?.totalOrders ?? 0)}
                      </Typography>
                    </Box>
                    <Divider />
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <CheckCircle color="success" />
                        <Typography>Completed Orders</Typography>
                      </Box>
                      <Typography variant="h6" fontWeight={600}>
                        {formatNumber(stats?.completedOrders ?? 0)}
                      </Typography>
                    </Box>
                    <Box>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        Completion Rate
                      </Typography>
                      <LinearProgress
                        variant="determinate"
                        value={
                          stats?.totalOrders
                            ? (stats.completedOrders / stats.totalOrders) * 100
                            : 0
                        }
                        sx={{ height: 8, borderRadius: 1 }}
                      />
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        {stats?.totalOrders
                          ? formatPercentage((stats.completedOrders / stats.totalOrders) * 100)
                          : '0%'}
                      </Typography>
                    </Box>
                  </Stack>
                )}
              </Card>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: '100%' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Platform Health
                </Typography>
                {healthLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4].map((i) => (
                      <Skeleton key={i} height={32} />
                    ))}
                  </Stack>
                ) : health ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[
                      { label: 'API Status', status: health.apiStatus },
                      { label: 'Database', status: health.databaseStatus },
                      { label: 'Cache', status: health.cacheStatus },
                      { label: 'Queue', status: health.queueStatus },
                    ].map((item) => (
                      <Box
                        key={item.label}
                        sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
                      >
                        <Typography variant="body2">{item.label}</Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {getHealthIcon(item.status)}
                          <Typography
                            variant="body2"
                            sx={{ color: getHealthColor(item.status), textTransform: 'capitalize' }}
                          >
                            {item.status}
                          </Typography>
                        </Box>
                      </Box>
                    ))}
                    {health.queueJobCount > 0 && (
                      <Alert severity="info" sx={{ mt: 1 }}>
                        {health.queueJobCount} jobs in queue
                      </Alert>
                    )}
                  </Stack>
                ) : (
                  <Typography color="text.secondary">Unable to load health status</Typography>
                )}
              </Card>
            </motion.div>
          </Grid>
        </Grid>

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, lg: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: 400, overflow: 'auto' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Top Categories
                </Typography>
                {categoriesLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4, 5].map((i) => (
                      <Box key={i}>
                        <Skeleton height={20} width="60%" />
                        <Skeleton height={8} sx={{ mt: 1 }} />
                      </Box>
                    ))}
                  </Stack>
                ) : categories && categories.length > 0 ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {categories.slice(0, 5).map((category: CategoryBreakdown) => (
                      <Box key={category.categoryId}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="body2">{category.categoryName}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {formatPercentage(category.percentage)}
                          </Typography>
                        </Box>
                        <LinearProgress
                          variant="determinate"
                          value={category.percentage}
                          sx={{
                            height: 6,
                            borderRadius: 1,
                            bgcolor: 'grey.200',
                            '& .MuiLinearProgress-bar': {
                              bgcolor: '#CA8A04',
                            },
                          }}
                        />
                        <Typography variant="caption" color="text.secondary">
                          {formatNumber(category.auctionCount)} auctions • {formatCurrency(category.revenue)}
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                ) : (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    No category data available
                  </Typography>
                )}
              </Card>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, lg: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: 400, overflow: 'auto' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Top Performers
                </Typography>
                {performersLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4, 5].map((i) => (
                      <Skeleton key={i} height={40} />
                    ))}
                  </Stack>
                ) : topPerformers ? (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Top Sellers
                    </Typography>
                    <List dense disablePadding>
                      {topPerformers.topSellers.slice(0, 3).map((seller, index) => (
                        <ListItem key={seller.sellerId} disablePadding sx={{ py: 0.5 }}>
                          <ListItemText
                            primary={
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Chip label={index + 1} size="small" color="primary" sx={{ minWidth: 24 }} />
                                <Typography variant="body2">{seller.username}</Typography>
                              </Box>
                            }
                            secondary={`${formatCurrency(seller.totalSales)} • ${seller.orderCount} orders`}
                          />
                        </ListItem>
                      ))}
                    </List>
                    <Divider sx={{ my: 2 }} />
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Top Buyers
                    </Typography>
                    <List dense disablePadding>
                      {topPerformers.topBuyers.slice(0, 3).map((buyer, index) => (
                        <ListItem key={buyer.buyerId} disablePadding sx={{ py: 0.5 }}>
                          <ListItemText
                            primary={
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Chip label={index + 1} size="small" color="secondary" sx={{ minWidth: 24 }} />
                                <Typography variant="body2">{buyer.username}</Typography>
                              </Box>
                            }
                            secondary={`${formatCurrency(buyer.totalSpent)} • ${buyer.auctionsWon} won`}
                          />
                        </ListItem>
                      ))}
                    </List>
                  </Box>
                ) : (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    No performer data available
                  </Typography>
                )}
              </Card>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, lg: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: 400, overflow: 'auto' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Recent Activity
                </Typography>
                {activityLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4, 5].map((i) => (
                      <Box key={i} sx={{ display: 'flex', gap: 2 }}>
                        <Skeleton variant="circular" width={32} height={32} />
                        <Box sx={{ flex: 1 }}>
                          <Skeleton height={20} />
                          <Skeleton height={16} width="60%" />
                        </Box>
                      </Box>
                    ))}
                  </Stack>
                ) : activityData && activityData.length > 0 ? (
                  <List dense disablePadding sx={{ mt: 1 }}>
                    {activityData.map((activity) => (
                      <ListItem key={activity.id} disablePadding sx={{ py: 1 }}>
                        <ListItemText
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Chip
                                label={activity.type}
                                size="small"
                                variant="outlined"
                                sx={{ textTransform: 'capitalize' }}
                              />
                              <Typography variant="body2" noWrap sx={{ flex: 1 }}>
                                {activity.message}
                              </Typography>
                            </Box>
                          }
                          secondary={new Date(activity.timestamp).toLocaleString()}
                        />
                      </ListItem>
                    ))}
                  </List>
                ) : (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    No recent activity
                  </Typography>
                )}
              </Card>
            </motion.div>
          </Grid>
        </Grid>

        {analytics && (
          <Grid container spacing={3} sx={{ mt: 3 }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    Auction Metrics
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Live Auctions
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.auctions.liveAuctions)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Success Rate
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatPercentage(analytics.auctions.successRate)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Avg. Final Price
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatCurrency(analytics.auctions.averageFinalPrice)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Ending Today
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.auctions.auctionsEndingToday)}
                      </Typography>
                    </Box>
                  </Stack>
                </Card>
              </motion.div>
            </Grid>

            <Grid size={{ xs: 12, md: 4 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    Bid Metrics
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Total Bids
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.totalBids)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Bids Today
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.bidsToday)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Unique Bidders
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.uniqueBidders)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Avg. Bid Amount
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatCurrency(analytics.bids.averageBidAmount)}
                      </Typography>
                    </Box>
                  </Stack>
                </Card>
              </motion.div>
            </Grid>

            <Grid size={{ xs: 12, md: 4 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    User Metrics
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Total Users
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.totalUsers)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        New Today
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.newUsersToday)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Sellers / Buyers
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.totalSellers)} / {formatNumber(analytics.users.totalBuyers)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        Retention Rate
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatPercentage(analytics.users.userRetentionRate)}
                      </Typography>
                    </Box>
                  </Stack>
                </Card>
              </motion.div>
            </Grid>
          </Grid>
        )}
      </motion.div>
    </Container>
  )
}
