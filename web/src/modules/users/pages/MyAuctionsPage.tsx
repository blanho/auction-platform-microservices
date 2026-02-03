import { useState, useMemo } from 'react'
import { palette } from '@/shared/theme/tokens'
import { TableEmptyStateRow } from '@/shared/ui'
import {
  Container,
  Typography,
  Box,
  Grid,
  Card,
  Stack,
  Button,
  Chip,
  Tabs,
  Tab,
  IconButton,
  Menu,
  MenuItem,
  TextField,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TablePagination,
  Avatar,
  Skeleton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar,
  Alert,
  CircularProgress,
} from '@mui/material'
import {
  Add,
  Search,
  MoreVert,
  Edit,
  Delete,
  Visibility,
  Timer,
  Gavel,
  TrendingUp,
  CheckCircle,
  Cancel,
  Pause,
  PlayArrow,
  FileDownload,
  FileUpload,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import {
  useMyAuctions,
  useActivateAuction,
  useDeactivateAuction,
  useDeleteAuction,
} from '@/modules/auctions/hooks'
import { ExportAuctionsDialog } from '@/modules/auctions/components/ExportAuctionsDialog'
import { ImportAuctionsDialog } from '@/modules/auctions/components/ImportAuctionsDialog'
import { BulkImportDialog } from '@/modules/auctions/components/BulkImportDialog'
import type { AuctionStatus, AuctionListItem } from '@/modules/auctions/types'
import { formatTimeLeft } from '../utils'

const statusConfig: Record<
  AuctionStatus,
  { label: string; color: string; bgColor: string; icon: React.ReactElement }
> = {
  active: {
    label: 'Active',
    color: palette.semantic.success,
    bgColor: palette.semantic.successLight,
    icon: <PlayArrow sx={{ fontSize: 14 }} />,
  },
  'ending-soon': {
    label: 'Ending Soon',
    color: palette.semantic.warning,
    bgColor: '#FED7AA',
    icon: <Timer sx={{ fontSize: 14 }} />,
  },
  ended: {
    label: 'Ended',
    color: palette.neutral[500],
    bgColor: palette.neutral[100],
    icon: <CheckCircle sx={{ fontSize: 14 }} />,
  },
  draft: {
    label: 'Draft',
    color: palette.brand.primary,
    bgColor: palette.brand.muted,
    icon: <Edit sx={{ fontSize: 14 }} />,
  },
  cancelled: {
    label: 'Cancelled',
    color: palette.semantic.error,
    bgColor: palette.semantic.errorLight,
    icon: <Cancel sx={{ fontSize: 14 }} />,
  },
  pending: {
    label: 'Pending Review',
    color: palette.semantic.info,
    bgColor: palette.semantic.infoLight,
    icon: <Pause sx={{ fontSize: 14 }} />,
  },
  sold: {
    label: 'Sold',
    color: palette.semantic.success,
    bgColor: palette.semantic.successLight,
    icon: <CheckCircle sx={{ fontSize: 14 }} />,
  },
}

const STATUS_TAB_MAP: (AuctionStatus | undefined)[] = [
  undefined,
  'active',
  'ended',
  'draft',
  'pending',
]

export function MyAuctionsPage() {
  const [tabValue, setTabValue] = useState(0)
  const [searchQuery, setSearchQuery] = useState('')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [menuAnchor, setMenuAnchor] = useState<{
    el: HTMLElement
    auction: AuctionListItem
  } | null>(null)
  const [deleteDialog, setDeleteDialog] = useState<string | null>(null)
  const [exportDialogOpen, setExportDialogOpen] = useState(false)
  const [importDialogOpen, setImportDialogOpen] = useState(false)
  const [bulkImportDialogOpen, setBulkImportDialogOpen] = useState(false)
  const [importMenuAnchor, setImportMenuAnchor] = useState<HTMLElement | null>(null)
  const [snackbar, setSnackbar] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error'
  }>({ open: false, message: '', severity: 'success' })

  const statusFilter = STATUS_TAB_MAP[tabValue]

  const { data: auctionsData, isLoading } = useMyAuctions({
    status: statusFilter,
    searchTerm: searchQuery || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  })

  const { data: allAuctionsData } = useMyAuctions({})

  const activateAuction = useActivateAuction()
  const deactivateAuction = useDeactivateAuction()
  const deleteAuctionMutation = useDeleteAuction()

  const auctions = auctionsData?.items ?? []
  const totalCount = auctionsData?.totalCount ?? 0
  const allAuctionsItems = allAuctionsData?.items
  const allAuctionsTotalCount = allAuctionsData?.totalCount ?? 0

  const allAuctions = useMemo(() => allAuctionsItems ?? [], [allAuctionsItems])

  const stats = useMemo(
    () => ({
      totalAuctions: allAuctionsTotalCount,
      activeAuctions: allAuctions.filter((a) => a.status === 'active').length,
      totalBids: allAuctions.reduce((sum, a) => sum + (a.bidCount || 0), 0),
      totalRevenue: allAuctions
        .filter((a) => a.status === 'ended')
        .reduce((sum, a) => sum + (a.currentBid || 0), 0),
    }),
    [allAuctions, allAuctionsTotalCount]
  )

  const tabs = useMemo(
    () => [
      { label: 'All', count: allAuctionsTotalCount },
      { label: 'Active', count: allAuctions.filter((a) => a.status === 'active').length },
      { label: 'Ended', count: allAuctions.filter((a) => a.status === 'ended').length },
      { label: 'Drafts', count: allAuctions.filter((a) => a.status === 'draft').length },
      { label: 'Pending', count: allAuctions.filter((a) => a.status === 'pending').length },
    ],
    [allAuctions, allAuctionsTotalCount]
  )

  const handleDeleteAuction = () => {
    if (deleteDialog) {
      deleteAuctionMutation.mutate(deleteDialog, {
        onSuccess: () => {
          setSnackbar({ open: true, message: 'Auction deleted successfully', severity: 'success' })
          setDeleteDialog(null)
        },
        onError: () => {
          setSnackbar({ open: true, message: 'Failed to delete auction', severity: 'error' })
        },
      })
    }
  }

  const handleActivate = (id: string) => {
    activateAuction.mutate(id, {
      onSuccess: () => {
        setSnackbar({ open: true, message: 'Auction activated successfully', severity: 'success' })
        setMenuAnchor(null)
      },
      onError: () => {
        setSnackbar({ open: true, message: 'Failed to activate auction', severity: 'error' })
      },
    })
  }

  const handleDeactivate = (id: string) => {
    deactivateAuction.mutate(id, {
      onSuccess: () => {
        setSnackbar({
          open: true,
          message: 'Auction deactivated successfully',
          severity: 'success',
        })
        setMenuAnchor(null)
      },
      onError: () => {
        setSnackbar({ open: true, message: 'Failed to deactivate auction', severity: 'error' })
      },
    })
  }

  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Skeleton variant="text" width={200} height={40} sx={{ mb: 2 }} />
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {[1, 2, 3, 4].map((i) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={i}>
              <Skeleton variant="rectangular" height={100} sx={{ borderRadius: 2 }} />
            </Grid>
          ))}
        </Grid>
        <Skeleton variant="rectangular" height={400} sx={{ borderRadius: 2 }} />
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: palette.neutral[900],
              }}
            >
              My Auctions
            </Typography>
            <Typography sx={{ color: palette.neutral[500] }}>
              Manage your auction listings
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              startIcon={<FileDownload />}
              onClick={() => setExportDialogOpen(true)}
              sx={{
                borderColor: palette.neutral[300],
                color: palette.neutral[700],
                textTransform: 'none',
                fontWeight: 500,
                '&:hover': { borderColor: palette.brand.primary, color: palette.brand.primary },
              }}
            >
              Export
            </Button>
            <Button
              variant="outlined"
              startIcon={<FileUpload />}
              onClick={(e) => setImportMenuAnchor(e.currentTarget)}
              sx={{
                borderColor: palette.neutral[300],
                color: palette.neutral[700],
                textTransform: 'none',
                fontWeight: 500,
                '&:hover': { borderColor: palette.brand.primary, color: palette.brand.primary },
              }}
            >
              Import
            </Button>
            <Menu
              anchorEl={importMenuAnchor}
              open={Boolean(importMenuAnchor)}
              onClose={() => setImportMenuAnchor(null)}
            >
              <MenuItem
                onClick={() => {
                  setImportMenuAnchor(null)
                  setImportDialogOpen(true)
                }}
              >
                Quick Import (small files)
              </MenuItem>
              <MenuItem
                onClick={() => {
                  setImportMenuAnchor(null)
                  setBulkImportDialogOpen(true)
                }}
              >
                Bulk Import (large files, up to 1M records)
              </MenuItem>
            </Menu>
            <Button
              variant="contained"
              startIcon={<Add />}
              component={Link}
              to="/auctions/create"
              sx={{
                bgcolor: palette.brand.primary,
                textTransform: 'none',
                fontWeight: 600,
                px: 3,
                '&:hover': { bgcolor: '#A16207' },
              }}
            >
              Create Auction
            </Button>
          </Stack>
        </Stack>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Box
                sx={{
                  width: 48,
                  height: 48,
                  borderRadius: 2,
                  bgcolor: palette.brand.muted,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Gavel sx={{ color: palette.brand.primary }} />
              </Box>
              <Box>
                <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                  Total Auctions
                </Typography>
                <Typography
                  sx={{ fontSize: '1.5rem', fontWeight: 700, color: palette.neutral[900] }}
                >
                  {stats.totalAuctions}
                </Typography>
              </Box>
            </Stack>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Box
                sx={{
                  width: 48,
                  height: 48,
                  borderRadius: 2,
                  bgcolor: palette.semantic.successLight,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <PlayArrow sx={{ color: palette.semantic.success }} />
              </Box>
              <Box>
                <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                  Active
                </Typography>
                <Typography
                  sx={{ fontSize: '1.5rem', fontWeight: 700, color: palette.neutral[900] }}
                >
                  {stats.activeAuctions}
                </Typography>
              </Box>
            </Stack>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Box
                sx={{
                  width: 48,
                  height: 48,
                  borderRadius: 2,
                  bgcolor: palette.semantic.infoLight,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <TrendingUp sx={{ color: palette.semantic.info }} />
              </Box>
              <Box>
                <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                  Total Bids
                </Typography>
                <Typography
                  sx={{ fontSize: '1.5rem', fontWeight: 700, color: palette.neutral[900] }}
                >
                  {stats.totalBids}
                </Typography>
              </Box>
            </Stack>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ p: 3, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
            <Stack direction="row" spacing={2} alignItems="center">
              <Box
                sx={{
                  width: 48,
                  height: 48,
                  borderRadius: 2,
                  bgcolor: '#EDE9FE',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <CheckCircle sx={{ color: '#7C3AED' }} />
              </Box>
              <Box>
                <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                  Revenue
                </Typography>
                <Typography
                  sx={{ fontSize: '1.5rem', fontWeight: 700, color: palette.neutral[900] }}
                >
                  ${stats.totalRevenue.toLocaleString()}
                </Typography>
              </Box>
            </Stack>
          </Card>
        </Grid>
      </Grid>

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <Box sx={{ borderBottom: '1px solid #E5E5E5' }}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            justifyContent="space-between"
            alignItems={{ xs: 'stretch', sm: 'center' }}
            spacing={2}
            sx={{ p: 2 }}
          >
            <Tabs
              value={tabValue}
              onChange={(_, v) => setTabValue(v)}
              sx={{
                minHeight: 40,
                '& .MuiTab-root': {
                  textTransform: 'none',
                  minHeight: 40,
                  fontWeight: 500,
                  color: palette.neutral[500],
                  '&.Mui-selected': { color: palette.neutral[900] },
                },
                '& .MuiTabs-indicator': { bgcolor: palette.brand.primary },
              }}
            >
              {tabs.map((tab, index) => (
                <Tab
                  key={tab.label}
                  label={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <span>{tab.label}</span>
                      <Chip
                        label={tab.count}
                        size="small"
                        sx={{
                          height: 20,
                          fontSize: '0.75rem',
                          bgcolor: index === tabValue ? palette.brand.muted : palette.neutral[100],
                        }}
                      />
                    </Stack>
                  }
                />
              ))}
            </Tabs>
            <TextField
              size="small"
              placeholder="Search auctions..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Search sx={{ color: palette.neutral[500] }} />
                  </InputAdornment>
                ),
              }}
              sx={{ width: { xs: '100%', sm: 250 } }}
            />
          </Stack>
        </Box>

        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>Auction</TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>Status</TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                Current Bid
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>Bids</TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>Views</TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>Time Left</TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }} align="right">
                Actions
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {auctions.length === 0 ? (
              <TableEmptyStateRow
                colSpan={7}
                title="No auctions found"
                icon={<Gavel sx={{ fontSize: 48, opacity: 0.5, color: palette.neutral[500] }} />}
                actions={
                  <Button
                    variant="outlined"
                    component={Link}
                    to="/auctions/create"
                    sx={{
                      borderColor: palette.brand.primary,
                      color: palette.brand.primary,
                      textTransform: 'none',
                    }}
                  >
                    Create your first auction
                  </Button>
                }
                cellSx={{ py: 6 }}
              />
            ) : (
              auctions.map((auction) => {
                const status = statusConfig[auction.status] ?? statusConfig.pending
                return (
                  <TableRow key={auction.id} sx={{ '&:hover': { bgcolor: palette.neutral[50] } }}>
                    <TableCell>
                      <Stack direction="row" spacing={2} alignItems="center">
                        <Avatar
                          variant="rounded"
                          src={auction.primaryImageUrl}
                          sx={{ width: 56, height: 56, bgcolor: palette.neutral[100] }}
                        >
                          <Gavel />
                        </Avatar>
                        <Box>
                          <Typography
                            component={Link}
                            to={`/auctions/${auction.id}`}
                            sx={{
                              fontWeight: 500,
                              color: palette.neutral[900],
                              textDecoration: 'none',
                              '&:hover': { color: palette.brand.primary },
                            }}
                          >
                            {auction.title}
                          </Typography>
                          <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                            Starting: ${auction.startingPrice.toLocaleString()}
                          </Typography>
                        </Box>
                      </Stack>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={status.icon}
                        label={status.label}
                        size="small"
                        sx={{
                          bgcolor: status.bgColor,
                          color: status.color,
                          fontWeight: 500,
                          '& .MuiChip-icon': { color: status.color },
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                        {auction.currentBid > 0 ? `$${auction.currentBid.toLocaleString()}` : '--'}
                      </Typography>
                    </TableCell>
                    <TableCell>{auction.bidCount}</TableCell>
                    <TableCell>--</TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={0.5} alignItems="center">
                        <Timer sx={{ fontSize: 16, color: palette.neutral[500] }} />
                        <span>{formatTimeLeft(auction.endTime)}</span>
                      </Stack>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        onClick={(e) => setMenuAnchor({ el: e.currentTarget, auction })}
                      >
                        <MoreVert />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>

        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, p) => setPage(p)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => {
            setRowsPerPage(parseInt(e.target.value, 10))
            setPage(0)
          }}
          rowsPerPageOptions={[5, 10, 25]}
        />
      </Card>

      <Menu
        anchorEl={menuAnchor?.el}
        open={Boolean(menuAnchor)}
        onClose={() => setMenuAnchor(null)}
      >
        <MenuItem
          component={Link}
          to={`/auctions/${menuAnchor?.auction.id}`}
          onClick={() => setMenuAnchor(null)}
        >
          <Visibility sx={{ mr: 1.5, fontSize: 20 }} /> View
        </MenuItem>
        <MenuItem
          component={Link}
          to={`/auctions/${menuAnchor?.auction.id}/edit`}
          onClick={() => setMenuAnchor(null)}
        >
          <Edit sx={{ mr: 1.5, fontSize: 20 }} /> Edit
        </MenuItem>
        {menuAnchor?.auction.status === 'active' && (
          <MenuItem
            onClick={() => handleDeactivate(menuAnchor.auction.id)}
            disabled={deactivateAuction.isPending}
          >
            {deactivateAuction.isPending ? (
              <CircularProgress size={16} sx={{ mr: 1.5 }} />
            ) : (
              <Pause sx={{ mr: 1.5, fontSize: 20, color: palette.brand.primary }} />
            )}
            Deactivate
          </MenuItem>
        )}
        {(menuAnchor?.auction.status === 'draft' || menuAnchor?.auction.status === 'pending') && (
          <MenuItem
            onClick={() => handleActivate(menuAnchor.auction.id)}
            disabled={activateAuction.isPending}
            sx={{ color: palette.semantic.success }}
          >
            {activateAuction.isPending ? (
              <CircularProgress size={16} sx={{ mr: 1.5 }} />
            ) : (
              <PlayArrow sx={{ mr: 1.5, fontSize: 20 }} />
            )}
            Activate
          </MenuItem>
        )}
        <MenuItem
          onClick={() => {
            setDeleteDialog(menuAnchor?.auction.id || null)
            setMenuAnchor(null)
          }}
          sx={{ color: palette.semantic.error }}
        >
          <Delete sx={{ mr: 1.5, fontSize: 20 }} /> Delete
        </MenuItem>
      </Menu>

      <Dialog
        open={Boolean(deleteDialog)}
        onClose={() => setDeleteDialog(null)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>Delete Auction?</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500] }}>
            This action cannot be undone. The auction and all associated data will be permanently
            deleted.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setDeleteDialog(null)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleDeleteAuction}
            sx={{
              bgcolor: palette.semantic.error,
              textTransform: 'none',
              '&:hover': { bgcolor: palette.semantic.errorHover },
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>

      <ExportAuctionsDialog
        open={exportDialogOpen}
        onClose={() => setExportDialogOpen(false)}
      />

      <ImportAuctionsDialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        onSuccess={() => {
          setSnackbar({
            open: true,
            message: 'Auctions imported successfully!',
            severity: 'success',
          })
        }}
      />

      <BulkImportDialog
        open={bulkImportDialogOpen}
        onClose={() => setBulkImportDialogOpen(false)}
        onComplete={() => {
          setSnackbar({
            open: true,
            message: 'Bulk import completed! Check the results for details.',
            severity: 'success',
          })
        }}
      />
    </Container>
  )
}

export default MyAuctionsPage
