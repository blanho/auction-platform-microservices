import { useState } from 'react'
import {
  Container,
  Typography,
  Box,
  Grid,
  Card,
  Stack,
  Button,
  Chip,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  LinearProgress,
  Alert,
} from '@mui/material'
import {
  TrendingUp,
  TrendingDown,
  Gavel,
  AccountBalanceWallet,
  Visibility,
  EmojiEvents,
  ArrowForward,
  Timer,
  ShoppingCart,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { useUserDashboard, useSellerAnalytics, useQuickStats } from '../hooks/useAnalytics'
import { formatCurrency, formatNumber, formatPercentage } from '@/shared/utils/formatters'

interface StatCardProps {
  title: string
  value: string | number
  change?: number
  changeLabel?: string
  icon: React.ReactNode
  iconBg: string
  loading?: boolean
}

function StatCard({ title, value, change, changeLabel, icon, iconBg, loading }: StatCardProps) {
  if (loading) {
    return (
      <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <Stack direction="row" spacing={2} alignItems="flex-start">
          <Skeleton variant="circular" width={48} height={48} />
          <Box sx={{ flex: 1 }}>
            <Skeleton variant="text" width="60%" />
            <Skeleton variant="text" width="40%" height={32} />
            <Skeleton variant="text" width="50%" />
          </Box>
        </Stack>
      </Card>
    )
  }

  return (
    <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
      <Stack direction="row" spacing={2} alignItems="flex-start">
        <Box
          sx={{
            width: 48,
            height: 48,
            borderRadius: 2,
            bgcolor: iconBg,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {icon}
        </Box>
        <Box sx={{ flex: 1 }}>
          <Typography sx={{ fontSize: '0.875rem', color: '#78716C', mb: 0.5 }}>{title}</Typography>
          <Typography sx={{ fontSize: '1.75rem', fontWeight: 700, color: '#1C1917', lineHeight: 1.2 }}>
            {value}
          </Typography>
          {change !== undefined && (
            <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mt: 0.5 }}>
              {change >= 0 ? (
                <TrendingUp sx={{ fontSize: 16, color: '#16A34A' }} />
              ) : (
                <TrendingDown sx={{ fontSize: 16, color: '#DC2626' }} />
              )}
              <Typography sx={{ fontSize: '0.8125rem', color: change >= 0 ? '#16A34A' : '#DC2626', fontWeight: 500 }}>
                {change >= 0 ? '+' : ''}
                {change}%
              </Typography>
              <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>{changeLabel}</Typography>
            </Stack>
          )}
        </Box>
      </Stack>
    </Card>
  )
}

export const DashboardPage = () => {
  const [timeRange, setTimeRange] = useState<string>('30d')
  const { data: userStats, isLoading: userLoading, error: userError } = useUserDashboard()
  const { data: sellerAnalytics, isLoading: sellerLoading } = useSellerAnalytics(timeRange)
  const { data: quickStats, isLoading: quickLoading } = useQuickStats()

  const handleTimeRangeChange = (_: React.MouseEvent<HTMLElement>, newRange: string | null) => {
    if (newRange) {
      setTimeRange(newRange)
    }
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: '#1C1917',
                mb: 0.5,
              }}
            >
              My Dashboard
            </Typography>
            <Typography sx={{ color: '#78716C' }}>
              Track your auction activity and performance
            </Typography>
          </Box>
          <Button
            component={Link}
            to="/auctions/create"
            variant="contained"
            startIcon={<Gavel />}
            sx={{
              bgcolor: '#CA8A04',
              '&:hover': { bgcolor: '#A16207' },
              textTransform: 'none',
            }}
          >
            Create Auction
          </Button>
        </Stack>
      </Box>

      {userError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Failed to load dashboard data. Please try again later.
        </Alert>
      )}

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Active Auctions"
            value={formatNumber(userStats?.activeAuctions ?? 0)}
            icon={<Gavel sx={{ color: '#CA8A04' }} />}
            iconBg="rgba(202, 138, 4, 0.1)"
            loading={userLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Auctions Won"
            value={formatNumber(userStats?.wonAuctions ?? 0)}
            icon={<EmojiEvents sx={{ color: '#16A34A' }} />}
            iconBg="rgba(22, 163, 74, 0.1)"
            loading={userLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Spent"
            value={formatCurrency(userStats?.totalSpent ?? 0)}
            icon={<ShoppingCart sx={{ color: '#3B82F6' }} />}
            iconBg="rgba(59, 130, 246, 0.1)"
            loading={userLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Total Earned"
            value={formatCurrency(userStats?.totalEarned ?? 0)}
            icon={<AccountBalanceWallet sx={{ color: '#10B981' }} />}
            iconBg="rgba(16, 185, 129, 0.1)"
            loading={userLoading}
          />
        </Grid>
      </Grid>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Bidding Summary
            </Typography>
            {userLoading ? (
              <Stack spacing={2} sx={{ mt: 2 }}>
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} height={50} />
                ))}
              </Stack>
            ) : (
              <Stack spacing={2} sx={{ mt: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Timer color="warning" />
                    <Typography>Active Bids</Typography>
                  </Box>
                  <Chip label={formatNumber(userStats?.activeBids ?? 0)} color="warning" size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <EmojiEvents color="success" />
                    <Typography>Won Auctions</Typography>
                  </Box>
                  <Chip label={formatNumber(userStats?.wonAuctions ?? 0)} color="success" size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <TrendingDown color="error" />
                    <Typography>Lost Auctions</Typography>
                  </Box>
                  <Chip label={formatNumber(userStats?.lostAuctions ?? 0)} color="error" size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Visibility color="info" />
                    <Typography>Watching</Typography>
                  </Box>
                  <Chip label={formatNumber(userStats?.watchingCount ?? 0)} color="info" size="small" />
                </Box>
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
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Quick Stats
            </Typography>
            {quickLoading ? (
              <Stack spacing={2} sx={{ mt: 2 }}>
                {[1, 2, 3, 4].map((i) => (
                  <Skeleton key={i} height={40} />
                ))}
              </Stack>
            ) : quickStats ? (
              <Stack spacing={2} sx={{ mt: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography color="text.secondary">Total Platform Auctions</Typography>
                  <Typography fontWeight={600}>{formatNumber(quickStats.totalAuctions)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography color="text.secondary">Active Auctions</Typography>
                  <Typography fontWeight={600}>{formatNumber(quickStats.activeAuctions)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography color="text.secondary">Total Bids Placed</Typography>
                  <Typography fontWeight={600}>{formatNumber(quickStats.totalBids)}</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography color="text.secondary">Platform Users</Typography>
                  <Typography fontWeight={600}>{formatNumber(quickStats.totalUsers)}</Typography>
                </Box>
              </Stack>
            ) : (
              <Typography color="text.secondary" sx={{ mt: 2 }}>
                Unable to load quick stats
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
        </Grid>
      </Grid>

      <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
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
        </Box>

        {sellerLoading ? (
          <Grid container spacing={3}>
            {[1, 2, 3, 4].map((i) => (
              <Grid key={i} size={{ xs: 12, sm: 6, md: 3 }}>
                <Skeleton height={100} />
              </Grid>
            ))}
          </Grid>
        ) : sellerAnalytics ? (
          <>
            <Grid container spacing={3} sx={{ mb: 3 }}>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" fontWeight={700} color="primary">
                    {formatNumber(sellerAnalytics.totalAuctions)}
                  </Typography>
                  <Typography color="text.secondary">Total Auctions</Typography>
                </Box>
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" fontWeight={700} color="success.main">
                    {formatCurrency(sellerAnalytics.totalRevenue)}
                  </Typography>
                  <Typography color="text.secondary">Total Revenue</Typography>
                </Box>
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" fontWeight={700} color="warning.main">
                    {formatPercentage(sellerAnalytics.successRate)}
                  </Typography>
                  <Typography color="text.secondary">Success Rate</Typography>
                </Box>
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <Box sx={{ textAlign: 'center', p: 2 }}>
                  <Typography variant="h4" fontWeight={700} color="info.main">
                    {formatCurrency(sellerAnalytics.averageFinalPrice)}
                  </Typography>
                  <Typography color="text.secondary">Avg. Final Price</Typography>
                </Box>
              </Grid>
            </Grid>

            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Auction Status
                </Typography>
                <Stack spacing={1}>
                  <Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">Active</Typography>
                      <Typography variant="body2">
                        {sellerAnalytics.activeAuctions} / {sellerAnalytics.totalAuctions}
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={
                        sellerAnalytics.totalAuctions
                          ? (sellerAnalytics.activeAuctions / sellerAnalytics.totalAuctions) * 100
                          : 0
                      }
                      color="warning"
                      sx={{ height: 8, borderRadius: 1 }}
                    />
                  </Box>
                  <Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">Completed</Typography>
                      <Typography variant="body2">
                        {sellerAnalytics.completedAuctions} / {sellerAnalytics.totalAuctions}
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={
                        sellerAnalytics.totalAuctions
                          ? (sellerAnalytics.completedAuctions / sellerAnalytics.totalAuctions) * 100
                          : 0
                      }
                      color="success"
                      sx={{ height: 8, borderRadius: 1 }}
                    />
                  </Box>
                  <Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">Cancelled</Typography>
                      <Typography variant="body2">
                        {sellerAnalytics.cancelledAuctions} / {sellerAnalytics.totalAuctions}
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={
                        sellerAnalytics.totalAuctions
                          ? (sellerAnalytics.cancelledAuctions / sellerAnalytics.totalAuctions) * 100
                          : 0
                      }
                      color="error"
                      sx={{ height: 8, borderRadius: 1 }}
                    />
                  </Box>
                </Stack>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Category Breakdown
                </Typography>
                {sellerAnalytics.categoryBreakdown && sellerAnalytics.categoryBreakdown.length > 0 ? (
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Category</TableCell>
                        <TableCell align="right">Auctions</TableCell>
                        <TableCell align="right">Revenue</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {sellerAnalytics.categoryBreakdown.slice(0, 5).map((cat) => (
                        <TableRow key={cat.categoryName}>
                          <TableCell>{cat.categoryName}</TableCell>
                          <TableCell align="right">{formatNumber(cat.auctionCount)}</TableCell>
                          <TableCell align="right">{formatCurrency(cat.revenue)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                ) : (
                  <Typography color="text.secondary">No category data available</Typography>
                )}
              </Grid>
            </Grid>
          </>
        ) : (
          <Typography color="text.secondary">No seller analytics available</Typography>
        )}
      </Card>
    </Container>
  )
}
