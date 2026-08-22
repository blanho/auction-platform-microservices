import { usePagination } from '@/shared/hooks'
import type { ColumnConfig, FilterPanelConfig, NotificationRecordFilter } from '@/shared/types'
import { DataTable, FilterPanel } from '@/shared/ui'
import { formatNumber } from '@/shared/utils/formatters'
import {
  CheckCircle,
  Email,
  Error,
  Notifications,
  PendingActions,
  PhoneIphone,
  Send,
  Sms,
  TrendingUp,
} from '@mui/icons-material'
import { Box, Card, Chip, Container, Grid, Typography } from '@mui/material'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useRecords, useRecordStats } from '../hooks'
import type { NotificationChannel, NotificationRecord, RecordStatus } from '../types/template.types'
import { formatTimeAgo } from '../utils'

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

export function RecordsDashboardPage() {
  const { t } = useTranslation('notifications')
  const pagination = usePagination<NotificationRecordFilter>({ defaultPageSize: 20 })
  const {
    data: recordsData,
    isLoading,
    refetch,
  } = useRecords({
    page: pagination.page,
    pageSize: pagination.pageSize,
    sortBy: pagination.sortBy,
    sortOrder: pagination.sortOrder,
    ...pagination.filter,
  })
  const { data: stats } = useRecordStats()

  const filterConfig: FilterPanelConfig = useMemo(
    () => ({
      fields: [
        {
          key: 'channel',
          label: t('records.channel'),
          type: 'select',
          options: (['email', 'sms', 'push', 'in_app'] as NotificationChannel[]).map((value) => ({
            value,
            label: t(`templates.channels.${value}`),
          })),
          clearable: true,
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
        {
          key: 'status',
          label: t('records.status'),
          type: 'select',
          options: (['pending', 'sent', 'delivered', 'failed', 'bounced'] as RecordStatus[]).map(
            (value) => ({ value, label: t(`records.statuses.${value}`) })
          ),
          clearable: true,
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
        {
          key: 'templateKey',
          label: t('records.templateKey'),
          type: 'text',
          placeholder: t('records.templatePlaceholder'),
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
        {
          key: 'dateFrom',
          label: t('records.fromDate'),
          type: 'date',
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
      ],
      collapsible: true,
      defaultExpanded: true,
      showClearButton: true,
    }),
    [t]
  )

  const columns: ColumnConfig<NotificationRecord>[] = useMemo(
    () => [
      {
        key: 'recipient',
        header: t('records.recipient'),
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
        header: t('records.channel'),
        sortable: true,
        sortKey: 'channel',
        render: (_, row) => (
          <Chip
            icon={CHANNEL_ICONS[row.channel]}
            label={t(`templates.channels.${row.channel}`)}
            size="small"
            sx={{ fontWeight: 600 }}
          />
        ),
      },
      {
        key: 'templateKey',
        header: t('records.template'),
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
        header: t('records.subject'),
        render: (_, row) => (
          <Typography sx={{ fontSize: '0.875rem', color: '#44403C' }}>
            {row.subject || '-'}
          </Typography>
        ),
      },
      {
        key: 'status',
        header: t('records.status'),
        sortable: true,
        sortKey: 'status',
        render: (_, row) => {
          const config = STATUS_CONFIG[row.status]
          return (
            <Chip
              icon={config.icon}
              label={t(`records.statuses.${row.status}`)}
              size="small"
              sx={{ bgcolor: config.bgcolor, color: config.color, fontWeight: 600 }}
            />
          )
        },
      },
      {
        key: 'sentAt',
        header: t('records.sentAt'),
        sortable: true,
        sortKey: 'sentAt',
        render: (_, row) => (
          <Typography variant="caption" sx={{ color: '#78716C' }}>
            {row.sentAt ? formatTimeAgo(row.sentAt) : '-'}
          </Typography>
        ),
      },
    ],
    [t]
  )

  const statCards = [
    {
      title: t('records.totalSent'),
      value: stats?.sentCount || 0,
      icon: <Send sx={{ fontSize: 32, color: '#7C3AED' }} />,
      color: '#7C3AED',
      bgcolor: '#F3E8FF',
    },
    {
      title: t('records.delivered'),
      value: stats?.deliveredCount || 0,
      icon: <CheckCircle sx={{ fontSize: 32, color: '#10B981' }} />,
      color: '#10B981',
      bgcolor: '#D1FAE5',
    },
    {
      title: t('records.failed'),
      value: stats?.failedCount || 0,
      icon: <Error sx={{ fontSize: 32, color: '#EF4444' }} />,
      color: '#EF4444',
      bgcolor: '#FEE2E2',
    },
    {
      title: t('records.successRate'),
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
          {t('records.title')}
        </Typography>
        <Typography sx={{ color: '#78716C', fontFamily: '"Fira Sans", sans-serif' }}>
          {t('records.description')}
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
                    {typeof stat.value === 'number' ? formatNumber(stat.value) : stat.value}
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
          config={filterConfig}
          value={pagination.filter}
          onChange={pagination.setFilter}
          onClear={pagination.clearFilter}
          onRefresh={refetch}
        />

        <DataTable
          columns={columns}
          data={recordsData}
          isLoading={isLoading}
          sortBy={pagination.sortBy}
          sortOrder={pagination.sortOrder}
          onSort={pagination.handleSort}
          page={pagination.page}
          pageSize={pagination.pageSize}
          onPageChange={pagination.setPage}
          onPageSizeChange={pagination.setPageSize}
          emptyMessage={t('records.empty')}
        />
      </Card>
    </Container>
  )
}
