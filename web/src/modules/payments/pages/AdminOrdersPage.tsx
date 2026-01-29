import { useState, useMemo, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Button,
  Tabs,
  Tab,
  Chip,
  Avatar,
  IconButton,
  Menu,
  MenuItem,
  Skeleton,
  Grid,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  TextField,
} from '@mui/material'
import { InlineAlert, TableEmptyStateRow, TableToolbar, StatCard } from '@/shared/ui'
import type { FilterConfig } from '@/shared/ui'
import {
  Refresh,
  Visibility,
  MoreVert,
  LocalShipping,
  Cancel,
  Receipt,
  TrendingUp,
  Pending,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api'
import type { Order, OrderStatus, OrderStats } from '../types'
import { getAdminOrderStatusConfig } from '../utils'
import { formatCurrency, formatDateTime } from '@/shared/utils/formatters'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

function OrderStatsGrid({ stats, loading }: { stats?: OrderStats; loading: boolean }) {
  return (
    <Grid container spacing={3} sx={{ mb: 4 }}>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title="Total Orders"
          value={stats?.totalOrders.toLocaleString() ?? '0'}
          icon={<Receipt />}
          color="#7C3AED"
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title="Pending"
          value={stats?.pendingOrders ?? 0}
          icon={<Pending />}
          color={palette.semantic.warning}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title="Processing"
          value={(stats?.processingOrders ?? 0) + (stats?.shippedOrders ?? 0)}
          icon={<LocalShipping />}
          color={palette.semantic.info}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatCard
          title="Revenue"
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
        <Tooltip title="Actions">
          <IconButton size="small" onClick={(e) => onMenuOpen(e, order)}>
            <MoreVert fontSize="small" />
          </IconButton>
        </Tooltip>
      </TableCell>
    </TableRow>
  )
}

const STATUS_FILTER_OPTIONS = [
  { value: 'pending', label: 'Pending' },
  { value: 'payment_pending', label: 'Payment Pending' },
  { value: 'paid', label: 'Paid' },
  { value: 'shipped', label: 'Shipped' },
  { value: 'delivered', label: 'Delivered' },
  { value: 'completed', label: 'Completed' },
  { value: 'cancelled', label: 'Cancelled' },
  { value: 'refunded', label: 'Refunded' },
]

export function AdminOrdersPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [tabValue, setTabValue] = useState(0)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | ''>('')
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null)
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [cancelReason, setCancelReason] = useState('')

  const filters: FilterConfig[] = useMemo(
    () => [{ key: 'status', label: 'Status', options: STATUS_FILTER_OPTIONS, minWidth: 150 }],
    []
  )

  const filterValues = useMemo(
    () => ({ status: statusFilter }),
    [statusFilter]
  )

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

  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      ordersApi.cancelOrder(id, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] })
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders', 'stats'] })
      setCancelDialogOpen(false)
      setCancelReason('')
      setSelectedOrder(null)
    },
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

  const handleOpenCancelDialog = useCallback(() => {
    setCancelDialogOpen(true)
    handleMenuClose()
  }, [handleMenuClose])

  const handleCancelOrder = useCallback(() => {
    if (!selectedOrder) {return}
    cancelMutation.mutate({ id: selectedOrder.id, reason: cancelReason })
  }, [selectedOrder, cancelReason, cancelMutation])

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
                Order Management
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                View and manage all platform orders
              </Typography>
            </Box>
            <Button
              variant="outlined"
              startIcon={<Refresh />}
              onClick={() => refetch()}
              sx={{ borderColor: 'divider' }}
            >
              Refresh
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
                <Tab label="All Orders" />
                <Tab label="Pending" />
                <Tab label="Paid" />
                <Tab label="Shipped" />
                <Tab label="Completed" />
                <Tab label="Cancelled" />
              </Tabs>
            </Box>

            <Box sx={{ p: 2 }}>
              <TableToolbar
                searchValue={search}
                searchPlaceholder="Search orders by ID, buyer, or seller..."
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
                    <TableCell sx={{ fontWeight: 600 }}>Order</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Buyer</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Seller</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Amount</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600 }}>
                      Actions
                    </TableCell>
                  </TableRow>
                </TableHead>
                {isLoading && <OrderTableSkeleton />}
                {!isLoading && ordersData?.items.length === 0 && (
                  <TableBody>
                    <TableEmptyStateRow
                      colSpan={7}
                      title="No orders found"
                      description="Orders will appear here when customers make purchases"
                      icon={<Receipt sx={{ fontSize: 64, color: 'grey.300' }} />}
                      cellSx={{ py: 8 }}
                    />
                  </TableBody>
                )}
                {!isLoading && ordersData?.items.length > 0 && (
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
          View Details
        </MenuItem>
        {selectedOrder && ['pending', 'payment_pending', 'paid'].includes(selectedOrder.status) && (
          <MenuItem onClick={handleOpenCancelDialog} sx={{ color: 'error.main' }}>
            <Cancel fontSize="small" sx={{ mr: 1.5 }} />
            Cancel Order
          </MenuItem>
        )}
      </Menu>

      <Dialog
        open={cancelDialogOpen}
        onClose={() => setCancelDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600, color: 'error.main' }}>Cancel Order</DialogTitle>
        <DialogContent>
          <InlineAlert severity="warning" sx={{ mb: 3 }}>
            This will cancel order #{selectedOrder?.id.slice(0, 8).toUpperCase()}. This action
            cannot be undone.
          </InlineAlert>
          <TextField
            fullWidth
            label="Cancellation Reason"
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            multiline
            rows={3}
            placeholder="Provide a reason for cancellation..."
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setCancelDialogOpen(false)} sx={{ color: 'text.secondary' }}>
            Keep Order
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleCancelOrder}
            disabled={cancelMutation.isPending}
          >
            {cancelMutation.isPending ? <CircularProgress size={20} /> : 'Cancel Order'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
