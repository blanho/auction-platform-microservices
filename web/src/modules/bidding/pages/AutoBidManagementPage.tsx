import { useState, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Chip,
  Button,
  Stack,
  Container,
  Switch,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Pagination,
  Skeleton,
  Tooltip,
} from '@mui/material'
import { FlashOn, Edit, Delete, Add } from '@mui/icons-material'
import {
  useMyAutoBids,
  useToggleAutoBid,
  useCancelAutoBid,
  useUpdateAutoBid,
} from '../hooks/useAutoBids'
import { BID_CONSTANTS } from '../constants'
import { formatCurrency, formatDateTime } from '@/shared/utils'
import type { AutoBid, UpdateAutoBidRequest } from '../types'
import { palette } from '@/shared/theme/tokens'

export const AutoBidManagementPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const [activeOnly, setActiveOnly] = useState<boolean | undefined>(undefined)
  const [editDialogOpen, setEditDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)

  const skeletonKeys = useMemo(
    () => Array.from({ length: 6 }, () => crypto.randomUUID()),
    []
  )
  const [selectedAutoBid, setSelectedAutoBid] = useState<AutoBid | null>(null)
  const [editForm, setEditForm] = useState<UpdateAutoBidRequest>({})

  const pageSize = BID_CONSTANTS.PAGE_SIZE
  const { data, isLoading, error } = useMyAutoBids(activeOnly, page, pageSize)
  const toggleMutation = useToggleAutoBid()
  const cancelMutation = useCancelAutoBid()
  const updateMutation = useUpdateAutoBid()

  const handleToggle = async (autoBid: AutoBid) => {
    try {
      await toggleMutation.mutateAsync({
        autoBidId: autoBid.id,
        activate: !autoBid.isActive,
      })
    } catch (error) {
      console.error('Failed to toggle auto bid:', error)
    }
  }

  const handleEdit = (autoBid: AutoBid) => {
    setSelectedAutoBid(autoBid)
    setEditForm({
      maxAmount: autoBid.maxAmount,
      isActive: autoBid.isActive,
    })
    setEditDialogOpen(true)
  }

  const handleSaveEdit = async () => {
    if (!selectedAutoBid) {return}

    try {
      await updateMutation.mutateAsync({
        autoBidId: selectedAutoBid.id,
        data: editForm,
      })
      setEditDialogOpen(false)
      setSelectedAutoBid(null)
    } catch (error) {
      console.error('Failed to update auto bid:', error)
    }
  }

  const handleDelete = (autoBid: AutoBid) => {
    setSelectedAutoBid(autoBid)
    setDeleteDialogOpen(true)
  }

  const confirmDelete = async () => {
    if (!selectedAutoBid) {return}

    try {
      await cancelMutation.mutateAsync(selectedAutoBid.id)
      setDeleteDialogOpen(false)
      setSelectedAutoBid(null)
    } catch (error) {
      console.error('Failed to cancel auto bid:', error)
    }
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <InlineAlert severity="error">Failed to load auto bids. Please try again.</InlineAlert>
      </Container>
    )
  }

  const totalPages = Math.ceil((data?.totalCount || 0) / pageSize)

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
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Box>
              <Stack direction="row" alignItems="center" spacing={2} mb={1}>
                <FlashOn sx={{ width: 40, height: 40, color: '#2563EB' }} />
                <Typography
                  variant="h3"
                  sx={{ fontFamily: 'Russo One', fontWeight: 700, color: '#1E293B' }}
                >
                  Auto Bid Manager
                </Typography>
              </Stack>
              <Typography variant="body1" color="text.secondary">
                Configure and manage your automatic bidding strategies
              </Typography>
            </Box>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/auctions')}
              sx={{
                background: 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
                fontFamily: 'Chakra Petch',
                fontWeight: 700,
                px: 4,
                py: 1.5,
                '&:hover': {
                  background: 'linear-gradient(135deg, #EA580C 0%, #F97316 100%)',
                },
              }}
            >
              Create Auto Bid
            </Button>
          </Stack>
        </Box>

        <Card
          sx={{
            mb: 4,
            p: 2,
            background: 'rgba(255, 255, 255, 0.85)',
            backdropFilter: 'blur(16px)',
            border: '1px solid rgba(255, 255, 255, 0.3)',
            borderRadius: 3,
          }}
        >
          <Stack direction="row" alignItems="center" spacing={2}>
            <Typography
              variant="body2"
              sx={{ fontFamily: 'Chakra Petch', fontWeight: 600, color: '#64748B' }}
            >
              Show:
            </Typography>
            <Button
              variant={activeOnly === undefined ? 'contained' : 'outlined'}
              onClick={() => setActiveOnly(undefined)}
              sx={{
                fontFamily: 'Chakra Petch',
                fontWeight: 600,
                ...(activeOnly === undefined && {
                  background: '#2563EB',
                  color: '#FFF',
                }),
              }}
            >
              All
            </Button>
            <Button
              variant={activeOnly === true ? 'contained' : 'outlined'}
              onClick={() => setActiveOnly(true)}
              sx={{
                fontFamily: 'Chakra Petch',
                fontWeight: 600,
                ...(activeOnly === true && {
                  background: '#2563EB',
                  color: '#FFF',
                }),
              }}
            >
              Active Only
            </Button>
            <Button
              variant={activeOnly === false ? 'contained' : 'outlined'}
              onClick={() => setActiveOnly(false)}
              sx={{
                fontFamily: 'Chakra Petch',
                fontWeight: 600,
                ...(activeOnly === false && {
                  background: '#2563EB',
                  color: '#FFF',
                }),
              }}
            >
              Inactive Only
            </Button>
          </Stack>
        </Card>

        {isLoading && (
          <Grid container spacing={3}>
            {skeletonKeys.map((key) => (
              <Grid key={key} size={{ xs: 12, sm: 6, md: 4 }}>
                <Skeleton variant="rounded" height={300} sx={{ borderRadius: 3 }} />
              </Grid>
            ))}
          </Grid>
        )}
        {!isLoading && data?.autoBids && data.autoBids.length > 0 && (
          <>
            <Grid container spacing={3} mb={4}>
              {data.autoBids.map((autoBid: AutoBid) => (
                <Grid key={autoBid.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Card
                    sx={{
                      height: '100%',
                      background: 'rgba(255, 255, 255, 0.85)',
                      backdropFilter: 'blur(16px)',
                      border: autoBid.isActive
                        ? '1px solid rgba(34, 197, 94, 0.3)'
                        : '1px solid rgba(148, 163, 184, 0.3)',
                      borderRadius: 3,
                      transition: 'all 0.3s ease',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: '0 12px 24px rgba(37, 99, 235, 0.15)',
                      },
                    }}
                  >
                    <CardContent sx={{ p: 3 }}>
                      <Stack
                        direction="row"
                        alignItems="center"
                        justifyContent="space-between"
                        mb={2}
                      >
                        <Stack direction="row" alignItems="center" spacing={1}>
                          <FlashOn
                            sx={{
                              width: 24,
                              height: 24,
                              color: autoBid.isActive ? palette.semantic.success : '#94A3B8',
                            }}
                          />
                          <Chip
                            label={autoBid.isActive ? 'ACTIVE' : 'INACTIVE'}
                            size="small"
                            sx={{
                              background: autoBid.isActive
                                ? 'rgba(34, 197, 94, 0.1)'
                                : 'rgba(148, 163, 184, 0.1)',
                              color: autoBid.isActive ? '#16A34A' : '#64748B',
                              fontFamily: 'Chakra Petch',
                              fontWeight: 700,
                              fontSize: '0.7rem',
                            }}
                          />
                        </Stack>
                        <Switch
                          checked={autoBid.isActive}
                          onChange={() => handleToggle(autoBid)}
                          disabled={toggleMutation.isPending}
                        />
                      </Stack>

                      <Typography
                        variant="body2"
                        sx={{
                          color: '#64748B',
                          fontFamily: 'Chakra Petch',
                          fontWeight: 500,
                          mb: 0.5,
                        }}
                      >
                        Auction ID
                      </Typography>
                      <Typography
                        variant="body1"
                        sx={{
                          fontFamily: 'Chakra Petch',
                          color: '#1E293B',
                          fontWeight: 600,
                          mb: 2,
                          cursor: 'pointer',
                          '&:hover': { color: '#2563EB' },
                        }}
                        onClick={() => navigate(`/auctions/${autoBid.auctionId}`)}
                      >
                        {autoBid.auctionId}
                      </Typography>

                      <Stack spacing={2} mb={3}>
                        <Box>
                          <Typography
                            variant="caption"
                            sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}
                          >
                            Max Amount
                          </Typography>
                          <Typography
                            variant="h6"
                            sx={{
                              color: '#2563EB',
                              fontFamily: 'Russo One',
                              fontWeight: 700,
                            }}
                          >
                            {formatCurrency(autoBid.maxAmount)}
                          </Typography>
                        </Box>

                        <Box>
                          <Typography
                            variant="caption"
                            sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}
                          >
                            Current Bid
                          </Typography>
                          <Typography
                            variant="body2"
                            sx={{ color: '#1E293B', fontFamily: 'Chakra Petch' }}
                          >
                            {formatCurrency(autoBid.currentBidAmount)}
                          </Typography>
                        </Box>

                        <Box>
                          <Typography
                            variant="caption"
                            sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}
                          >
                            Created
                          </Typography>
                          <Typography
                            variant="body2"
                            sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}
                          >
                            {formatDateTime(autoBid.createdAt)}
                          </Typography>
                        </Box>

                        {autoBid.lastBidAt && (
                          <Box>
                            <Typography
                              variant="caption"
                              sx={{ color: '#94A3B8', fontFamily: 'Chakra Petch' }}
                            >
                              Last Bid
                            </Typography>
                            <Typography
                              variant="body2"
                              sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}
                            >
                              {formatDateTime(autoBid.lastBidAt)}
                            </Typography>
                          </Box>
                        )}
                      </Stack>

                      <Stack direction="row" spacing={1}>
                        <Tooltip title="Edit">
                          <IconButton
                            size="small"
                            onClick={() => handleEdit(autoBid)}
                            sx={{
                              background: 'rgba(37, 99, 235, 0.1)',
                              color: '#2563EB',
                              '&:hover': {
                                background: 'rgba(37, 99, 235, 0.2)',
                              },
                            }}
                          >
                            <Edit sx={{ width: 16, height: 16 }} />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            size="small"
                            onClick={() => handleDelete(autoBid)}
                            sx={{
                              background: 'rgba(239, 68, 68, 0.1)',
                              color: '#DC2626',
                              '&:hover': {
                                background: 'rgba(239, 68, 68, 0.2)',
                              },
                            }}
                          >
                            <Delete sx={{ width: 16, height: 16 }} />
                          </IconButton>
                        </Tooltip>
                      </Stack>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>

            {totalPages > 1 && (
              <Box display="flex" justifyContent="center">
                <Pagination
                  count={totalPages}
                  page={page}
                  onChange={(_, value) => setPage(value)}
                  color="primary"
                  size="large"
                  sx={{
                    '& .MuiPaginationItem-root': {
                      fontFamily: 'Chakra Petch',
                      fontWeight: 600,
                      background: 'rgba(255, 255, 255, 0.7)',
                      backdropFilter: 'blur(8px)',
                    },
                    '& .Mui-selected': {
                      background: '#2563EB !important',
                      color: '#FFF',
                    },
                  }}
                />
              </Box>
            )}
          </>
        )}
        {!isLoading && (!data || data.length === 0) && (
          <Card
            sx={{
              p: 6,
              textAlign: 'center',
              background: 'rgba(255, 255, 255, 0.7)',
              backdropFilter: 'blur(12px)',
              border: '1px solid rgba(255, 255, 255, 0.3)',
              borderRadius: 3,
            }}
          >
            <FlashOn sx={{ width: 80, height: 80, color: '#CBD5E1', margin: '0 auto 16px' }} />
            <Typography variant="h6" sx={{ fontFamily: 'Russo One', color: '#64748B', mb: 2 }}>
              No Auto Bids Configured
            </Typography>
            <Typography variant="body2" color="text.secondary" mb={3}>
              Set up automatic bidding to stay competitive without constant monitoring
            </Typography>
            <Button
              variant="contained"
              onClick={() => navigate('/auctions')}
              sx={{
                background: 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
                fontFamily: 'Chakra Petch',
                fontWeight: 700,
                px: 4,
                py: 1.5,
                '&:hover': {
                  background: 'linear-gradient(135deg, #EA580C 0%, #F97316 100%)',
                },
              }}
            >
              Browse Auctions
            </Button>
          </Card>
        )}
      </Container>

      <Dialog
        open={editDialogOpen}
        onClose={() => setEditDialogOpen(false)}
        slotProps={{
          paper: {
            sx: {
              background: 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              borderRadius: 3,
              border: '1px solid rgba(255, 255, 255, 0.3)',
            },
          },
        }}
      >
        <DialogTitle sx={{ fontFamily: 'Russo One', color: '#1E293B' }}>Edit Auto Bid</DialogTitle>
        <DialogContent>
          <Stack spacing={3} mt={2}>
            <TextField
              fullWidth
              type="number"
              label="Max Amount"
              value={editForm.maxAmount || ''}
              onChange={(e) => setEditForm({ ...editForm, maxAmount: Number(e.target.value) })}
              sx={{
                '& .MuiInputBase-root': {
                  fontFamily: 'Chakra Petch',
                },
              }}
            />
            <TextField
              fullWidth
              type="number"
              label="Bid Increment"
              value={editForm.bidIncrement || ''}
              onChange={(e) => setEditForm({ ...editForm, bidIncrement: Number(e.target.value) })}
              sx={{
                '& .MuiInputBase-root': {
                  fontFamily: 'Chakra Petch',
                },
              }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)} sx={{ fontFamily: 'Chakra Petch' }}>
            Cancel
          </Button>
          <Button
            onClick={handleSaveEdit}
            variant="contained"
            disabled={updateMutation.isPending}
            sx={{
              background: '#2563EB',
              fontFamily: 'Chakra Petch',
              fontWeight: 600,
            }}
          >
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        slotProps={{
          paper: {
            sx: {
              background: 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              borderRadius: 3,
              border: '1px solid rgba(255, 255, 255, 0.3)',
            },
          },
        }}
      >
        <DialogTitle sx={{ fontFamily: 'Russo One', color: '#1E293B' }}>Confirm Delete</DialogTitle>
        <DialogContent>
          <Typography sx={{ fontFamily: 'Chakra Petch' }}>
            Are you sure you want to cancel this auto bid? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)} sx={{ fontFamily: 'Chakra Petch' }}>
            Cancel
          </Button>
          <Button
            onClick={confirmDelete}
            variant="contained"
            disabled={cancelMutation.isPending}
            sx={{
              background: '#DC2626',
              fontFamily: 'Chakra Petch',
              fontWeight: 600,
              '&:hover': {
                background: palette.semantic.errorHover,
              },
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
