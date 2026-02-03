import { useMemo } from 'react'
import { Box, Container, Typography, Card, Grid, Chip } from '@mui/material'
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
import type { NotificationChannel, RecordStatus, NotificationRecord } from '../types/template.types'
import { formatTimeAgo } from '../utils'
import { DataTable, FilterPanel } from '@/shared/ui'
import { usePagination } from '@/shared/hooks'
import type { ColumnConfig, FilterPanelConfig, NotificationRecordFilter } from '@/shared/types'

const CHANNEL_ICONS: Record<NotificationChannel, React.ReactElement> = {
  email: <Email fontSize="small" />,
  sms: <Sms fontSize="small" />,
  push: <PhoneIphone fontSize="small" />,
  in_app: <Notifications fontSize="small" />,
}

const STATUS_CONFIG: Record<
  RecordStatus,
  { color: string; bgcolor: string; icon: React.ReactElement }
> = {
  pending: { color: '#F59E0B', bgcolor: '#FEF3C7', icon: <PendingActions fontSize="small" /> },
  sent: { color: '#3B82F6', bgcolor: '#DBEAFE', icon: <Send fontSize="small" /> },
  delivered: { color: '#10B981', bgcolor: '#D1FAE5', icon: <CheckCircle fontSize="small" /> },
  failed: { color: '#EF4444', bgcolor: '#FEE2E2', icon: <Error fontSize="small" /> },
  bounced: { color: '#78716C', bgcolor: '#F5F5F5', icon: <Error fontSize="small" /> },
}

const FILTER_CONFIG: FilterPanelConfig = {
  fields: [
    {
      key: 'channel',
      label: 'Channel',
      type: 'select',
      options: [
        { value: 'email', label: 'Email' },
        { value: 'sms', label: 'SMS' },
        { value: 'push', label: 'Push' },
        { value: 'in_app', label: 'In-App' },
      ],
      clearable: true,
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'status',
      label: 'Status',
      type: 'select',
      options: [
        { value: 'pending', label: 'Pending' },
        { value: 'sent', label: 'Sent' },
        { value: 'delivered', label: 'Delivered' },
        { value: 'failed', label: 'Failed' },
        { value: 'bounced', label: 'Bounced' },
      ],
      clearable: true,
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'templateKey',
      label: 'Template Key',
      type: 'text',
      placeholder: 'Search by template...',
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'dateFrom',
      label: 'From Date',
      type: 'date',
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
  ],
  collapsible: true,
  defaultExpanded: true,
  showClearButton: true,
}

export function RecordsDashboardPage() {
  const pagination = usePagination<NotificationRecordFilter>({ pageSize: 20 })
  const { data: recordsData, isLoading, refetch } = useRecords({
    page: pagination.page,
    pageSize: pagination.pageSize,
    sortBy: pagination.sortBy,
    sortOrder: pagination.sortOrder,
    ...pagination.filter,
  })
  const { data: stats } = useRecordStats()

  const items = useMemo(() => recordsData?.items ?? [], [recordsData?.items])

  const columns: ColumnConfig<NotificationRecord>[] = useMemo(
    () => [
      {
        key: 'recipient',
        header: 'Recipient',
        sortable: true,
        sortKey: 'recipient',
        render: (_, row) => (
          <Box>
            <Typography sx={{ fontWeight: 500, color: '#1C1917' }}>{row.recipient}</Typography>
            {row.errorMessage && (
              <Typography variant="caption" sx={{ color: '#EF4444' }}>
                {row.errorMessage}
              </Typography>
            )}
          </Box>
        ),
      },
      {
        key: 'channel',
        header: 'Channel',
        sortable: true,
        sortKey: 'channel',
        render: (_, row) => (
          <Chip
            icon={CHANNEL_ICONS[row.channel]}
            label={row.channel.toUpperCase()}
            size="small"
            sx={{ fontWeight: 600 }}
          />
        ),
      },
      {
        key: 'templateKey',
        header: 'Template',
        sortable: true,
        sortKey: 'templateKey',
        render: (_, row) => (
          <Typography
            variant="caption"
            sx={{ fontFamily: '"Fira Code", monospace', color: '#78716C' }}
          >
            {row.templateKey || '-'}
          </Typography>
        ),
      },
      {
        key: 'subject',
        header: 'Subject',
        render: (_, row) => (
          <Typography sx={{ fontSize: '0.875rem', color: '#44403C' }}>
            {row.subject || '-'}
          </Typography>
        ),
      },
      {
        key: 'status',
        header: 'Status',
        sortable: true,
        sortKey: 'status',
        render: (_, row) => {
          const config = STATUS_CONFIG[row.status]
          return (
            <Chip
              icon={config.icon}
              label={row.status.toUpperCase()}
              size="small"
              sx={{ bgcolor: config.bgcolor, color: config.color, fontWeight: 600 }}
            />
          )
        },
      },
      {
        key: 'sentAt',
        header: 'Sent At',
        sortable: true,
        sortKey: 'sentAt',
        render: (_, row) => (
          <Typography variant="caption" sx={{ color: '#78716C' }}>
            {row.sentAt ? formatTimeAgo(row.sentAt) : '-'}
          </Typography>
        ),
      },
    ],
    []
  )

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
        <FilterPanel
          config={FILTER_CONFIG}
          value={pagination.filter}
          onChange={pagination.setFilter}
          onClear={pagination.clearFilter}
          onRefresh={refetch}
        />

        <DataTable
          columns={columns}
          data={items}
          isLoading={isLoading}
          sortBy={pagination.sortBy}
          sortOrder={pagination.sortOrder}
          onSort={pagination.handleSort}
          page={pagination.page}
          pageSize={pagination.pageSize}
          totalCount={recordsData?.totalCount ?? 0}
          totalPages={recordsData?.totalPages ?? 0}
          onPageChange={pagination.setPage}
          onPageSizeChange={pagination.setPageSize}
          emptyMessage="No notification records found"
        />
      </Card>
    </Container>
  )
}
