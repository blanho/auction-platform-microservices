import {
  BulkImportDialog,
  ExportAuctionsDialog,
  ImportAuctionsDialog,
} from '@/modules/auctions/components'
import {
  useActivateAuction,
  useCancelAuction,
  useDeactivateAuction,
  useDeleteAuction,
  useExtendAuction,
  useMyAuctions,
} from '@/modules/auctions/hooks'
import type { AuctionListItem, AuctionStatus } from '@/modules/auctions/types'
import { palette } from '@/shared/theme/tokens'
import { TableEmptyStateRow } from '@/shared/ui'
import { formatCurrency, formatDateTime } from '@/shared/utils/formatters'
import {
  Add,
  Cancel,
  CheckCircle,
  Delete,
  DoNotDisturb,
  Edit,
  FileDownload,
  FileUpload,
  Gavel,
  MoreTime,
  MoreVert,
  Pause,
  PlayArrow,
  Search,
  Timer,
  TrendingUp,
  Visibility,
} from '@mui/icons-material'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  Chip,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  InputAdornment,
  Menu,
  MenuItem,
  Skeleton,
  Snackbar,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TablePagination,
  TableRow,
  Tabs,
  TextField,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { formatTimeLeft } from '../utils'

const statusConfig: Record<
  AuctionStatus,
  { color: string; bgColor: string; icon: React.ReactElement }
