import { palette } from '@/shared/theme/tokens'
import { TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'
import { formatNumber } from '@/shared/utils/formatters'
import { Archive, CheckCircle, MailOutline, Person } from '@mui/icons-material'
import {
  Box,
  Card,
  Chip,
  Container,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Pagination,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAllNotifications } from '../hooks'
import type {
  AdminNotificationFilters,
  Notification,
  NotificationStatus,
  NotificationType,
} from '../types/notification.types'
import { formatTimeAgo, getNotificationColor, getNotificationLabel } from '../utils'

const STATUS_CONFIG: Record<
  NotificationStatus,
  { color: string; bgcolor: string; icon: React.ReactElement }
> = {
  unread: { color: palette.semantic.warning, bgcolor: '#FEF3C7', icon: <MailOutline /> },
  read: { color: palette.semantic.success, bgcolor: '#D1FAE5', icon: <CheckCircle /> },
  archived: { color: palette.neutral[500], bgcolor: palette.neutral[100], icon: <Archive /> },
}

export function AllNotificationsPage() {
  const { t } = useTranslation('notifications')
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
          {t('admin.title')}
        </Typography>
        <Typography sx={{ color: palette.neutral[500], fontFamily: '"Inter", sans-serif' }}>
          {t('admin.description')}
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)', mb: 3 }}>
        <Box sx={{ p: 3, bgcolor: '#FAF5FF', borderBottom: '1px solid #F5F5F5' }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <TextField
                fullWidth
                size="small"
                label={t('admin.userId')}
                value={filters.userId || ''}
                onChange={(e) => handleFilterChange('userId', e.target.value)}
                placeholder={t('admin.userIdPlaceholder')}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <FormControl fullWidth size="small">
                <InputLabel>{t('admin.type')}</InputLabel>
                <Select
                  value={filters.type || ''}
                  label={t('admin.type')}
                  onChange={(e) => handleFilterChange('type', e.target.value)}
                >
                  <MenuItem value="">{t('admin.allTypes')}</MenuItem>
                  {[
                    'bid_placed',
                    'bid_outbid',
                    'auction_won',
                    'auction_lost',
                    'auction_ending',
                    'auction_ended',
                    'payment_received',
                    'payment_failed',
                    'system',
                    'promotional',
                  ].map((type) => (
                    <MenuItem key={type} value={type}>
                      {t(`admin.types.${type}`)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <FormControl fullWidth size="small">
                <InputLabel>{t('admin.status')}</InputLabel>
                <Select
                  value={filters.status || ''}
                  label={t('admin.status')}
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                >
                  <MenuItem value="">{t('admin.allStatuses')}</MenuItem>
                  {['unread', 'read', 'archived'].map((status) => (
                    <MenuItem key={status} value={status}>
                      {t(`admin.statuses.${status}`)}
                    </MenuItem>
                  ))}
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
                  {t('admin.user')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  {t('admin.type')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  {t('admin.notificationTitle')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  {t('admin.message')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  {t('admin.status')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  {t('admin.created')}
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && <TableSkeletonRows rows={10} columns={6} />}
              {!isLoading && (data?.items?.length ?? 0) === 0 && (
                <TableEmptyStateRow
                  colSpan={6}
                  title={t('admin.noNotifications')}
                  cellSx={{ py: 8 }}
                />
              )}
              {!isLoading &&
                (data?.items?.length ?? 0) > 0 &&
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
                        label={t(`admin.types.${notification.type}`, {
                          defaultValue: getNotificationLabel(notification.type as NotificationType),
                        })}
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
                        label={t(`admin.statuses.${notification.status}`)}
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
                ))}
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
              onChange={(_, p) => setFilters((prev) => ({ ...prev, page: p }))}
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
                {t('admin.total')}
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {formatNumber(data.totalCount)}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                {t('admin.currentPage')}
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {data.page} / {data.totalPages}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
                {t('admin.perPage')}
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#7C3AED' }}>
                {formatNumber(data.pageSize)}
              </Typography>
            </Grid>
          </Grid>
        </Card>
      )}
    </Container>
  )
}
