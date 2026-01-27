import { useState } from 'react'
import { motion } from 'framer-motion'
import { Link, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Container,
  Typography,
  Box,
  Card,
  Button,
  Tabs,
  Tab,
  Chip,
  Skeleton,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Avatar,
  IconButton,
  Pagination,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
} from '@mui/material'
import {
  ShoppingBag,
  Store,
  Visibility,
  LocalShipping,
  CheckCircle,
  Pending,
  Cancel,
  Inventory,
} from '@mui/icons-material'
import { ordersApi } from '../api'
import type { Order, OrderStatus } from '../types'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const getStatusConfig = (status: OrderStatus) => {
  const config: Record<OrderStatus, { icon: React.ReactElement; color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info' }> = {
    pending: { icon: <Pending fontSize="small" />, color: 'warning' },
    payment_pending: { icon: <Pending fontSize="small" />, color: 'warning' },
    paid: { icon: <CheckCircle fontSize="small" />, color: 'info' },
    shipped: { icon: <LocalShipping fontSize="small" />, color: 'primary' },
    delivered: { icon: <Inventory fontSize="small" />, color: 'success' },
    completed: { icon: <CheckCircle fontSize="small" />, color: 'success' },
    cancelled: { icon: <Cancel fontSize="small" />, color: 'error' },
    refunded: { icon: <Cancel fontSize="small" />, color: 'default' },
  }
  return config[status]
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  )
}

function OrderRow({ order, role }: { order: Order; role: 'buyer' | 'seller' }) {
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
            sx={{ width: 56, height: 56 }}
          >
            <ShoppingBag />
          </Avatar>
          <Box>
            <Typography variant="subtitle2" fontWeight={600} noWrap sx={{ maxWidth: 200 }}>
              {order.auctionTitle}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Order #{order.id.slice(0, 8)}
            </Typography>
          </Box>
        </Box>
      </TableCell>
      <TableCell>
        <Typography variant="body2">
          {role === 'buyer' ? order.sellerName : order.buyerName}
        </Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2" fontWeight={600}>
          ${order.totalAmount.toLocaleString()}
        </Typography>
      </TableCell>
      <TableCell>
        <Chip
          icon={getStatusConfig(order.status).icon}
          label={order.status.replace('_', ' ')}
          color={getStatusConfig(order.status).color}
          size="small"
          sx={{ textTransform: 'capitalize' }}
        />
      </TableCell>
      <TableCell>
        <Typography variant="body2" color="text.secondary">
          {formatDate(order.createdAt)}
        </Typography>
      </TableCell>
      <TableCell align="right">
        <IconButton component={Link} to={`/orders/${order.id}`} size="small">
          <Visibility fontSize="small" />
        </IconButton>
      </TableCell>
    </TableRow>
  )
}

function OrdersTable({ role, status }: { role: 'buyer' | 'seller'; status?: OrderStatus }) {
  const [page, setPage] = useState(1)
  const pageSize = 10

  const { data, isLoading, error } = useQuery({
    queryKey: ['orders', role, status, page],
    queryFn: () =>
      role === 'buyer'
        ? ordersApi.getMyPurchases({ status, page, pageSize })
        : ordersApi.getMySales({ status, page, pageSize }),
  })

  if (isLoading) {
    return (
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Item</TableCell>
              <TableCell>{role === 'buyer' ? 'Seller' : 'Buyer'}</TableCell>
              <TableCell>Amount</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Date</TableCell>
              <TableCell align="right">Action</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {[...Array(3)].map((_, i) => (
              <TableRow key={i}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Skeleton variant="rounded" width={56} height={56} />
                    <Box>
                      <Skeleton width={150} />
                      <Skeleton width={80} />
                    </Box>
                  </Box>
                </TableCell>
                <TableCell><Skeleton width={100} /></TableCell>
                <TableCell><Skeleton width={80} /></TableCell>
                <TableCell><Skeleton width={80} /></TableCell>
                <TableCell><Skeleton width={100} /></TableCell>
                <TableCell><Skeleton width={40} /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    )
  }

  if (error) {
    return (
      <Alert severity="error">
        Failed to load orders. Please try again.
      </Alert>
    )
  }

  if (!data || data.items.length === 0) {
    return (
      <Card sx={{ p: 6, textAlign: 'center' }}>
        {role === 'buyer' ? (
          <ShoppingBag sx={{ fontSize: 64, color: 'grey.300', mb: 2 }} />
        ) : (
          <Store sx={{ fontSize: 64, color: 'grey.300', mb: 2 }} />
        )}
        <Typography variant="h6" gutterBottom>
          No {role === 'buyer' ? 'purchases' : 'sales'} yet
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          {role === 'buyer'
            ? 'Win an auction to see your orders here'
            : 'Sell items to see your orders here'}
        </Typography>
        <Button variant="contained" component={Link} to="/auctions">
          Browse Auctions
        </Button>
      </Card>
    )
  }

  return (
    <>
      <TableContainer component={Card}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Item</TableCell>
              <TableCell>{role === 'buyer' ? 'Seller' : 'Buyer'}</TableCell>
              <TableCell>Amount</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Date</TableCell>
              <TableCell align="right">Action</TableCell>
            </TableRow>
          </TableHead>
          <TableBody
            component={motion.tbody}
            variants={staggerContainer}
            initial="initial"
            animate="animate"
          >
            {data.items.map(order => (
              <OrderRow key={order.id} order={order} role={role} />
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {data.totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
          <Pagination
            count={data.totalPages}
            page={page}
            onChange={(_, newPage) => setPage(newPage)}
            color="primary"
          />
        </Box>
      )}
    </>
  )
}

export function OrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const tabParam = searchParams.get('tab')
  const [tabValue, setTabValue] = useState(tabParam === 'sales' ? 1 : 0)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | ''>('')

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
    setSearchParams(newValue === 1 ? { tab: 'sales' } : {})
    setStatusFilter('')
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box sx={{ mb: 4 }}>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
                mb: 1,
              }}
            >
              Orders
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Manage your purchases and sales
            </Typography>
          </Box>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Card sx={{ mb: 3 }}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={tabValue} onChange={handleTabChange}>
                <Tab
                  icon={<ShoppingBag />}
                  iconPosition="start"
                  label="My Purchases"
                />
                <Tab
                  icon={<Store />}
                  iconPosition="start"
                  label="My Sales"
                />
              </Tabs>
            </Box>
          </Card>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Stack direction="row" spacing={2} sx={{ mb: 3 }}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Status</InputLabel>
              <Select
                value={statusFilter}
                label="Status"
                onChange={e => setStatusFilter(e.target.value as OrderStatus | '')}
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
          </Stack>
        </motion.div>

        <motion.div variants={staggerItem}>
          <TabPanel value={tabValue} index={0}>
            <OrdersTable
              role="buyer"
              status={statusFilter || undefined}
            />
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            <OrdersTable
              role="seller"
              status={statusFilter || undefined}
            />
          </TabPanel>
        </motion.div>
      </motion.div>
    </Container>
  )
}
