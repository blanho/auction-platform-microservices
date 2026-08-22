import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { InlineAlert, StatCard, StatCardSkeleton } from '@/shared/ui'
import { formatCurrency, formatNumber, formatPercentage } from '@/shared/utils/formatters'
import {
  AccountBalanceWallet,
  Add,
  ArrowForward,
  EmojiEvents,
  Gavel,
  ShoppingCart,
  Timer,
  TrendingUp,
  Visibility,
} from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Chip,
  Container,
  Divider,
  Grid,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { CategoryChart, PerformanceMetrics, RevenueChart } from '../components'
import { useQuickStats, useSellerAnalytics, useUserDashboard } from '../hooks/useAnalytics'
import type { CategoryBreakdown, TrendDataPoint } from '../types'
import type { TimeRange } from '../utils/date.utils'

export function UserDashboardPage() {
  const { t } = useTranslation('analytics')
  const [timeRange, setTimeRange] = useState<TimeRange>('30d')
  const { data: userStats, isLoading: userLoading, error: userError } = useUserDashboard()
  const { data: sellerAnalytics, isLoading: sellerLoading } = useSellerAnalytics(timeRange)
  const { data: quickStats, isLoading: quickLoading } = useQuickStats()

  const hasSellerData =
    sellerAnalytics && (sellerAnalytics.totalAuctions > 0 || sellerAnalytics.totalRevenue > 0)

  const handleTimeRangeChange = (_: React.MouseEvent<HTMLElement>, newRange: TimeRange | null) => {
    if (newRange) {
      setTimeRange(newRange)
    }
  }

  const revenueChartData: TrendDataPoint[] =
    sellerAnalytics?.dailyRevenue?.map((d) => ({
      date: d.date,
      value: d.revenue,
      label: formatCurrency(d.revenue),
    })) || []

  const categoryData: CategoryBreakdown[] =
    sellerAnalytics?.categoryBreakdown?.map((c, index, arr) => {
      const totalRevenue = arr.reduce((sum, cat) => sum + cat.revenue, 0)
      return {
        categoryId: `cat-${index}`,
        categoryName: c.categoryName,
        auctionCount: c.auctionCount,
        bidCount: 0,
        revenue: c.revenue,
        percentage: totalRevenue > 0 ? (c.revenue / totalRevenue) * 100 : 0,
      }
    }) || []

  const performanceMetrics = sellerAnalytics
    ? [
        {
          label: t('seller.active'),
          value: sellerAnalytics.activeAuctions,
          total: sellerAnalytics.totalAuctions,
          color: 'warning' as const,
        },
        {
          label: t('seller.completed'),
          value: sellerAnalytics.completedAuctions,
          total: sellerAnalytics.totalAuctions,
          color: 'success' as const,
        },
        {
          label: t('seller.cancelled'),
          value: sellerAnalytics.cancelledAuctions,
          total: sellerAnalytics.totalAuctions,
          color: 'error' as const,
        },
      ]
    : []

  return (
    <Box
      component={motion.div}
      variants={staggerContainer}
      initial="initial"
      animate="animate"
      sx={{ bgcolor: 'background.default', minHeight: '100vh', pb: 6 }}
    >
      <Container maxWidth="xl" sx={{ pt: 4 }}>
        <motion.div variants={fadeInUp}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            justifyContent="space-between"
            alignItems={{ xs: 'flex-start', sm: 'center' }}
            spacing={2}
            sx={{ mb: 4 }}
          >
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: 'text.primary',
                  mb: 0.5,
                }}
              >
                {t('user.title')}
              </Typography>
              <Typography sx={{ color: 'text.secondary', fontSize: '0.9375rem' }}>
                {t('trackActivity')}
              </Typography>
            </Box>
            <Button
              component={Link}
              to="/auctions/create"
              variant="contained"
              startIcon={<Add />}
              sx={{
                bgcolor: 'primary.main',
                '&:hover': { bgcolor: 'primary.dark' },
                textTransform: 'none',
                fontWeight: 600,
                px: 3,
              }}
            >
              {t('user.createAuction')}
            </Button>
          </Stack>
        </motion.div>

        {userError && (
          <InlineAlert severity="error" sx={{ mb: 3 }}>
            {t('errors.loadFailed')}
          </InlineAlert>
        )}

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
            <motion.div variants={staggerItem}>
              {userLoading ? (
                <StatCardSkeleton />
              ) : (
                <StatCard
                  title={t('user.activeAuctions')}
                  value={formatNumber(userStats?.activeAuctions ?? 0)}
                  icon={<Gavel />}
                  iconBg="rgba(202, 138, 4, 0.12)"
                  iconColor="#CA8A04"
                />
              )}
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
            <motion.div variants={staggerItem}>
              {userLoading ? (
                <StatCardSkeleton />
              ) : (
                <StatCard
                  title={t('user.auctionsWon')}
                  value={formatNumber(userStats?.wonAuctions ?? 0)}
                  icon={<EmojiEvents />}
                  iconBg="rgba(22, 163, 74, 0.12)"
                  iconColor="#16A34A"
                />
              )}
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
            <motion.div variants={staggerItem}>
              {userLoading ? (
                <StatCardSkeleton />
              ) : (
                <StatCard
                  title={t('user.totalSpent')}
                  value={formatCurrency(userStats?.totalSpent ?? 0)}
                  icon={<ShoppingCart />}
                  iconBg="rgba(59, 130, 246, 0.12)"
                  iconColor="#3B82F6"
                />
              )}
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
            <motion.div variants={staggerItem}>
              {userLoading ? (
                <StatCardSkeleton />
              ) : (
                <StatCard
                  title={t('user.totalEarned')}
                  value={formatCurrency(userStats?.totalEarned ?? 0)}
                  icon={<AccountBalanceWallet />}
                  iconBg="rgba(16, 185, 129, 0.12)"
                  iconColor="#10B981"
                />
              )}
            </motion.div>
          </Grid>
        </Grid>

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, md: 6 }}>
            <motion.div variants={staggerItem}>
              <Card
                sx={{
                  p: 3,
                  borderRadius: 2,
                  boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
                  border: '1px solid',
                  borderColor: 'divider',
                  height: '100%',
                }}
              >
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  {t('user.biddingSummary')}
                </Typography>
                {userLoading ? (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4].map((i) => (
                      <Skeleton key={i} height={44} />
                    ))}
                  </Stack>
                ) : (
                  <Stack spacing={0} sx={{ mt: 2 }}>
                    <SummaryRow
                      icon={<Timer sx={{ color: 'warning.main' }} />}
                      label={t('user.activeBids')}
                      value={userStats?.activeBids ?? 0}
                      chipColor="warning"
                    />
                    <Divider />
                    <SummaryRow
                      icon={<EmojiEvents sx={{ color: 'success.main' }} />}
                      label={t('user.wonAuctions')}
                      value={userStats?.wonAuctions ?? 0}
                      chipColor="success"
                    />
                    <Divider />
                    <SummaryRow
                      icon={
                        <TrendingUp sx={{ color: 'error.main', transform: 'rotate(180deg)' }} />
                      }
                      label={t('user.lostAuctions')}
                      value={userStats?.lostAuctions ?? 0}
                      chipColor="error"
                    />
                    <Divider />
                    <SummaryRow
                      icon={<Visibility sx={{ color: 'info.main' }} />}
                      label={t('user.watching')}
                      value={userStats?.watchingCount ?? 0}
                      chipColor="info"
                    />
                  </Stack>
                )}
                <Button
                  component={Link}
                  to="/bids"
                  fullWidth
                  endIcon={<ArrowForward />}
                  sx={{ mt: 3, textTransform: 'none' }}
                >
                  {t('user.viewAllBids')}
                </Button>
              </Card>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <motion.div variants={staggerItem}>
              <Card
                sx={{
                  p: 3,
                  borderRadius: 2,
                  boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
                  border: '1px solid',
                  borderColor: 'divider',
                  height: '100%',
                }}
              >
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  {t('user.platformStats')}
                </Typography>
                {quickLoading && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {[1, 2, 3, 4].map((i) => (
                      <Skeleton key={i} height={36} />
                    ))}
                  </Stack>
                )}
                {!quickLoading && quickStats && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <QuickStatRow
                      label={t('user.platformAuctions')}
                      value={formatNumber(quickStats.totalAuctions)}
                    />
                    <QuickStatRow
                      label={t('user.activeAuctionsLabel')}
                      value={formatNumber(quickStats.activeAuctions)}
                    />
                    <QuickStatRow
                      label={t('user.totalBidsPlaced')}
                      value={formatNumber(quickStats.totalBids)}
                    />
                    <QuickStatRow
                      label={t('user.platformUsers')}
                      value={formatNumber(quickStats.totalUsers)}
                    />
                  </Stack>
                )}
                {!quickLoading && !quickStats && (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    {t('user.platformStatsUnavailable')}
                  </Typography>
                )}
                <Button
                  component={Link}
                  to="/auctions"
                  fullWidth
                  endIcon={<ArrowForward />}
                  sx={{ mt: 3, textTransform: 'none' }}
                >
                  {t('user.browseAuctions')}
                </Button>
              </Card>
            </motion.div>
          </Grid>
        </Grid>

        <motion.div variants={fadeInUp}>
          <Card
            sx={{
              p: 3,
              borderRadius: 2,
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
              <Typography variant="h6" fontWeight={600}>
                {t('seller.title')}
              </Typography>
              <ToggleButtonGroup
                value={timeRange}
                exclusive
                onChange={handleTimeRangeChange}
                size="small"
                aria-label={t('period.timeRange')}
              >
                <ToggleButton value="7d">{t('period.7d')}</ToggleButton>
                <ToggleButton value="30d">{t('period.30d')}</ToggleButton>
                <ToggleButton value="90d">{t('period.90d')}</ToggleButton>
                <ToggleButton value="1y">{t('period.1y')}</ToggleButton>
              </ToggleButtonGroup>
            </Stack>

            {sellerLoading && (
              <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Skeleton height={100} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Skeleton height={100} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Skeleton height={100} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                  <Skeleton height={100} />
                </Grid>
              </Grid>
            )}
            {!sellerLoading && sellerAnalytics && (
              <>
                <Grid container spacing={3} sx={{ mb: 4 }}>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatNumber(sellerAnalytics.totalAuctions)}
                      label={t('seller.totalAuctions')}
                      color="primary.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatCurrency(sellerAnalytics.totalRevenue)}
                      label={t('seller.totalRevenue')}
                      color="success.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatPercentage(sellerAnalytics.successRate)}
                      label={t('seller.successRate')}
                      color="warning.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatCurrency(sellerAnalytics.averageFinalPrice)}
                      label={t('seller.avgFinalPrice')}
                      color="info.main"
                    />
                  </Grid>
                </Grid>

                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, lg: 8 }}>
                    <RevenueChart
                      data={revenueChartData}
                      isLoading={sellerLoading}
                      title={t('user.revenueOverTime')}
                      height={280}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, lg: 4 }}>
                    <PerformanceMetrics
                      metrics={performanceMetrics}
                      isLoading={sellerLoading}
                      title={t('seller.auctionStatus')}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <CategoryChart
                      data={categoryData}
                      isLoading={sellerLoading}
                      title={t('user.revenueByCategory')}
                      dataKey="revenue"
                      height={250}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <CategoryTable data={categoryData} isLoading={sellerLoading} />
                  </Grid>
                </Grid>
              </>
            )}
            {!sellerLoading && !hasSellerData && (
              <Box sx={{ py: 6, textAlign: 'center' }}>
                <Typography color="text.secondary">{t('user.noSellerAnalytics')}</Typography>
                <Button
                  component={Link}
                  to="/auctions/create"
                  variant="outlined"
                  sx={{ mt: 2, textTransform: 'none' }}
                >
                  {t('user.createFirstAuction')}
                </Button>
              </Box>
            )}
          </Card>
        </motion.div>
      </Container>
    </Box>
  )
}

