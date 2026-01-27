import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Chip,
  TextField,
  MenuItem,
  Stack,
  Container,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Pagination,
  Skeleton,
  Alert,
  IconButton,
} from '@mui/material'
import { AccessTime, FilterList, Close } from '@mui/icons-material'
import { useBidHistory } from '../hooks/useBids'
import { BidStatus, type BidHistoryFilters } from '../types'
import { getBidStatusColor } from '../utils'
import { formatCurrency, formatDateTime } from '@/shared/utils'

export const BidHistoryPage = () => {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<BidHistoryFilters>({
    page: 1,
    pageSize: 20,
  })

  const { data, isLoading, error } = useBidHistory(filters)

  const handleFilterChange = (key: keyof BidHistoryFilters, value: string | number | undefined) => {
    setFilters((prev: BidHistoryFilters) => ({
      ...prev,
      [key]: value,
      page: 1,
    }))
  }

  const clearFilters = () => {
    setFilters({
      page: 1,
      pageSize: 20,
    })
  }

  const hasActiveFilters = filters.auctionId || filters.status || filters.fromDate || filters.toDate

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">Failed to load bid history. Please try again.</Alert>
      </Container>
    )
  }

  const totalPages = Math.ceil((data?.totalCount || 0) / (filters.pageSize || 20))

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
            <Typography variant="h3" sx={{ fontFamily: 'Russo One', fontWeight: 700, color: '#1E293B' }}>
              Bid History
            </Typography>
          </Stack>
          <Typography variant="body1" color="text.secondary">
            Complete history of all your bids across all auctions
          </Typography>
        </Box>

        <Card
          sx={{
            mb: 4,
            background: 'rgba(255, 255, 255, 0.85)',
            backdropFilter: 'blur(16px)',
            border: '1px solid rgba(255, 255, 255, 0.3)',
            borderRadius: 3,
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" mb={3}>
              <Stack direction="row" alignItems="center" spacing={1}>
                <FilterList sx={{ width: 24, height: 24, color: '#2563EB' }} />
                <Typography variant="h6" sx={{ fontFamily: 'Russo One', color: '#1E293B' }}>
                  Filters
                </Typography>
              </Stack>
              {hasActiveFilters && (
                <IconButton onClick={clearFilters} size="small" sx={{ color: '#64748B' }}>
                  <Close sx={{ width: 20, height: 20 }} />
                </IconButton>
              )}
            </Stack>

            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <TextField
                  fullWidth
                  label="Auction ID"
                  value={filters.auctionId || ''}
                  onChange={(e) => handleFilterChange('auctionId', e.target.value || undefined)}
                  size="small"
                  sx={{
                    '& .MuiInputBase-root': {
                      fontFamily: 'Chakra Petch',
                    },
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <TextField
                  fullWidth
                  select
                  label="Status"
                  value={filters.status || ''}
                  onChange={(e) => handleFilterChange('status', e.target.value || undefined)}
                  size="small"
                  sx={{
                    '& .MuiInputBase-root': {
                      fontFamily: 'Chakra Petch',
                    },
                  }}
                >
                  <MenuItem value="">All Statuses</MenuItem>
                  {Object.values(BidStatus).map((status) => (
                    <MenuItem key={status} value={status}>
                      {status}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <TextField
                  fullWidth
                  type="date"
                  label="From Date"
                  value={filters.fromDate || ''}
                  onChange={(e) => handleFilterChange('fromDate', e.target.value || undefined)}
                  size="small"
                  InputLabelProps={{ shrink: true }}
                  sx={{
                    '& .MuiInputBase-root': {
                      fontFamily: 'Chakra Petch',
                    },
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                <TextField
                  fullWidth
                  type="date"
                  label="To Date"
                  value={filters.toDate || ''}
                  onChange={(e) => handleFilterChange('toDate', e.target.value || undefined)}
                  size="small"
                  InputLabelProps={{ shrink: true }}
                  sx={{
                    '& .MuiInputBase-root': {
                      fontFamily: 'Chakra Petch',
                    },
                  }}
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        <Card
          sx={{
            background: 'rgba(255, 255, 255, 0.85)',
            backdropFilter: 'blur(16px)',
            border: '1px solid rgba(255, 255, 255, 0.3)',
            borderRadius: 3,
          }}
        >
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow sx={{ background: 'rgba(37, 99, 235, 0.05)' }}>
                  <TableCell sx={{ fontFamily: 'Russo One', color: '#1E293B', fontWeight: 700 }}>
                    Auction
                  </TableCell>
                  <TableCell sx={{ fontFamily: 'Russo One', color: '#1E293B', fontWeight: 700 }}>
                    Bid Amount
                  </TableCell>
                  <TableCell sx={{ fontFamily: 'Russo One', color: '#1E293B', fontWeight: 700 }}>
                    Status
                  </TableCell>
                  <TableCell sx={{ fontFamily: 'Russo One', color: '#1E293B', fontWeight: 700 }}>
                    Winning
                  </TableCell>
                  <TableCell sx={{ fontFamily: 'Russo One', color: '#1E293B', fontWeight: 700 }}>
                    Bid Time
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {isLoading ? (
                  [...Array(10)].map((_, index) => (
                    <TableRow key={index}>
                      <TableCell colSpan={5}>
                        <Skeleton height={40} />
                      </TableCell>
                    </TableRow>
                  ))
                ) : data?.items && data.items.length > 0 ? (
                  data.items.map((bid) => {
                    const statusColors = getBidStatusColor(bid.status)
                    return (
                      <TableRow
                        key={bid.id}
                        hover
                        sx={{
                          cursor: 'pointer',
                          '&:hover': {
                            background: 'rgba(37, 99, 235, 0.03)',
                          },
                        }}
                        onClick={() => navigate(`/auctions/${bid.auctionId}`)}
                      >
                        <TableCell>
                          <Typography
                            variant="body2"
                            sx={{
                              fontFamily: 'Chakra Petch',
                              fontWeight: 600,
                              color: '#1E293B',
                            }}
                          >
                            {bid.auctionTitle}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography
                            variant="body2"
                            sx={{
                              fontFamily: 'Russo One',
                              color: '#2563EB',
                              fontWeight: 700,
                            }}
                          >
                            {formatCurrency(bid.amount)}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={bid.status.toUpperCase()}
                            size="small"
                            sx={{
                              background: statusColors.bg,
                              color: statusColors.color,
                              fontFamily: 'Chakra Petch',
                              fontWeight: 600,
                              fontSize: '0.7rem',
                            }}
                          />
                        </TableCell>
                        <TableCell>
                          {bid.isWinning ? (
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
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}>
                            {formatDateTime(bid.bidTime)}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    )
                  })
                ) : (
                  <TableRow>
                    <TableCell colSpan={5} align="center" sx={{ py: 8 }}>
                      <Typography variant="body1" sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}>
                        No bid history found
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>

          {totalPages > 1 && (
            <Box display="flex" justifyContent="center" p={3}>
              <Pagination
                count={totalPages}
                page={filters.page || 1}
                onChange={(_, value) => handleFilterChange('page', value)}
                color="primary"
                size="large"
                sx={{
                  '& .MuiPaginationItem-root': {
                    fontFamily: 'Chakra Petch',
                    fontWeight: 600,
                  },
                  '& .Mui-selected': {
                    background: '#2563EB !important',
                    color: '#FFF',
                  },
                }}
              />
            </Box>
          )}
        </Card>
      </Container>
    </Box>
  )
}
