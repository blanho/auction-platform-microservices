import { getCurrentLocale } from '@/i18n'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { formatCurrency, formatNumber, formatPercentage } from '@/shared/utils/formatters'
import {
  AttachMoney,
  CheckCircle,
  Gavel,
  People,
  Report as ReportIcon,
  ShoppingCart,
  TrendingDown,
  TrendingUp,
} from '@mui/icons-material'
import {
  Box,
  Card,
  Chip,
  Container,
  Divider,
  Grid,
  LinearProgress,
  List,
  ListItem,
  ListItemText,
  Skeleton,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  BidMetricsCard,
  DailyStatsChart,
  RealTimeStatsCard,
  TopPerformersTable,
  TrendChart,
} from '../components'
import {
  useAdminDashboardStats,
  useCategoryPerformance,
  useDashboardActivity,
  usePlatformAnalytics,
  usePlatformHealth,
} from '../hooks/useAnalytics'
import type { CategoryBreakdown, StatCardConfig } from '../types'
import { getHealthColor, getHealthIcon } from '../utils'

export function AdminDashboardPage() {
  const { t } = useTranslation('analytics')
  const statCards: StatCardConfig[] = [
    {
      key: 'totalRevenue',
      label: t('stats.totalRevenue'),
      icon: <AttachMoney />,
      color: palette.semantic.success,
      format: 'currency',
      changeKey: 'revenueChange',
    },
    {
      key: 'activeUsers',
      label: t('stats.activeUsers'),
      icon: <People />,
      color: palette.semantic.info,
      format: 'number',
      changeKey: 'activeUsersChange',
    },
    {
      key: 'liveAuctions',
      label: t('stats.liveAuctions'),
      icon: <Gavel />,
      color: palette.brand.primary,
      format: 'number',
      changeKey: 'liveAuctionsChange',
    },
    {
      key: 'pendingReports',
      label: t('stats.pendingReports'),
      icon: <ReportIcon />,
      color: palette.semantic.error,
      format: 'number',
      changeKey: 'pendingReportsChange',
    },
  ]
  const [period, setPeriod] = useState<string>('week')
  const { data: stats, isLoading: statsLoading, error: statsError } = useAdminDashboardStats()
  const { data: activityData, isLoading: activityLoading } = useDashboardActivity(10)
  const { data: health, isLoading: healthLoading } = usePlatformHealth()
  const { data: analytics } = usePlatformAnalytics({ period })
  const { data: categories, isLoading: categoriesLoading } = useCategoryPerformance()

  const handlePeriodChange = useCallback(
    (_: React.MouseEvent<HTMLElement>, newPeriod: string | null) => {
      if (newPeriod) {
        setPeriod(newPeriod)
      }
    },
    []
  )

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
              }}
            >
              {t('adminDashboard')}
            </Typography>
            <ToggleButtonGroup
              value={period}
              exclusive
              onChange={handlePeriodChange}
              size="small"
              aria-label={t('period.timeRange')}
            >
              <ToggleButton value="day">{t('period.day')}</ToggleButton>
              <ToggleButton value="week">{t('period.week')}</ToggleButton>
              <ToggleButton value="month">{t('period.month')}</ToggleButton>
              <ToggleButton value="year">{t('period.year')}</ToggleButton>
            </ToggleButtonGroup>
          </Box>
        </motion.div>

        {statsError && (
          <InlineAlert severity="error" sx={{ mb: 3 }}>
            {t('errors.statsLoadFailed')}
          </InlineAlert>
        )}

        <Box sx={{ mb: 4 }}>
          <motion.div variants={staggerItem}>
            <RealTimeStatsCard />
          </motion.div>
        </Box>

        <Grid container spacing={3} sx={{ mb: 4 }}>
          {statCards.map((card) => {
            const value = (stats?.[card.key] as number) ?? 0
            const change = (stats?.[card.changeKey] as number) ?? 0
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
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'flex-start',
                            justifyContent: 'space-between',
                          }}
                        >
                          <Box>
                            <Typography variant="body2" color="text.secondary" gutterBottom>
                              {t(`stats.${card.key}`, { defaultValue: card.label })}
                            </Typography>
                            <Typography variant="h4" fontWeight={700}>
                              {card.format === 'currency'
                                ? formatCurrency(value)
                                : formatNumber(value)}
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
                            {t('stats.vsLast', { period: t(`period.${period}`) })}
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
                  {t('orders.title')}
                </Typography>
                {statsLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <Skeleton height={60} />
                    <Skeleton height={60} />
                  </Stack>
                ) : (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <Box
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                      }}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <ShoppingCart color="primary" />
                        <Typography>{t('orders.total')}</Typography>
                      </Box>
                      <Typography variant="h6" fontWeight={600}>
                        {formatNumber(stats?.totalOrders ?? 0)}
                      </Typography>
                    </Box>
                    <Divider />
                    <Box
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                      }}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <CheckCircle color="success" />
                        <Typography>{t('orders.completed')}</Typography>
                      </Box>
                      <Typography variant="h6" fontWeight={600}>
                        {formatNumber(stats?.completedOrders ?? 0)}
                      </Typography>
                    </Box>
                    <Box>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        {t('stats.completionRate')}
                      </Typography>
                      <LinearProgress
                        variant="determinate"
                        value={
                          stats?.totalOrders ? (stats.completedOrders / stats.totalOrders) * 100 : 0
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
                  {t('health.title')}
                </Typography>
                {healthLoading && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4].map((i) => (
                      <Skeleton key={i} height={32} />
                    ))}
                  </Stack>
                )}
                {!healthLoading && health && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[
                      { label: t('health.apiStatus'), status: health.apiStatus },
                      { label: t('health.database'), status: health.databaseStatus },
                      { label: t('health.cache'), status: health.cacheStatus },
                      { label: t('health.queue'), status: health.queueStatus },
                    ].map((item) => (
                      <Box
                        key={item.label}
                        sx={{
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'space-between',
                        }}
                      >
                        <Typography variant="body2">{item.label}</Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          {getHealthIcon(item.status)}
                          <Typography
                            variant="body2"
                            sx={{ color: getHealthColor(item.status), textTransform: 'capitalize' }}
                          >
                            {t(`health.${item.status.toLowerCase()}`, {
                              defaultValue: item.status,
                            })}
                          </Typography>
                        </Box>
                      </Box>
                    ))}
                    {health.queueJobCount > 0 && (
                      <InlineAlert severity="info" sx={{ mt: 1 }}>
                        {t('health.jobsInQueue', { count: health.queueJobCount })}
                      </InlineAlert>
                    )}
                  </Stack>
                )}
                {!healthLoading && !health && (
                  <Typography color="text.secondary">{t('health.unavailable')}</Typography>
                )}
              </Card>
            </motion.div>
          </Grid>
        </Grid>

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, lg: 8 }}>
            <motion.div variants={staggerItem}>
              <TrendChart />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, lg: 4 }}>
            <motion.div variants={staggerItem}>
              <TopPerformersTable />
            </motion.div>
          </Grid>
        </Grid>

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, lg: 6 }}>
            <motion.div variants={staggerItem}>
              <BidMetricsCard />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, lg: 6 }}>
            <motion.div variants={staggerItem}>
              <DailyStatsChart />
            </motion.div>
          </Grid>
        </Grid>

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, lg: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: 400, overflow: 'auto' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  {t('categories.title')}
                </Typography>
                {categoriesLoading && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4, 5].map((i) => (
                      <Box key={i}>
                        <Skeleton height={20} width="60%" />
                        <Skeleton height={8} sx={{ mt: 1 }} />
                      </Box>
                    ))}
                  </Stack>
                )}
                {!categoriesLoading && categories && categories.length > 0 && (
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
                          {t('categories.auctions', {
                            count: category.auctionCount,
                          })}{' '}
                          • {formatCurrency(category.revenue)}
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                )}
                {!categoriesLoading && (!categories || categories.length === 0) && (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    {t('user.noCategoryData')}
                  </Typography>
                )}
              </Card>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, lg: 8 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, height: 400, overflow: 'auto' }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  {t('activity.title')}
                </Typography>
                {activityLoading && (
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
                )}
                {!activityLoading && activityData && activityData.length > 0 && (
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
                          secondary={new Date(activity.timestamp).toLocaleString(
                            getCurrentLocale()
                          )}
                        />
                      </ListItem>
                    ))}
                  </List>
                )}
                {!activityLoading && (!activityData || activityData.length === 0) && (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    {t('activity.noActivity')}
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
                    {t('metrics.auctions')}
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.liveAuctions')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.auctions.liveAuctions)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.successRate')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatPercentage(analytics.auctions.successRate)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.avgFinalPrice')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatCurrency(analytics.auctions.averageFinalPrice)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.endingToday')}
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
                    {t('metrics.bids')}
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.totalBids')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.totalBids)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.bidsToday')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.bidsToday)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.uniqueBidders')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.bids.uniqueBidders)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.avgBidAmount')}
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
                    {t('metrics.users')}
                  </Typography>
                  <Stack spacing={1.5} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.totalUsers')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.totalUsers)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.newToday')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.newUsersToday)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.sellersBuyers')}
                      </Typography>
                      <Typography variant="body2" fontWeight={600}>
                        {formatNumber(analytics.users.totalSellers)} /{' '}
                        {formatNumber(analytics.users.totalBuyers)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('metrics.retentionRate')}
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