function SummaryRow({
  icon,
  label,
  value,
  chipColor,
}: {
  icon: React.ReactNode
  label: string
  value: number
  chipColor: 'warning' | 'success' | 'error' | 'info'
}) {
  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        py: 1.5,
        cursor: 'pointer',
        transition: 'background-color 0.15s',
        mx: -1,
        px: 1,
        borderRadius: 1,
        '&:hover': {
          bgcolor: 'action.hover',
        },
      }}
    >
      <Stack direction="row" alignItems="center" spacing={1.5}>
        {icon}
        <Typography sx={{ fontSize: '0.9375rem' }}>{label}</Typography>
      </Stack>
      <Chip
        label={formatNumber(value)}
        color={chipColor}
        size="small"
        sx={{ fontWeight: 600, minWidth: 48 }}
      />
    </Box>
  )
}

function QuickStatRow({ label, value }: { label: string; value: string }) {
  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        py: 0.75,
      }}
    >
      <Typography color="text.secondary" sx={{ fontSize: '0.9375rem' }}>
        {label}
      </Typography>
      <Typography fontWeight={600} sx={{ fontFamily: '"Fira Code", monospace' }}>
        {value}
      </Typography>
    </Box>
  )
}

function MetricBox({ value, label, color }: { value: string; label: string; color: string }) {
  return (
    <Box
      sx={{
        textAlign: 'center',
        p: 2,
        borderRadius: 2,
        bgcolor: 'action.hover',
        cursor: 'default',
        transition: 'transform 0.2s',
        '&:hover': {
          transform: 'scale(1.02)',
        },
      }}
    >
      <Typography
        variant="h4"
        fontWeight={700}
        sx={{
          color,
          fontFamily: '"Fira Sans", sans-serif',
          fontSize: { xs: '1.5rem', sm: '2rem' },
        }}
      >
        {value}
      </Typography>
      <Typography color="text.secondary" sx={{ fontSize: '0.8125rem', mt: 0.5 }}>
        {label}
      </Typography>
    </Box>
  )
}

