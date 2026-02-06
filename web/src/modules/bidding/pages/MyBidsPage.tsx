import { useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Card,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Skeleton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  CircularProgress,
  IconButton,
  Tooltip,
} from '@mui/material'
import {
  Gavel,
  EmojiEvents,
  TrendingDown,
  AccessTime,
  AutoMode,
  Delete,
  Edit,
  OpenInNew,
} from '@mui/icons-material'
import { useMyBids, useMyAutoBids, useCancelAutoBid } from '../hooks'
import { TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'
import { useUpdateAutoBid } from '../hooks/useAutoBids'
import { formatCurrency } from '@/shared/utils'
import type { AutoBid } from '../types'
import { palette } from '@/shared/theme/tokens'

const getStatusChip = (status: string, isWinning: boolean) => {
  if (isWinning) {
    return (
      <Chip
        icon={<EmojiEvents sx={{ fontSize: 16 }} />}
        label="Winning"
        size="small"
        sx={{ bgcolor: '#DCFCE7', color: '#166534' }}
      />
    )
  }

  switch (status) {
    case 'active':
      return (
        <Chip
          icon={<TrendingDown sx={{ fontSize: 16 }} />}
          label="Outbid"
          size="small"
          sx={{ bgcolor: '#FEF3C7', color: '#92400E' }}
        />
      )
    case 'ended':
      return (
        <Chip
          icon={<AccessTime sx={{ fontSize: 16 }} />}
          label="Ended"
          size="small"
          sx={{ bgcolor: '#F3F4F6', color: '#6B7280' }}
        />
      )
    default:
      return (
        <Chip
          label={status}
          size="small"
          sx={{ bgcolor: '#F3F4F6', color: '#6B7280', textTransform: 'capitalize' }}
        />
      )
  }
}

export function MyBidsPage() {
  const [activeTab, setActiveTab] = useState(0)
  const [editAutoBid, setEditAutoBid] = useState<AutoBid | null>(null)
  const [newMaxAmount, setNewMaxAmount] = useState('')

  const { data: myBids, isLoading: bidsLoading, error: bidsError } = useMyBids()
  const { data: autoBids, isLoading: autoBidsLoading } = useMyAutoBids()
  const cancelAutoBid = useCancelAutoBid()
  const updateAutoBid = useUpdateAutoBid()

  let emptyMessage = "You haven't placed any bids yet"
  if (activeTab === 1) {
    emptyMessage = 'No winning bids'
  }
  if (activeTab === 2) {
    emptyMessage = 'No outbid auctions'
  }

  const handleCancelAutoBid = async (autoBidId: string) => {
    try {
      await cancelAutoBid.mutateAsync(autoBidId)
    } catch {
      // Error handled by mutation
    }
  }

  const handleUpdateAutoBid = async () => {
    if (!editAutoBid || !newMaxAmount) {return}
    try {
      await updateAutoBid.mutateAsync({
        autoBidId: editAutoBid.id,
        data: {
          maxAmount: Number.parseFloat(newMaxAmount),
        },
      })
      setEditAutoBid(null)
    } catch {
      // Error handled by mutation
    }
  }

  const openEditDialog = (autoBid: AutoBid) => {
    setEditAutoBid(autoBid)
    setNewMaxAmount(autoBid.maxAmount.toString())
  }

  if (bidsError) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <InlineAlert severity="error">Failed to load your bids. Please try again.</InlineAlert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 600,
            color: palette.neutral[900],
          }}
        >
          My Bids
        </Typography>
        <Typography sx={{ color: palette.neutral[500] }}>Track your bidding activity</Typography>
      </Box>

      {autoBids?.autoBids?.length ? (
        <Card
          sx={{
            mb: 4,
            p: 3,
            borderRadius: 2,
            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            bgcolor: '#FFFBEB',
            border: '1px solid #FDE68A',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <AutoMode sx={{ color: palette.brand.primary }} />
            <Typography variant="h6" sx={{ fontWeight: 600, color: palette.neutral[900] }}>
              Active Auto-Bids
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {autoBidsLoading ? (
              <Skeleton variant="rectangular" height={60} />
            ) : (
              autoBids.autoBids
                .filter((ab: AutoBid) => ab.isActive)
                .map((autoBid: AutoBid) => (
                  <Box
                    key={autoBid.id}
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      p: 2,
                      bgcolor: 'white',
                      borderRadius: 1,
                    }}
                  >
                    <Box>
                      <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                        Auction #{autoBid.auctionId.slice(0, 8)}
                      </Typography>
                      <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                        Max: {formatCurrency(autoBid.maxAmount)} · Current:{' '}
                        {formatCurrency(autoBid.currentBidAmount)}
                      </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Edit Auto-Bid">
                        <IconButton
                          size="small"
                          onClick={() => openEditDialog(autoBid)}
                          sx={{ color: palette.brand.primary }}
                        >
                          <Edit fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Cancel Auto-Bid">
                        <IconButton
                          size="small"
                          onClick={() => handleCancelAutoBid(autoBid.id)}
                          disabled={cancelAutoBid.isPending}
                          sx={{ color: palette.semantic.error }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </Box>
                ))
            )}
          </Box>
        </Card>
      ) : null}

      <Card
        sx={{
          borderRadius: 2,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        }}
      >
        <Box sx={{ borderBottom: '1px solid #F5F5F5' }}>
          <Tabs
            value={activeTab}
            onChange={(_, v) => setActiveTab(v)}
            sx={{
              px: 2,
              '& .MuiTab-root': {
                textTransform: 'none',
                fontWeight: 500,
                minHeight: 56,
              },
              '& .Mui-selected': {
                color: palette.brand.primary,
              },
              '& .MuiTabs-indicator': {
                bgcolor: palette.brand.primary,
              },
            }}
          >
            <Tab icon={<Gavel sx={{ fontSize: 20 }} />} iconPosition="start" label="All Bids" />
            <Tab
              icon={<EmojiEvents sx={{ fontSize: 20 }} />}
              iconPosition="start"
              label="Winning"
            />
            <Tab
              icon={<TrendingDown sx={{ fontSize: 20 }} />}
              iconPosition="start"
              label="Outbid"
            />
          </Tabs>
        </Box>

        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Auction</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Your Bid</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="right">
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {bidsLoading && <TableSkeletonRows rows={5} columns={5} />}
              {!bidsLoading && (myBids?.length ?? 0) === 0 && (
                <TableEmptyStateRow
                  colSpan={5}
                  title={emptyMessage}
                  icon={<Gavel sx={{ fontSize: 48, color: '#D4D4D4' }} />}
                  actions={
                    <Button
                      component={Link}
                      to="/auctions"
                      variant="contained"
                      sx={{
                        bgcolor: palette.neutral[900],
                        textTransform: 'none',
                        '&:hover': { bgcolor: palette.neutral[700] },
                      }}
                    >
                      Browse Auctions
                    </Button>
                  }
                  cellSx={{ py: 8 }}
                />
              )}
              {!isLoading && (myBids?.length ?? 0) > 0 && (
                myBids?.map((bid) => (
                  <TableRow key={bid.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box
                          sx={{
                            width: 48,
                            height: 48,
                            borderRadius: 1,
                            bgcolor: '#F5F5F5',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                          }}
                        >
                          <Gavel sx={{ color: palette.neutral[500] }} />
                        </Box>
                        <Box>
                          <Typography
                            component={Link}
                            to={`/auctions/${bid.auctionId}`}
                            sx={{
                              fontWeight: 500,
                              color: palette.neutral[900],
                              textDecoration: 'none',
                              '&:hover': { color: palette.brand.primary },
                            }}
                          >
                            Auction #{bid.auctionId.slice(0, 8)}
                          </Typography>
                          <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>
                            Bid by {bid.bidderUsername}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                        {formatCurrency(bid.amount)}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ color: palette.neutral[700] }}>
                        {new Date(bid.createdAt).toLocaleDateString()}
                      </Typography>
                      <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                        {new Date(bid.createdAt).toLocaleTimeString()}
                      </Typography>
                    </TableCell>
                    <TableCell>{getStatusChip(bid.status, false)}</TableCell>
                    <TableCell align="right">
                      <Tooltip title="View Auction">
                        <IconButton
                          component={Link}
                          to={`/auctions/${bid.auctionId}`}
                          size="small"
                          sx={{ color: '#78716C' }}
                        >
                          <OpenInNew fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      <Dialog open={!!editAutoBid} onClose={() => setEditAutoBid(null)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Edit Auto-Bid</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500], mb: 3 }}>
            Update your maximum bid amount for automatic bidding.
          </Typography>

          <TextField
            fullWidth
            label="Maximum Amount"
            type="number"
            value={newMaxAmount}
            onChange={(e) => setNewMaxAmount(e.target.value)}
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            sx={{ mb: 2.5 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setEditAutoBid(null)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleUpdateAutoBid}
            disabled={!newMaxAmount || updateAutoBid.isPending}
            sx={{
              bgcolor: palette.brand.primary,
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {updateAutoBid.isPending ? <CircularProgress size={20} color="inherit" /> : 'Update'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
