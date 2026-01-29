import { useState } from 'react'
import {
  Box,
  Container,
  Typography,
  Card,
  Grid,
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
} from '@mui/material'
import {
  TrendingUp,
  Email,
  Sms,
  PhoneIphone,
  Notifications,
  CheckCircle,
  Error,
  PendingActions,
  Send,
} from '@mui/icons-material'
import { useRecords, useRecordStats } from '../hooks'
import type {
  NotificationChannel,
  RecordStatus,
  NotificationRecordFilterDto,
} from '../types/template.types'
import { formatTimeAgo } from '../utils'
import { TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'

const CHANNEL_ICONS: Record<NotificationChannel, React.ReactElement> = {
  email: <Email />,
  sms: <Sms />,
  push: <PhoneIphone />,
  in_app: <Notifications />,
}

const STATUS_CONFIG: Record<
  RecordStatus,
  { color: string; bgcolor: string; icon: React.ReactElement }
> = {
  pending: { color: '#F59E0B', bgcolor: '#FEF3C7', icon: <PendingActions /> },
  sent: { color: '#3B82F6', bgcolor: '#DBEAFE', icon: <Send /> },
  delivered: { color: '#10B981', bgcolor: '#D1FAE5', icon: <CheckCircle /> },
  failed: { color: '#EF4444', bgcolor: '#FEE2E2', icon: <Error /> },
  bounced: { color: '#78716C', bgcolor: '#F5F5F5', icon: <Error /> },
}

export function RecordsDashboardPage() {
  const [filters, setFilters] = useState<NotificationRecordFilterDto>({
    page: 1,
    pageSize: 20,
  })

  const { data: recordsData, isLoading } = useRecords(filters)
  const { data: stats } = useRecordStats()

  const handleFilterChange = (
    key: keyof NotificationRecordFilterDto,
    value: string | undefined
  ) => {
    setFilters({ ...filters, [key]: value, page: 1 })
  }

  const statCards = [
    {
      title: 'Total Sent',
      value: stats?.sentCount || 0,
      icon: <Send sx={{ fontSize: 32, color: '#7C3AED' }} />,
      color: '#7C3AED',
      bgcolor: '#F3E8FF',
    },
    {
      title: 'Delivered',
      value: stats?.deliveredCount || 0,
      icon: <CheckCircle sx={{ fontSize: 32, color: '#10B981' }} />,
      color: '#10B981',
      bgcolor: '#D1FAE5',
    },
    {
      title: 'Failed',
      value: stats?.failedCount || 0,
      icon: <Error sx={{ fontSize: 32, color: '#EF4444' }} />,
      color: '#EF4444',
      bgcolor: '#FEE2E2',
    },
    {
      title: 'Success Rate',
      value: stats?.sentCount
        ? `${((stats.deliveredCount / stats.sentCount) * 100).toFixed(1)}%`
        : '0%',
      icon: <TrendingUp sx={{ fontSize: 32, color: '#F97316' }} />,
      color: '#F97316',
      bgcolor: '#FFEDD5',
    },
  ]

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Fira Code", monospace',
            fontWeight: 600,
            color: '#4C1D95',
            mb: 1,
          }}
        >
          Notification Records
        </Typography>
        <Typography sx={{ color: '#78716C', fontFamily: '"Fira Sans", sans-serif' }}>
          Track delivery status and performance metrics
        </Typography>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statCards.map((stat) => (
          <Grid key={stat.title} size={{ xs: 12, sm: 6, md: 3 }}>
            <Card
              sx={{
                p: 3,
                borderRadius: 2,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                border: `1px solid ${stat.bgcolor}`,
              }}
            >
              <Box
                sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}
              >
                <Box>
                  <Typography sx={{ color: '#78716C', fontSize: '0.875rem', mb: 1 }}>
                    {stat.title}
                  </Typography>
                  <Typography
                    variant="h4"
                    sx={{
                      fontFamily: '"Fira Code", monospace',
                      fontWeight: 700,
                      color: stat.color,
                    }}
                  >
                    {stat.value}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    bgcolor: stat.bgcolor,
                    p: 1.5,
                    borderRadius: '50%',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  {stat.icon}
                </Box>
              </Box>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)', mb: 3 }}>
        <Box sx={{ p: 3, bgcolor: '#FAF5FF', borderBottom: '1px solid #F5F5F5' }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Channel</InputLabel>
                <Select
                  value={filters.channel || ''}
                  label="Channel"
                  onChange={(e) => handleFilterChange('channel', e.target.value || undefined)}
                >
                  <MenuItem value="">All Channels</MenuItem>
                  <MenuItem value="email">Email</MenuItem>
                  <MenuItem value="sms">SMS</MenuItem>
                  <MenuItem value="push">Push</MenuItem>
                  <MenuItem value="in_app">In-App</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Status</InputLabel>
                <Select
                  value={filters.status || ''}
                  label="Status"
                  onChange={(e) => handleFilterChange('status', e.target.value || undefined)}
                >
                  <MenuItem value="">All Statuses</MenuItem>
                  <MenuItem value="pending">Pending</MenuItem>
                  <MenuItem value="sent">Sent</MenuItem>
                  <MenuItem value="delivered">Delivered</MenuItem>
                  <MenuItem value="failed">Failed</MenuItem>
                  <MenuItem value="bounced">Bounced</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                size="small"
                label="Template Key"
                value={filters.templateKey || ''}
                onChange={(e) => handleFilterChange('templateKey', e.target.value || undefined)}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <TextField
                fullWidth
                size="small"
                type="date"
                label="From Date"
                value={filters.fromDate || ''}
                onChange={(e) => handleFilterChange('fromDate', e.target.value || undefined)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
          </Grid>
        </Box>

        <TableContainer sx={{ maxHeight: 600, overflow: 'auto' }}>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Recipient
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Channel
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Template
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Subject
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Status
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95', bgcolor: '#FAF5FF' }}>
                  Sent At
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && <TableSkeletonRows rows={10} columns={6} />}
              {!isLoading && recordsData?.items.length === 0 && (
                <TableEmptyStateRow colSpan={6} title="No records found" cellSx={{ py: 8 }} />
              )}
              {!isLoading && recordsData?.items.length > 0 && (
                recordsData?.items.map((record) => (
                  <TableRow
                    key={record.id}
                    sx={{
                      '&:hover': { bgcolor: '#FAFAF9' },
                    }}
                  >
                    <TableCell>
                      <Typography sx={{ fontWeight: 500, color: '#1C1917' }}>
                        {record.recipient}
                      </Typography>
                      {record.errorMessage && (
                        <Typography variant="caption" sx={{ color: '#EF4444' }}>
                          {record.errorMessage}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={CHANNEL_ICONS[record.channel]}
                        label={record.channel.toUpperCase()}
                        size="small"
                        sx={{ fontWeight: 600 }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography
                        variant="caption"
                        sx={{
                          fontFamily: '"Fira Code", monospace',
                          color: '#78716C',
                        }}
                      >
                        {record.templateKey || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ fontSize: '0.875rem', color: '#44403C' }}>
                        {record.subject || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={STATUS_CONFIG[record.status].icon}
                        label={record.status.toUpperCase()}
                        size="small"
                        sx={{
                          bgcolor: STATUS_CONFIG[record.status].bgcolor,
                          color: STATUS_CONFIG[record.status].color,
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ color: '#78716C' }}>
                        {record.sentAt ? formatTimeAgo(record.sentAt) : '-'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {recordsData && recordsData.totalPages > 1 && (
          <Box
            sx={{ display: 'flex', justifyContent: 'center', p: 3, borderTop: '1px solid #F5F5F5' }}
          >
            <Pagination
              count={recordsData.totalPages}
              page={filters.page || 1}
              onChange={(_, p) => handleFilterChange('page', p)}
              color="primary"
            />
          </Box>
        )}
      </Card>
    </Container>
  )
}