function CategoryTable({ data, isLoading }: { data: CategoryBreakdown[]; isLoading?: boolean }) {
  const { t } = useTranslation('analytics')
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
        {[1, 2, 3, 4, 5].map((i) => (
          <Skeleton key={i} height={40} sx={{ mb: 1 }} />
        ))}
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
        sx={{ fontFamily: '"Fira Sans", sans-serif' }}
      >
        {t('seller.categoryBreakdown')}
      </Typography>
      {data.length > 0 ? (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600 }}>{t('seller.category')}</TableCell>
              <TableCell align="right" sx={{ fontWeight: 600 }}>
                {t('seller.auctions')}
              </TableCell>
              <TableCell align="right" sx={{ fontWeight: 600 }}>
                {t('seller.revenue')}
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.slice(0, 5).map((cat) => (
              <TableRow
                key={cat.categoryId || cat.categoryName}
                sx={{
                  cursor: 'pointer',
                  '&:hover': { bgcolor: 'action.hover' },
                }}
              >
                <TableCell sx={{ py: 1.5 }}>{cat.categoryName}</TableCell>
                <TableCell align="right" sx={{ py: 1.5, fontFamily: '"Fira Code", monospace' }}>
                  {formatNumber(cat.auctionCount)}
                </TableCell>
                <TableCell align="right" sx={{ py: 1.5, fontFamily: '"Fira Code", monospace' }}>
                  {formatCurrency(cat.revenue)}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      ) : (
        <Typography color="text.secondary" sx={{ py: 2 }}>
          {t('user.noCategoryData')}
        </Typography>
      )}
    </Box>
  )
}
