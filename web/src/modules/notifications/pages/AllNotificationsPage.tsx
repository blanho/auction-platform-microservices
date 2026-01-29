import { useState } from 'react'
import {
  Box,
  Container,
  Typography,
  Card,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Pagination,
  Grid,
} from '@mui/material'
import { Person, CheckCircle, MailOutline, Archive } from '@mui/icons-material'
import { useAllNotifications } from '../hooks'
import type {
  Notification,
  NotificationType,
  NotificationStatus,
  AdminNotificationFilters,
} from '../types/notification.types'
import { formatTimeAgo, getNotificationColor, getNotificationLabel } from '../utils'
import { palette } from '@/shared/theme/tokens'
import { TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'

const STATUS_CONFIG: Record<
  NotificationStatus,
  { color: string; bgcolor: string; icon: React.ReactElement }
> = {
  unread: { color: palette.semantic.warning, bgcolor: '#FEF3C7', icon: <MailOutline /> },
  read: { color: palette.semantic.success, bgcolor: '#D1FAE5', icon: <CheckCircle /> },
  archived: { color: palette.neutral[500], bgcolor: palette.neutral[100], icon: <Archive /> },
}

export function AllNotificationsPage() {
  const [filters, setFilters] = useState<
    AdminNotificationFilters & { page: number; pageSize: number }
  >({
    page: 1,
    pageSize: 20,
  })

  const { data, isLoading } = useAllNotifications(filters.page, filters.pageSize, filters)

  const handleFilterChange = (key: string, value: string | undefined) => {
    setFilters({ ...filters, [key]: value || undefined, page: 1 })
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Inter", sans-serif',
            fontWeight: 600,
            color: '#4C1D95',
            mb: 1,
          }}
        >
          All Notifications
        </Typography>
        <Typography sx={{ color: palette.neutral[500], fontFamily: '"Inter", sans-serif' }}>
          View and manage all user notifications across the platform
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)', mb: 3 }}>
        <Box sx={{ p: 3, bgcolor: '#FAF5FF', borderBottom: '1px solid #F5F5F5' }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <TextField
                fullWidth
                size="small"
                label="User ID"
                value={filters.userId || ''}
                onChange={(e) => handleFilterChange('userId', e.target.value)}
                placeholder="Filter by user ID"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Type</InputLabel>
                <Select
                  value={filters.type || ''}
                  label="Type"
                  onChange={(e) => handleFilterChange('type', e.target.value)}
                >
                  <MenuItem value="">All Types</MenuItem>
                  <MenuItem value="bid_placed">Bid Placed</MenuItem>
                  <MenuItem value="bid_outbid">Bid Outbid</MenuItem>
                  <MenuItem value="auction_won">Auction Won</MenuItem>
                  <MenuItem value="auction_lost">Auction Lost</MenuItem>
                  <MenuItem value="auction_ending">Auction Ending</MenuItem>
                  <MenuItem value="auction_ended">Auction Ended</MenuItem>
                  <MenuItem value="payment_received">Payment Received</MenuItem>
                  <MenuItem value="payment_failed">Payment Failed</MenuItem>
                  <MenuItem value="system">System</MenuItem>
                  <MenuItem value="promotional">Promotional</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  value={filters.status || ''}
                  label="Status"
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                >
                  <MenuItem value="">All Statuses</MenuItem>
                  <MenuItem value="unread">Unread</MenuItem>
                  <MenuItem value="read">Read</MenuItem>
                  <MenuItem value="archived">Archived</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </Box>

        <TableContainer sx={{ maxHeight: 600 }}>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  User
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Type
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Title
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Message
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Status
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Created
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && <TableSkeletonRows rows={10} columns={6} />}
              {!isLoading && data?.items.length === 0 && (
                <TableEmptyStateRow
                  colSpan={6}
                  title="No notifications found"
                  cellSx={{ py: 8 }}
                />
              )}
              {!isLoading && data?.items.length > 0 && (
                data?.items.map((notification: Notification) => (
                  <TableRow
                    key={notification.id}
                    sx={{
                      '&:hover': { bgcolor: '#FAFAF9' },
                      transition: 'background-color 200ms',
                    }}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Person sx={{ fontSize: 18, color: palette.neutral[500] }} />
                        <Typography
                          variant="caption"
                          sx={{
                            fontFamily: '"Inter", monospace',
                            color: palette.neutral[500],
                          }}
                        >
                          {notification.userId.substring(0, 8)}...
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={getNotificationLabel(notification.type as NotificationType)}
                        size="small"
                        sx={{
                          bgcolor: `${getNotificationColor(notification.type as NotificationType)}20`,
                          color: getNotificationColor(notification.type as NotificationType),
                          fontWeight: 600,
                          fontSize: '0.75rem',
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                        {notification.title}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography
                        sx={{
                          fontSize: '0.875rem',
                          color: palette.neutral[700],
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                          maxWidth: 300,
                        }}
                      >
                        {notification.message}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={STATUS_CONFIG[notification.status as NotificationStatus].icon}
                        label={notification.status.toUpperCase()}
                        size="small"
                        sx={{
                          bgcolor: STATUS_CONFIG[notification.status as NotificationStatus].bgcolor,
                          color: STATUS_CONFIG[notification.status as NotificationStatus].color,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                        {formatTimeAgo(notification.createdAt)}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {data && data.totalPages > 1 && (
          <Box
            sx={{ display: 'flex', justifyContent: 'center', p: 3, borderTop: '1px solid #F5F5F5' }}
          >
            <Pagination
              count={data.totalPages}
              page={filters.page}
              onChange={(_, p) => handleFilterChange('page', p)}
              color="primary"
            />
          </Box>
        )}
      </Card>

      {data && (
        <Card sx={{ p: 3, borderRadius: 2, bgcolor: '#FAF5FF' }}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                Total Notifications
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {data.totalCount}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                Current Page
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {data.page} / {data.totalPages}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                Per Page
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {data.pageSize}
              </Typography>
            </Grid>
          </Grid>
        </Card>
      )}
    </Container>
  )
}
