import { useState, useMemo } from 'react'
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
  Avatar,
  IconButton,
  Stack,
} from '@mui/material'
import { ShoppingBag, Store, Visibility } from '@mui/icons-material'
import { ordersApi } from '../api'
import type { Order, OrderStatus } from '../types'
import { getOrderStatusConfig } from '../utils'
import { formatDate } from '@/shared/utils/formatters'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { usePagination } from '@/shared/hooks'
import { DataTable, FilterPanel } from '@/shared/ui'
import type { ColumnConfig, FilterPanelConfig, OrderFilter } from '@/shared/types'

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

const ORDER_STATUS_OPTIONS = [
  { value: 'pending', label: 'Pending' },
  { value: 'payment_pending', label: 'Payment Pending' },
  { value: 'paid', label: 'Paid' },
  { value: 'processing', label: 'Processing' },
  { value: 'shipped', label: 'Shipped' },
  { value: 'delivered', label: 'Delivered' },
  { value: 'completed', label: 'Completed' },
  { value: 'cancelled', label: 'Cancelled' },
  { value: 'refunded', label: 'Refunded' },
]

const filterConfig: FilterPanelConfig = {
  fields: [
    {
      key: 'status',
      type: 'select',
      label: 'Status',
      options: ORDER_STATUS_OPTIONS,
      gridSize: { xs: 12, sm: 6, md: 4 },
    },
  ],
  collapsible: false,
  showClearButton: true,
}

function OrdersTable({ role }: { role: 'buyer' | 'seller' }) {
  const pagination = usePagination<OrderFilter>({
    defaultPageSize: 10,
    defaultSortBy: 'createdAt',
    defaultSortOrder: 'desc',
  })

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['orders', role, pagination.queryParams],
    queryFn: () =>
      role === 'buyer'
        ? ordersApi.getMyPurchases({
            status: pagination.filter.status as OrderStatus | undefined,
            page: pagination.page,
            pageSize: pagination.pageSize,
          })
        : ordersApi.getMySales({
            status: pagination.filter.status as OrderStatus | undefined,
            page: pagination.page,
            pageSize: pagination.pageSize,
          }),
  })

  const columns: ColumnConfig<Order>[] = useMemo(
    () => [
      {
        key: 'auctionTitle',
        header: 'Item',
        sortable: true,
        sortKey: 'title',
        render: (_value, order) => (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Avatar variant="rounded" src={order.auctionImageUrl} sx={{ width: 56, height: 56 }}>
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
        ),
      },
      {
        key: role === 'buyer' ? 'sellerName' : 'buyerName',
        header: role === 'buyer' ? 'Seller' : 'Buyer',
        sortable: true,
        sortKey: role === 'buyer' ? 'sellerName' : 'buyerName',
        render: (value) => <Typography variant="body2">{String(value)}</Typography>,
      },
      {
        key: 'totalAmount',
        header: 'Amount',
        sortable: true,
        align: 'right',
        render: (value) => (
          <Typography variant="body2" fontWeight={600}>
            ${Number(value).toLocaleString()}
          </Typography>
        ),
      },
      {
        key: 'status',
        header: 'Status',
        sortable: true,
        render: (value) => {
          const config = getOrderStatusConfig(value as OrderStatus)
          return (
            <Chip
              icon={config.icon}
              label={String(value).replace('_', ' ')}
              color={config.color}
              size="small"
              sx={{ textTransform: 'capitalize' }}
            />
          )
        },
      },
      {
        key: 'createdAt',
        header: 'Date',
        sortable: true,
        render: (value) => (
          <Typography variant="body2" color="text.secondary">
            {formatDate(String(value))}
          </Typography>
        ),
      },
      {
        key: 'id',
        header: '',
        align: 'right',
        render: (value) => (
          <IconButton component={Link} to={`/orders/${value}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        ),
      },
    ],
    [role]
  )

  const emptyContent = (
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

  if (!isLoading && (!data || data.items.length === 0) && Object.keys(pagination.filter).length === 0) {
    return emptyContent
  }

  return (
    <Stack spacing={2}>
      <FilterPanel
        config={filterConfig}
        value={pagination.filter}
        onChange={pagination.setFilter}
        onClear={pagination.clearFilter}
        onRefresh={refetch}
      />

      <DataTable
        columns={columns}
        data={data}
        isLoading={isLoading}
        error={error instanceof Error ? error : null}
        sortBy={pagination.sortBy}
        sortOrder={pagination.sortOrder}
        onSort={pagination.handleSort}
        page={pagination.page}
        pageSize={pagination.pageSize}
        onPageChange={pagination.setPage}
        onPageSizeChange={pagination.setPageSize}
        emptyMessage={`No ${role === 'buyer' ? 'purchases' : 'sales'} found`}
        emptyIcon={role === 'buyer' ? <ShoppingBag /> : <Store />}
        rowHover
        animated
      />
    </Stack>
  )
}

export function OrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const tabParam = searchParams.get('tab')
  const [tabValue, setTabValue] = useState(tabParam === 'sales' ? 1 : 0)

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
    setSearchParams(newValue === 1 ? { tab: 'sales' } : {})
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
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
                <Tab icon={<ShoppingBag />} iconPosition="start" label="My Purchases" />
                <Tab icon={<Store />} iconPosition="start" label="My Sales" />
              </Tabs>
            </Box>
          </Card>
        </motion.div>

        <motion.div variants={staggerItem}>
          <TabPanel value={tabValue} index={0}>
            <OrdersTable role="buyer" />
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            <OrdersTable role="seller" />
          </TabPanel>
        </motion.div>
      </motion.div>
    </Container>
  )
}
