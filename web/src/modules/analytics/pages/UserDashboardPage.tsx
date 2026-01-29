import { useState } from 'react'
import {
  Container,
  Typography,
  Box,
  Grid,
  Stack,
  Button,
  ToggleButton,
  ToggleButtonGroup,
  Card,
  Divider,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Skeleton,
} from '@mui/material'
import { InlineAlert, StatCard, StatCardSkeleton } from '@/shared/ui'
import {
  Gavel,
  AccountBalanceWallet,
  EmojiEvents,
  Visibility,
  ArrowForward,
  TrendingUp,
  Timer,
  ShoppingCart,
  Add,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  RevenueChart,
  CategoryChart,
  PerformanceMetrics,
} from '../components'
import { useUserDashboard, useSellerAnalytics, useQuickStats } from '../hooks/useAnalytics'
import { formatCurrency, formatNumber, formatPercentage } from '@/shared/utils/formatters'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import type { TimeRange } from '../utils/date.utils'
import type { TrendDataPoint, CategoryBreakdown } from '../types'

export function UserDashboardPage() {
  const [timeRange, setTimeRange] = useState<TimeRange>('30d')
  const { data: userStats, isLoading: userLoading, error: userError } = useUserDashboard()
  const { data: sellerAnalytics, isLoading: sellerLoading } = useSellerAnalytics(timeRange)
  const { data: quickStats, isLoading: quickLoading } = useQuickStats()

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
          label: 'Active Auctions',
          value: sellerAnalytics.activeAuctions,
          total: sellerAnalytics.totalAuctions,
          color: 'warning' as const,
        },
        {
          label: 'Completed',
          value: sellerAnalytics.completedAuctions,
          total: sellerAnalytics.totalAuctions,
          color: 'success' as const,
        },
        {
          label: 'Cancelled',
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
                My Dashboard
              </Typography>
              <Typography sx={{ color: 'text.secondary', fontSize: '0.9375rem' }}>
                Track your auction activity and performance metrics
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
              Create Auction
            </Button>
          </Stack>
        </motion.div>

        {userError && (
          <InlineAlert severity="error" sx={{ mb: 3 }}>
            Failed to load dashboard data. Please try again later.
          </InlineAlert>
        )}

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
            <motion.div variants={staggerItem}>
              {userLoading ? (
                <StatCardSkeleton />
              ) : (
                <StatCard
                  title="Active Auctions"
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
                  title="Auctions Won"
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
                  title="Total Spent"
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
                  title="Total Earned"
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
                  Bidding Summary
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
                      label="Active Bids"
                      value={userStats?.activeBids ?? 0}
                      chipColor="warning"
                    />
                    <Divider />
                    <SummaryRow
                      icon={<EmojiEvents sx={{ color: 'success.main' }} />}
                      label="Won Auctions"
                      value={userStats?.wonAuctions ?? 0}
                      chipColor="success"
                    />
                    <Divider />
                    <SummaryRow
                      icon={
                        <TrendingUp sx={{ color: 'error.main', transform: 'rotate(180deg)' }} />
                      }
                      label="Lost Auctions"
                      value={userStats?.lostAuctions ?? 0}
                      chipColor="error"
                    />
                    <Divider />
                    <SummaryRow
                      icon={<Visibility sx={{ color: 'info.main' }} />}
                      label="Watching"
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
                  View All Bids
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
                  Platform Stats
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
                      label="Total Platform Auctions"
                      value={formatNumber(quickStats.totalAuctions)}
                    />
                    <QuickStatRow
                      label="Active Auctions"
                      value={formatNumber(quickStats.activeAuctions)}
                    />
                    <QuickStatRow
                      label="Total Bids Placed"
                      value={formatNumber(quickStats.totalBids)}
                    />
                    <QuickStatRow
                      label="Platform Users"
                      value={formatNumber(quickStats.totalUsers)}
                    />
                  </Stack>
                )}
                {!quickLoading && !quickStats && (
                  <Typography color="text.secondary" sx={{ mt: 2 }}>
                    Unable to load platform stats
                  </Typography>
                )}
                <Button
                  component={Link}
                  to="/auctions"
                  fullWidth
                  endIcon={<ArrowForward />}
                  sx={{ mt: 3, textTransform: 'none' }}
                >
                  Browse Auctions
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
                Seller Analytics
              </Typography>
              <ToggleButtonGroup
                value={timeRange}
                exclusive
                onChange={handleTimeRangeChange}
                size="small"
                aria-label="time range"
              >
                <ToggleButton value="7d">7D</ToggleButton>
                <ToggleButton value="30d">30D</ToggleButton>
                <ToggleButton value="90d">90D</ToggleButton>
                <ToggleButton value="1y">1Y</ToggleButton>
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
                      label="Total Auctions"
                      color="primary.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatCurrency(sellerAnalytics.totalRevenue)}
                      label="Total Revenue"
                      color="success.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatPercentage(sellerAnalytics.successRate)}
                      label="Success Rate"
                      color="warning.main"
                    />
                  </Grid>
                  <Grid size={{ xs: 6, md: 3 }}>
                    <MetricBox
                      value={formatCurrency(sellerAnalytics.averageFinalPrice)}
                      label="Avg. Final Price"
                      color="info.main"
                    />
                  </Grid>
                </Grid>

                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, lg: 8 }}>
                    <RevenueChart
                      data={revenueChartData}
                      isLoading={sellerLoading}
                      title="Revenue Over Time"
                      height={280}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, lg: 4 }}>
                    <PerformanceMetrics
                      metrics={performanceMetrics}
                      isLoading={sellerLoading}
                      title="Auction Status"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <CategoryChart
                      data={categoryData}
                      isLoading={sellerLoading}
                      title="Revenue by Category"
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
                <Typography color="text.secondary">
                  No seller analytics available. Start selling to see your performance metrics.
                </Typography>
                <Button
                  component={Link}
                  to="/auctions/create"
                  variant="outlined"
                  sx={{ mt: 2, textTransform: 'none' }}
                >
                  Create Your First Auction
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
        Category Breakdown
      </Typography>
      {data.length > 0 ? (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600 }}>Category</TableCell>
              <TableCell align="right" sx={{ fontWeight: 600 }}>
                Auctions
              </TableCell>
              <TableCell align="right" sx={{ fontWeight: 600 }}>
                Revenue
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
          No category data available
        </Typography>
      )}
    </Box>
  )
}
