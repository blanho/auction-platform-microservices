import { useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { Box, Typography, Chip, Container, Stack } from '@mui/material'
import { AccessTime } from '@mui/icons-material'
import { useBidHistory } from '../hooks/useBids'
import { BidStatus, type Bid } from '../types'
import { formatCurrency, formatDateTime } from '@/shared/utils'
import { StatusBadge, DataTable, FilterPanel } from '@/shared/ui'
import { usePagination } from '@/shared/hooks'
import type { ColumnConfig, FilterPanelConfig, BidFilter } from '@/shared/types'

const BID_STATUS_OPTIONS = Object.values(BidStatus).map((status) => ({
  value: status,
  label: status,
}))

const filterConfig: FilterPanelConfig = {
  fields: [
    {
      key: 'auctionId',
      type: 'text',
      label: 'Auction ID',
      placeholder: 'Enter auction ID...',
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'status',
      type: 'select',
      label: 'Status',
      options: BID_STATUS_OPTIONS,
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'dateRange',
      type: 'dateRange',
      label: 'Date',
      startKey: 'dateFrom',
      endKey: 'dateTo',
      gridSize: { xs: 12, sm: 12, md: 6 },
    },
  ],
  collapsible: true,
  defaultExpanded: true,
  showClearButton: true,
}

export function BidHistoryPage() {
  const navigate = useNavigate()

  const pagination = usePagination<BidFilter>({
    defaultPageSize: 20,
    defaultSortBy: 'bidTime',
    defaultSortOrder: 'desc',
  })

  const { data, isLoading, error, refetch } = useBidHistory({
    page: pagination.page,
    pageSize: pagination.pageSize,
    auctionId: pagination.filter.auctionId,
    status: pagination.filter.status,
    fromDate: pagination.filter.dateFrom,
    toDate: pagination.filter.dateTo,
  })

  const columns: ColumnConfig<Bid>[] = useMemo(
    () => [
      {
        key: 'auctionTitle',
        header: 'Auction',
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
        header: 'Bid Amount',
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
        header: 'Status',
        sortable: true,
        render: (value) => (
          <StatusBadge
            status={String(value)}
            sx={{ fontFamily: 'Chakra Petch', fontWeight: 600, fontSize: '0.7rem' }}
          />
        ),
      },
      {
        key: 'isWinning',
        header: 'Winning',
        render: (value) =>
          value ? (
            <Chip
              label="WINNING"
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
        header: 'Bid Time',
        sortable: true,
        render: (value) => (
          <Typography variant="body2" sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}>
            {formatDateTime(String(value))}
          </Typography>
        ),
      },
    ],
    []
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
              Bid History
            </Typography>
          </Stack>
          <Typography variant="body1" color="text.secondary">
            Complete history of all your bids across all auctions
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
            emptyMessage="No bid history found"
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
