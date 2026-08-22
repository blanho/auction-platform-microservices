import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import type { FilterConfig } from '@/shared/ui'
import { StatCard, TableEmptyStateRow, TableToolbar } from '@/shared/ui'
import { formatCurrency, formatDateTime, formatNumber } from '@/shared/utils/formatters'
import {
  LocalShipping,
  MoreVert,
  Pending,
  Receipt,
  Refresh,
  TrendingUp,
  Visibility,
} from '@mui/icons-material'
import {
  Avatar,
  Box,
  Button,
  Card,
  Chip,
  Container,
  Grid,
  IconButton,
  Menu,
  MenuItem,
  Skeleton,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Tabs,
  Tooltip,
  Typography,
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useCallback, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { ordersApi } from '../api'
import type { Order, OrderStats, OrderStatus } from '../types'
import { getAdminOrderStatusConfig } from '../utils'

function OrderStatsGrid({ stats, loading }: { stats?: OrderStats; loading: boolean }) {
  const { t } = useTranslation('payments')
  return (
    <Grid container spacing={3} sx={{ mb: 4 }}>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title={t('adminOrders.totalOrders')}
          value={formatNumber(stats?.totalOrders ?? 0)}
          icon={<Receipt />}
          color="#7C3AED"
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title={t('adminOrders.pending')}
          value={stats?.pendingOrders ?? 0}
          icon={<Pending />}
          color={palette.semantic.warning}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title={t('adminOrders.processing')}
          value={(stats?.processingOrders ?? 0) + (stats?.shippedOrders ?? 0)}
          icon={<LocalShipping />}
          color={palette.semantic.info}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title={t('adminOrders.revenue')}
          value={formatCurrency(stats?.totalRevenue ?? 0)}
          icon={<TrendingUp />}
          color={palette.semantic.success}
          loading={loading}
        />
      </Grid>
    </Grid>
  )
}

function OrderTableSkeleton() {
  return (
    <TableBody>
      {[...Array(5)].map((_, i) => (
        <TableRow key={i}>
          <TableCell>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Skeleton variant="rounded" width={48} height={48} />
              <Box>
                <Skeleton width={150} />
                <Skeleton width={100} />
              </Box>
            </Box>
          </TableCell>
          <TableCell>
            <Skeleton width={100} />
          </TableCell>
          <TableCell>
            <Skeleton width={100} />
          </TableCell>
          <TableCell>
            <Skeleton width={80} />
          </TableCell>
          <TableCell>
            <Skeleton width={80} />
          </TableCell>
          <TableCell>
            <Skeleton width={120} />
          </TableCell>
          <TableCell>
            <Skeleton width={40} />
          </TableCell>
        </TableRow>
      ))}
    </TableBody>
  )
}

function OrderTableRow({
  order,
  onMenuOpen,
}: {
  order: Order
  onMenuOpen: (event: React.MouseEvent<HTMLElement>, order: Order) => void
}) {
  const { t } = useTranslation('payments')
  const statusConfig = getAdminOrderStatusConfig(order.status)

  return (
    <TableRow
      component={motion.tr}
      variants={staggerItem}
      sx={{
        '&:hover': { bgcolor: 'action.hover' },
        cursor: 'pointer',
      }}
    >
      <TableCell>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Avatar variant="rounded" src={order.auctionImageUrl} sx={{ width: 48, height: 48 }}>
            <Receipt />
          </Avatar>
          <Box>
            <Typography
              variant="subtitle2"
              fontWeight={600}
              component={Link}
              to={`/orders/${order.id}`}
              sx={{
                color: 'text.primary',
                textDecoration: 'none',
                '&:hover': { color: 'primary.main' },
              }}
            >
              #{order.id.slice(0, 8).toUpperCase()}
            </Typography>
            <Typography
              variant="caption"
              color="text.secondary"
              noWrap
              sx={{ maxWidth: 200, display: 'block' }}
            >
              {order.auctionTitle || order.itemTitle}
            </Typography>
          </Box>
        </Box>
      </TableCell>
      <TableCell>
        <Typography variant="body2">{order.buyerName || order.buyerUsername}</Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2">{order.sellerName || order.sellerUsername}</Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2" fontWeight={600}>
          {formatCurrency(order.totalAmount)}
        </Typography>
      </TableCell>
      <TableCell>
        <Chip
          icon={statusConfig.icon}
          label={statusConfig.label}
          color={statusConfig.color}
          size="small"
          sx={{ fontWeight: 500 }}
        />
      </TableCell>
      <TableCell>
        <Typography variant="body2" color="text.secondary">
          {formatDateTime(order.createdAt)}
        </Typography>
      </TableCell>
      <TableCell align="right">
        <Tooltip title={t('adminOrders.actions')}>
          <IconButton size="small" onClick={(e) => onMenuOpen(e, order)}>
            <MoreVert fontSize="small" />
          </IconButton>
        </Tooltip>
      </TableCell>
    </TableRow>
  )
}

const STATUS_FILTER_VALUES: OrderStatus[] = [
  'pending',
  'payment_pending',
  'paid',
  'shipped',
  'delivered',
  'completed',
  'cancelled',
  'refunded',
]

export function AdminOrdersPage() {
  const { t } = useTranslation('payments')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [tabValue, setTabValue] = useState(0)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | ''>('')
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null)
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null)

  const filters: FilterConfig[] = useMemo(
    () => [
      {
        key: 'status',
        label: t('orders.status'),
        options: STATUS_FILTER_VALUES.map((value) => ({
          value,
          label: t(`orderStatuses.${value}`),
        })),
        minWidth: 150,
      },
    ],
    [t]
  )

  const filterValues = useMemo(() => ({ status: statusFilter }), [statusFilter])

  const handleFilterChange = useCallback((key: string, value: string) => {
    if (key === 'status') {
      setStatusFilter(value as OrderStatus | '')
      setPage(0)
    }
  }, [])

  const handleClearFilters = useCallback(() => {
    setSearch('')
    setStatusFilter('')
    setPage(0)
  }, [])

  const statusFromTab = useMemo(() => {
    const tabStatuses: (OrderStatus | undefined)[] = [
      undefined,
      'pending',
      'paid',
      'shipped',
      'completed',
      'cancelled',
    ]
    return tabStatuses[tabValue]
  }, [tabValue])

  const {
    data: ordersData,
    isLoading,
    refetch,
  } = useQuery({
    queryKey: ['admin', 'orders', page, rowsPerPage, search, statusFromTab, statusFilter],
    queryFn: async () => {
      const response = await ordersApi.getAllOrders({
        page: page + 1,
        pageSize: rowsPerPage,
        search: search || undefined,
        status: statusFilter || statusFromTab,
      })
      return response
    },
  })

  const { data: statsData, isLoading: statsLoading } = useQuery({
    queryKey: ['admin', 'orders', 'stats'],
    queryFn: () => ordersApi.getOrderStats(),
  })

  const handleMenuOpen = useCallback((event: React.MouseEvent<HTMLElement>, order: Order) => {
    setMenuAnchor(event.currentTarget)
    setSelectedOrder(order)
  }, [])

  const handleMenuClose = useCallback(() => {
    setMenuAnchor(null)
  }, [])

  const handleViewOrder = useCallback(() => {
    handleMenuClose()
  }, [handleMenuClose])

  const handleTabChange = useCallback((_: React.SyntheticEvent, value: number) => {
    setTabValue(value)
    setPage(0)
    setStatusFilter('')
  }, [])

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Fira Sans", sans-serif',
                  fontWeight: 700,
                  color: 'text.primary',
                }}
              >
                {t('adminOrders.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {t('adminOrders.description')}
              </Typography>
            </Box>
            <Button
              variant="outlined"
              startIcon={<Refresh />}
              onClick={() => refetch()}
              sx={{ borderColor: 'divider' }}
            >
              {t('common:refresh')}
            </Button>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <OrderStatsGrid stats={statsData} loading={statsLoading} />
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={tabValue} onChange={handleTabChange}>
                <Tab label={t('adminOrders.allOrders')} />
                <Tab label={t('orderStatuses.pending')} />
                <Tab label={t('orderStatuses.paid')} />
                <Tab label={t('orderStatuses.shipped')} />
                <Tab label={t('orderStatuses.completed')} />
                <Tab label={t('orderStatuses.cancelled')} />
              </Tabs>
            </Box>

            <Box sx={{ p: 2 }}>
              <TableToolbar
                searchValue={search}
                searchPlaceholder={t('adminOrders.searchPlaceholder')}
                onSearchChange={setSearch}
                filters={filters}
                filterValues={filterValues}
                onFilterChange={handleFilterChange}
                onClearFilters={handleClearFilters}
                onRefresh={() => refetch()}
                showRefreshButton={false}
              />
            </Box>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>{t('adminOrders.order')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{t('orders.buyer')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{t('orders.seller')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{t('orders.amount')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{t('orders.status')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{t('orders.date')}</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>
                      {t('adminOrders.actions')}
                    </TableCell>
                  </TableRow>
                </TableHead>
                {isLoading && <OrderTableSkeleton />}
                {!isLoading && (ordersData?.items?.length ?? 0) === 0 && (
                  <TableBody>
                    <TableEmptyStateRow
                      colSpan={7}
                      title={t('adminOrders.noOrders')}
                      description={t('adminOrders.noOrdersDescription')}
                      icon={<Receipt sx={{ fontSize: 64, color: 'grey.300' }} />}
                      cellSx={{ py: 8 }}
                    />
                  </TableBody>
                )}
                {!isLoading && (ordersData?.items?.length ?? 0) > 0 && (
                  <TableBody
                    component={motion.tbody}
                    variants={staggerContainer}
                    initial="initial"
                    animate="animate"
                  >
                    {ordersData?.items.map((order) => (
                      <OrderTableRow key={order.id} order={order} onMenuOpen={handleMenuOpen} />
                    ))}
                  </TableBody>
                )}
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={ordersData?.totalCount || 0}
              page={page}
              onPageChange={(_, newPage) => setPage(newPage)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) => {
                setRowsPerPage(parseInt(e.target.value))
                setPage(0)
              }}
              rowsPerPageOptions={[10, 25, 50, 100]}
            />
          </Card>
        </motion.div>
      </motion.div>

      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        <MenuItem
          component={Link}
          to={selectedOrder ? `/orders/${selectedOrder.id}` : '#'}
          onClick={handleViewOrder}
        >
          <Visibility fontSize="small" sx={{ mr: 1.5 }} />
          {t('adminOrders.viewDetails')}
        </MenuItem>
      </Menu>
    </Container>
  )
}