> = {
  active: {
    color: palette.semantic.success,
    bgColor: palette.semantic.successLight,
    icon: <PlayArrow sx={{ fontSize: 14 }} />,
  },
  'ending-soon': {
    color: palette.semantic.warning,
    bgColor: '#FED7AA',
    icon: <Timer sx={{ fontSize: 14 }} />,
  },
  ended: {
    color: palette.neutral[500],
    bgColor: palette.neutral[100],
    icon: <CheckCircle sx={{ fontSize: 14 }} />,
  },
  draft: {
    color: palette.brand.primary,
    bgColor: palette.brand.muted,
    icon: <Edit sx={{ fontSize: 14 }} />,
  },
  cancelled: {
    color: palette.semantic.error,
    bgColor: palette.semantic.errorLight,
    icon: <Cancel sx={{ fontSize: 14 }} />,
  },
  pending: {
    color: palette.semantic.info,
    bgColor: palette.semantic.infoLight,
    icon: <Pause sx={{ fontSize: 14 }} />,
  },
  sold: {
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
  const { t } = useTranslation('users')
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
  const [cancelDialog, setCancelDialog] = useState<string | null>(null)
  const [cancelReason, setCancelReason] = useState('')
  const [extendDialog, setExtendDialog] = useState<{ id: string; currentEndTime: string } | null>(
    null
  )
  const [newEndTime, setNewEndTime] = useState('')

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
  const cancelAuctionMutation = useCancelAuction()
  const extendAuctionMutation = useExtendAuction()

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
      { label: t('myAuctions.tabs.all'), count: allAuctionsTotalCount },
      {
        label: t('myAuctions.tabs.active'),
        count: allAuctions.filter((a) => a.status === 'active').length,
      },
      {
        label: t('myAuctions.tabs.ended'),
        count: allAuctions.filter((a) => a.status === 'ended').length,
      },
      {
        label: t('myAuctions.tabs.drafts'),
        count: allAuctions.filter((a) => a.status === 'draft').length,
      },
      {
        label: t('myAuctions.tabs.pending'),
        count: allAuctions.filter((a) => a.status === 'pending').length,
      },
    ],
    [allAuctions, allAuctionsTotalCount, t]
  )

  const handleDeleteAuction = () => {
    if (deleteDialog) {
      deleteAuctionMutation.mutate(deleteDialog, {
        onSuccess: () => {
          setSnackbar({
            open: true,
            message: t('myAuctions.messages.deleted'),
            severity: 'success',
          })
          setDeleteDialog(null)
        },
        onError: () => {
          setSnackbar({
            open: true,
            message: t('myAuctions.messages.deleteError'),
            severity: 'error',
          })
        },
      })
    }
  }

  const handleActivate = (id: string) => {
    activateAuction.mutate(id, {
      onSuccess: () => {
        setSnackbar({
          open: true,
          message: t('myAuctions.messages.activated'),
          severity: 'success',
        })
        setMenuAnchor(null)
      },
      onError: () => {
        setSnackbar({
          open: true,
          message: t('myAuctions.messages.activateError'),
          severity: 'error',
        })
      },
    })
  }

  const handleDeactivate = (id: string) => {
    deactivateAuction.mutate(id, {
      onSuccess: () => {
        setSnackbar({
          open: true,
          message: t('myAuctions.messages.deactivated'),
          severity: 'success',
        })
        setMenuAnchor(null)
      },
      onError: () => {
        setSnackbar({
          open: true,
          message: t('myAuctions.messages.deactivateError'),
          severity: 'error',
        })
      },
    })
  }

  const handleCancelAuction = () => {
    if (cancelDialog) {
      cancelAuctionMutation.mutate(
        { id: cancelDialog, reason: cancelReason || undefined },
        {
          onSuccess: () => {
            setSnackbar({
              open: true,
              message: t('myAuctions.messages.cancelled'),
              severity: 'success',
            })
            setCancelDialog(null)
            setCancelReason('')
          },
          onError: () => {
            setSnackbar({
              open: true,
              message: t('myAuctions.messages.cancelError'),
              severity: 'error',
            })
          },
        }
      )
    }
  }

  const handleExtendAuction = () => {
    if (extendDialog && newEndTime) {
      extendAuctionMutation.mutate(
        { id: extendDialog.id, newEndTime },
        {
          onSuccess: () => {
            setSnackbar({
              open: true,
              message: t('myAuctions.messages.extended'),
              severity: 'success',
            })
            setExtendDialog(null)
            setNewEndTime('')
          },
          onError: () => {
            setSnackbar({
              open: true,
              message: t('myAuctions.messages.extendError'),
              severity: 'error',
            })
          },
        }
      )
    }
  }

  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
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
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 4 }}>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          flexWrap="wrap"
          gap={2}
        >
          <Box>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                color: palette.neutral[900],
              }}
            >
              {t('myAuctions.title')}
            </Typography>
            <Typography sx={{ color: palette.neutral[500] }}>
              {t('myAuctions.description')}
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
              {t('myAuctions.export')}
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
              {t('myAuctions.import')}
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
                {t('myAuctions.quickImport')}
              </MenuItem>
              <MenuItem
                onClick={() => {
                  setImportMenuAnchor(null)
                  setBulkImportDialogOpen(true)
                }}
              >
                {t('myAuctions.bulkImport')}
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
              {t('myAuctions.createAuction')}
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
                  {t('myAuctions.totalAuctions')}
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
                  {t('myAuctions.active')}
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
                  {t('myAuctions.totalBids')}
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
                  {t('myAuctions.revenue')}
                </Typography>
                <Typography
                  sx={{ fontSize: '1.5rem', fontWeight: 700, color: palette.neutral[900] }}
                >
                  {formatCurrency(stats.totalRevenue)}
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
              placeholder={t('myAuctions.search')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search sx={{ color: palette.neutral[500] }} />
                    </InputAdornment>
                  ),
                },
              }}
              sx={{ width: { xs: '100%', sm: 250 } }}
            />
          </Stack>
        </Box>

        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.auction')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.statusLabel')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.currentBid')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.bids')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.views')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }}>
                {t('myAuctions.timeLeft')}
              </TableCell>
              <TableCell sx={{ color: palette.neutral[500], fontWeight: 500 }} align="right">
                {t('myAuctions.actions')}
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {auctions.length === 0 ? (
              <TableEmptyStateRow
                colSpan={7}
                title={t('myAuctions.noAuctionsFound')}
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
                    {t('myAuctions.createFirst')}
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
                            {t('myAuctions.startingPrice', {
                              price: formatCurrency(auction.startingPrice),
                            })}
                          </Typography>
                        </Box>
                      </Stack>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={status.icon}
                        label={t(`myAuctions.status.${auction.status}`, {
                          defaultValue: t('myAuctions.status.pending'),
                        })}
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
                        {auction.currentBid > 0 ? formatCurrency(auction.currentBid) : '--'}
                      </Typography>
                    </TableCell>
                    <TableCell>{auction.bidCount}</TableCell>
                    <TableCell>--</TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={0.5} alignItems="center">
                        <Timer sx={{ fontSize: 16, color: palette.neutral[500] }} />
                        <span>{formatTimeLeft(auction.endTime, t)}</span>
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
            setRowsPerPage(Number.parseInt(e.target.value, 10))
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
          <Visibility sx={{ mr: 1.5, fontSize: 20 }} /> {t('myAuctions.view')}
        </MenuItem>
        <MenuItem
          component={Link}
          to={`/auctions/${menuAnchor?.auction.id}/edit`}
          onClick={() => setMenuAnchor(null)}
        >
          <Edit sx={{ mr: 1.5, fontSize: 20 }} /> {t('myAuctions.edit')}
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
            {t('myAuctions.deactivate')}
          </MenuItem>
        )}
        {menuAnchor?.auction.status === 'active' && (
          <MenuItem
            onClick={() => {
              setExtendDialog({
                id: menuAnchor.auction.id,
                currentEndTime: menuAnchor.auction.endTime,
              })
              setMenuAnchor(null)
            }}
          >
            <MoreTime sx={{ mr: 1.5, fontSize: 20, color: palette.semantic.info }} />
            {t('myAuctions.extendTime')}
          </MenuItem>
        )}
        {menuAnchor?.auction.status === 'active' && (
          <MenuItem
            onClick={() => {
              setCancelDialog(menuAnchor.auction.id)
              setMenuAnchor(null)
            }}
            sx={{ color: palette.semantic.warning }}
          >
            <DoNotDisturb sx={{ mr: 1.5, fontSize: 20 }} />
            {t('myAuctions.cancelAuction')}
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
            {t('myAuctions.activate')}
          </MenuItem>
        )}
        <MenuItem
          onClick={() => {
            setDeleteDialog(menuAnchor?.auction.id || null)
            setMenuAnchor(null)
          }}
          sx={{ color: palette.semantic.error }}
        >
          <Delete sx={{ mr: 1.5, fontSize: 20 }} /> {t('myAuctions.delete')}
        </MenuItem>
      </Menu>

      <Dialog
        open={Boolean(deleteDialog)}
        onClose={() => setDeleteDialog(null)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.deleteTitle')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500] }}>
            {t('myAuctions.deleteDescription')}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setDeleteDialog(null)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('profile.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleDeleteAuction}
            disabled={deleteAuctionMutation.isPending}
            sx={{
              bgcolor: palette.semantic.error,
              textTransform: 'none',
              '&:hover': { bgcolor: palette.semantic.errorHover },
            }}
          >
            {deleteAuctionMutation.isPending ? (
              <CircularProgress size={20} sx={{ color: 'white' }} />
            ) : (
              t('myAuctions.delete')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={Boolean(cancelDialog)}
        onClose={() => {
          setCancelDialog(null)
          setCancelReason('')
        }}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.cancelAuction')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
            {t('myAuctions.cancelDescription')}
          </Typography>
          <TextField
            fullWidth
            label={t('myAuctions.cancelReason')}
            multiline
            rows={3}
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder={t('myAuctions.cancelReasonPlaceholder')}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => {
              setCancelDialog(null)
              setCancelReason('')
            }}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('myAuctions.keepAuction')}
          </Button>
          <Button
            variant="contained"
            onClick={handleCancelAuction}
            disabled={cancelAuctionMutation.isPending}
            sx={{
              bgcolor: palette.semantic.warning,
              color: palette.neutral[900],
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: '#D97706' },
            }}
          >
            {cancelAuctionMutation.isPending ? (
              <CircularProgress size={20} />
            ) : (
              t('myAuctions.cancelAuction')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={Boolean(extendDialog)}
        onClose={() => {
          setExtendDialog(null)
          setNewEndTime('')
        }}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.extendAuction')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
            {t('myAuctions.extendDescription')}
          </Typography>
          {extendDialog && (
            <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[600], mb: 2 }}>
              {t('myAuctions.currentEndTime', {
                time: formatDateTime(extendDialog.currentEndTime),
              })}
            </Typography>
          )}
          <TextField
            fullWidth
            label={t('myAuctions.newEndTime')}
            type="datetime-local"
            value={newEndTime}
            onChange={(e) => setNewEndTime(e.target.value)}
            slotProps={{
              inputLabel: { shrink: true },
              htmlInput: {
                min: extendDialog
                  ? new Date(extendDialog.currentEndTime).toISOString().slice(0, 16)
                  : undefined,
              },
            }}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => {
              setExtendDialog(null)
              setNewEndTime('')
            }}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('profile.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleExtendAuction}
            disabled={extendAuctionMutation.isPending || !newEndTime}
            sx={{
              bgcolor: palette.semantic.info,
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: '#0284C7' },
            }}
          >
            {extendAuctionMutation.isPending ? (
              <CircularProgress size={20} sx={{ color: 'white' }} />
            ) : (
              t('myAuctions.extendAuction')
            )}
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

      <ExportAuctionsDialog open={exportDialogOpen} onClose={() => setExportDialogOpen(false)} />

      <ImportAuctionsDialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        onSuccess={() => {
          setSnackbar({
            open: true,
            message: t('myAuctions.messages.imported'),
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
            message: t('myAuctions.messages.bulkImportCompleted'),
            severity: 'success',
          })
        }}
      />
    </Container>
  )
}

export default MyAuctionsPage
