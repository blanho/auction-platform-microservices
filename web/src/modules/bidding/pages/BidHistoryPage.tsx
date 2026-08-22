import { usePagination } from '@/shared/hooks'
import type { BidFilter, ColumnConfig, FilterPanelConfig } from '@/shared/types'
import { DataTable, FilterPanel, StatusBadge } from '@/shared/ui'
import { formatCurrency, formatDateTime } from '@/shared/utils'
import { AccessTime } from '@mui/icons-material'
import { Box, Chip, Container, Stack, Typography } from '@mui/material'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useBidHistory } from '../hooks/useBids'
import { BidStatus, type BidHistory } from '../types'

export function BidHistoryPage() {
  const { t } = useTranslation('bidding')
  const navigate = useNavigate()

  const filterConfig: FilterPanelConfig = useMemo(
    () => ({
      fields: [
        {
          key: 'auctionId',
          type: 'text',
          label: t('history.auctionId'),
          placeholder: t('history.auctionIdPlaceholder'),
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
        {
          key: 'status',
          type: 'select',
          label: t('history.status'),
          options: Object.values(BidStatus).map((status) => ({
            value: status,
            label: t(`history.statuses.${status}`, { defaultValue: status }),
          })),
          gridSize: { xs: 12, sm: 6, md: 3 },
        },
        {
          key: 'dateRange',
          type: 'dateRange',
          label: t('history.date'),
          startKey: 'dateFrom',
          endKey: 'dateTo',
          gridSize: { xs: 12, sm: 12, md: 6 },
        },
      ],
      collapsible: true,
      defaultExpanded: true,
      showClearButton: true,
    }),
    [t]
  )

  const pagination = usePagination<BidFilter>({
    defaultPageSize: 20,
    defaultSortBy: 'bidTime',
    defaultSortOrder: 'desc',
  })

  const { data, isLoading, error, refetch } = useBidHistory({
    page: pagination.page,
    pageSize: pagination.pageSize,
    auctionId: pagination.filter.auctionId,
    status: pagination.filter.status as BidStatus | undefined,
    fromDate: pagination.filter.dateFrom,
    toDate: pagination.filter.dateTo,
  })

  const columns: ColumnConfig<BidHistory>[] = useMemo(
    () => [
      {
        key: 'auctionTitle',
        header: t('history.auction'),
        sortable: true,
        sortKey: 'auctionTitle',
        render: (value) => (
          <Typography
            variant="body2"
            sx={{ fontFamily: 'Chakra Petch', fontWeight: 600, color: '#1E293B' }}
          >
            {String(value)}
          </Typography>
        ),
      },
      {
        key: 'amount',
        header: t('history.bidAmount'),
        sortable: true,
        align: 'right',
        render: (value) => (
          <Typography
            variant="body2"
            sx={{ fontFamily: 'Russo One', color: '#2563EB', fontWeight: 700 }}
          >
            {formatCurrency(Number(value))}
          </Typography>
        ),
      },
      {
        key: 'status',
        header: t('history.status'),
        sortable: true,
        render: (value) => (
          <StatusBadge
            status={String(value)}
            label={t(`history.statuses.${String(value)}`, { defaultValue: String(value) })}
            sx={{ fontFamily: 'Chakra Petch', fontWeight: 600, fontSize: '0.7rem' }}
          />
        ),
      },
      {
        key: 'isWinning',
        header: t('history.winning'),
        render: (value) =>
          value ? (
            <Chip
              label={t('winning.badge')}
              size="small"
              sx={{
                background: 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
                color: '#FFF',
                fontFamily: 'Chakra Petch',
                fontWeight: 600,
                fontSize: '0.7rem',
              }}
            />
          ) : (
            <Typography variant="body2" sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}>
              -
            </Typography>
          ),
      },
      {
        key: 'bidTime',
        header: t('history.bidTime'),
        sortable: true,
        render: (value) => (
          <Typography variant="body2" sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}>
            {formatDateTime(String(value))}
          </Typography>
        ),
      },
    ],
    [t]
  )

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #F8FAFC 0%, #E0E7FF 100%)',
        py: 6,
      }}
    >
      <Container maxWidth="xl">
        <Box
          sx={{
            mb: 4,
            p: 4,
            background: 'rgba(255, 255, 255, 0.7)',
            backdropFilter: 'blur(12px)',
            borderRadius: 3,
            border: '1px solid rgba(255, 255, 255, 0.3)',
          }}
        >
          <Stack direction="row" alignItems="center" spacing={2} mb={1}>
            <AccessTime sx={{ width: 40, height: 40, color: '#2563EB' }} />
            <Typography
              variant="h3"
              sx={{ fontFamily: 'Russo One', fontWeight: 700, color: '#1E293B' }}
            >
              {t('bidHistory')}
            </Typography>
          </Stack>
          <Typography variant="body1" color="text.secondary">
            {t('bidHistoryDescription')}
          </Typography>
        </Box>

        <Stack spacing={3}>
          <FilterPanel
            config={filterConfig}
            value={pagination.filter}
            onChange={pagination.setFilter}
            onClear={pagination.clearFilter}
            onRefresh={refetch}
            sx={{
              background: 'rgba(255, 255, 255, 0.85)',
              backdropFilter: 'blur(16px)',
              border: '1px solid rgba(255, 255, 255, 0.3)',
            }}
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
            onRowClick={(row) => navigate(`/auctions/${row.auctionId}`)}
            rowHover
            emptyMessage={t('empty.noBidHistory')}
            sx={{
              '& .MuiPaper-root': {
                background: 'rgba(255, 255, 255, 0.85)',
                backdropFilter: 'blur(16px)',
                border: '1px solid rgba(255, 255, 255, 0.3)',
              },
            }}
            tableContainerSx={{
              background: 'transparent',
              border: 'none',
            }}
          />
        </Stack>
      </Container>
    </Box>
  )
}
