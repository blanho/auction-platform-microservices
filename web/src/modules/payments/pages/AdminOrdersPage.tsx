import { useState, useMemo, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  TextField,
  InputAdornment,
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
  Alert,
  CircularProgress,
  Select,
  FormControl,
  InputLabel,
} from '@mui/material'
import {
  Search,
  Refresh,
  Visibility,
  MoreVert,
  LocalShipping,
  Cancel,
  CheckCircle,
  Receipt,
  AttachMoney,
  TrendingUp,
  Pending,
  Inventory,
  FilterList,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api'
import type { Order, OrderStatus, OrderStats } from '../types'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const STATUS_CONFIG: Record<
  OrderStatus,
  { label: string; color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'; icon: React.ReactElement }
> = {
  pending: { label: 'Pending', color: 'warning', icon: <Pending fontSize="small" /> },
  payment_pending: { label: 'Awaiting Payment', color: 'warning', icon: <Pending fontSize="small" /> },
  paid: { label: 'Paid', color: 'info', icon: <AttachMoney fontSize="small" /> },
  processing: { label: 'Processing', color: 'info', icon: <LocalShipping fontSize="small" /> },
  shipped: { label: 'Shipped', color: 'primary', icon: <LocalShipping fontSize="small" /> },
  delivered: { label: 'Delivered', color: 'success', icon: <Inventory fontSize="small" /> },
  completed: { label: 'Completed', color: 'success', icon: <CheckCircle fontSize="small" /> },
  cancelled: { label: 'Cancelled', color: 'error', icon: <Cancel fontSize="small" /> },
  disputed: { label: 'Disputed', color: 'error', icon: <Cancel fontSize="small" /> },
  refunded: { label: 'Refunded', color: 'default', icon: <Receipt fontSize="small" /> },
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function StatsCard({
  title,
  value,
  icon,
  color,
  loading,
}: {
  title: string
  value: string | number
  icon: React.ReactNode
  color: string
  loading?: boolean
}) {
  if (loading) {
    return (
      <Card sx={{ p: 3 }}>
        <Skeleton variant="circular" width={48} height={48} />
        <Skeleton width={80} sx={{ mt: 2 }} />
        <Skeleton width={60} />
      </Card>
    )
  }

  return (
    <Card
      sx={{
        p: 3,
        display: 'flex',
        alignItems: 'flex-start',
        gap: 2,
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: '0 8px 24px rgba(0,0,0,0.12)',
        },
      }}
    >
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
        <Typography variant="body2" color="text.secondary">
          {title}
        </Typography>
        <Typography variant="h5" fontWeight={700}>
          {value}
        </Typography>
      </Box>
    </Card>
  )
}

function OrderStatsGrid({ stats, loading }: { stats?: OrderStats; loading: boolean }) {
  return (
    <Grid container spacing={3} sx={{ mb: 4 }}>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatsCard
          title="Total Orders"
          value={stats?.totalOrders.toLocaleString() ?? '0'}
          icon={<Receipt />}
          color="#7C3AED"
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatsCard
          title="Pending"
          value={stats?.pendingOrders ?? 0}
          icon={<Pending />}
          color="#F59E0B"
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatsCard
          title="Processing"
          value={(stats?.processingOrders ?? 0) + (stats?.shippedOrders ?? 0)}
          icon={<LocalShipping />}
          color="#3B82F6"
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <StatsCard
          title="Revenue"
          value={formatCurrency(stats?.totalRevenue ?? 0)}
          icon={<TrendingUp />}
          color="#10B981"
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
          <TableCell><Skeleton width={100} /></TableCell>
          <TableCell><Skeleton width={100} /></TableCell>
          <TableCell><Skeleton width={80} /></TableCell>
          <TableCell><Skeleton width={80} /></TableCell>
          <TableCell><Skeleton width={120} /></TableCell>
          <TableCell><Skeleton width={40} /></TableCell>
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
  const statusConfig = STATUS_CONFIG[order.status]

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
          <Avatar
            variant="rounded"
            src={order.auctionImageUrl}
            sx={{ width: 48, height: 48 }}
          >
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
            <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 200, display: 'block' }}>
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
          {formatDate(order.createdAt)}
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

  const { data: ordersData, isLoading, refetch } = useQuery({
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
    if (!selectedOrder) return
    cancelMutation.mutate({ id: selectedOrder.id, reason: cancelReason })
  }, [selectedOrder, cancelReason, cancelMutation])

  const handleTabChange = useCallback((_: React.SyntheticEvent, value: number) => {
    setTabValue(value)
    setPage(0)
    setStatusFilter('')
  }, [])

  const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(0)
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

            <Box sx={{ p: 2, display: 'flex', gap: 2, alignItems: 'center' }}>
              <TextField
                placeholder="Search orders by ID, buyer, or seller..."
                size="small"
                value={search}
                onChange={handleSearchChange}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  },
                }}
                sx={{ width: 350 }}
              />

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Status</InputLabel>
                <Select
                  value={statusFilter}
                  label="Status"
                  onChange={(e) => setStatusFilter(e.target.value as OrderStatus | '')}
                  startAdornment={<FilterList sx={{ mr: 1, color: 'text.secondary' }} />}
                >
                  <MenuItem value="">All</MenuItem>
                  <MenuItem value="pending">Pending</MenuItem>
                  <MenuItem value="payment_pending">Payment Pending</MenuItem>
                  <MenuItem value="paid">Paid</MenuItem>
                  <MenuItem value="shipped">Shipped</MenuItem>
                  <MenuItem value="delivered">Delivered</MenuItem>
                  <MenuItem value="completed">Completed</MenuItem>
                  <MenuItem value="cancelled">Cancelled</MenuItem>
                  <MenuItem value="refunded">Refunded</MenuItem>
                </Select>
              </FormControl>
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
                    <TableCell align="right" sx={{ fontWeight: 600 }}>Actions</TableCell>
                  </TableRow>
                </TableHead>
                {isLoading ? (
                  <OrderTableSkeleton />
                ) : ordersData?.items.length === 0 ? (
                  <TableBody>
                    <TableRow>
                      <TableCell colSpan={7} align="center" sx={{ py: 8 }}>
                        <Receipt sx={{ fontSize: 64, color: 'grey.300', mb: 2 }} />
                        <Typography variant="h6" color="text.secondary">
                          No orders found
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Orders will appear here when customers make purchases
                        </Typography>
                      </TableCell>
                    </TableRow>
                  </TableBody>
                ) : (
                  <TableBody
                    component={motion.tbody}
                    variants={staggerContainer}
                    initial="initial"
                    animate="animate"
                  >
                    {ordersData?.items.map((order) => (
                      <OrderTableRow
                        key={order.id}
                        order={order}
                        onMenuOpen={handleMenuOpen}
                      />
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
          <Alert severity="warning" sx={{ mb: 3 }}>
            This will cancel order #{selectedOrder?.id.slice(0, 8).toUpperCase()}. This action cannot be undone.
          </Alert>
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
