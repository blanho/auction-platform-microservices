import i18n, { getCurrentLocale } from '@/i18n'
import { getErrorMessage } from '@/services/http'
import type { FilterConfig } from '@/shared/ui'
import { InlineAlert, StatusBadge, TableEmptyStateRow, TableToolbar } from '@/shared/ui'
import { formatNumber } from '@/shared/utils/formatters'
import { CheckCircle, Delete, Visibility } from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  IconButton,
  InputLabel,
  MenuItem,
  Pagination,
  Select,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useReportDetail, useReportList, useReportStats } from '../hooks/useAnalytics'
import { useDeleteReport, useUpdateReportStatus } from '../hooks/useReportMutations'
import type { Report, ReportPriority, ReportQueryParams, ReportStatus, ReportType } from '../types'

const REPORT_STATUS_OPTIONS: {
  value: ReportStatus
  color: 'default' | 'warning' | 'success' | 'error'
}[] = [
  { value: 'Pending', color: 'warning' },
  { value: 'UnderReview', color: 'default' },
  { value: 'Resolved', color: 'success' },
  { value: 'Dismissed', color: 'error' },
]

const REPORT_TYPES: ReportType[] = [
  'Fraud',
  'FakeItem',
  'NonPayment',
  'Harassment',
  'InappropriateContent',
  'SuspiciousActivity',
  'Other',
]

const REPORT_PRIORITY_OPTIONS: {
  value: ReportPriority
  color: 'default' | 'info' | 'warning' | 'error'
}[] = [
  { value: 'Low', color: 'default' },
  { value: 'Medium', color: 'info' },
  { value: 'High', color: 'warning' },
  { value: 'Critical', color: 'error' },
]

const getStatusChip = (status: ReportStatus) => {
  return <StatusBadge status={status} label={i18n.t(`analytics:reportStatus.${status}`)} />
}

const getPriorityChip = (priority: ReportPriority) => {
  const option = REPORT_PRIORITY_OPTIONS.find((o) => o.value === priority)
  return (
    <Chip
      label={i18n.t(`analytics:reportPriority.${priority}`)}
      color={option?.color ?? 'default'}
      size="small"
      variant="outlined"
    />
  )
}

const getTypeLabel = (type: ReportType) => {
  return i18n.t(`analytics:reportTypes.${type}`)
}

export const ReportsPage = () => {
  const { t } = useTranslation('analytics')
  const [filters, setFilters] = useState<ReportQueryParams>({
    page: 1,
    pageSize: 10,
  })
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null)
  const [viewDialogOpen, setViewDialogOpen] = useState(false)
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [updateStatus, setUpdateStatus] = useState<ReportStatus>('UnderReview')
  const [updateResolution, setUpdateResolution] = useState('')
  const [actionError, setActionError] = useState<string | null>(null)

  const toolbarFilters: FilterConfig[] = useMemo(
    () => [
      {
        key: 'status',
        label: t('filter.status'),
        options: REPORT_STATUS_OPTIONS.map(({ value, color }) => ({
          value,
          color,
          label: t(`reportStatus.${value}`),
        })),
        minWidth: 120,
      },
      {
        key: 'type',
        label: t('filter.type'),
        options: REPORT_TYPES.map((value) => ({ value, label: t(`reportTypes.${value}`) })),
        minWidth: 150,
      },
      {
        key: 'priority',
        label: t('filter.priority'),
        options: REPORT_PRIORITY_OPTIONS.map(({ value, color }) => ({
          value,
          color,
          label: t(`reportPriority.${value}`),
        })),
        minWidth: 120,
      },
    ],
    [t]
  )

  const toolbarFilterValues = useMemo(
    () => ({
      status: filters.status ?? '',
      type: filters.type ?? '',
      priority: filters.priority ?? '',
    }),
    [filters.status, filters.type, filters.priority]
  )

  const handleToolbarFilterChange = (key: string, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value || undefined, page: 1 }))
  }

  const handleClearFilters = () => {
    setFilters({ page: 1, pageSize: 10 })
  }

  const {
    data: reportsData,
    isLoading: reportsLoading,
    error: reportsError,
    refetch,
  } = useReportList(filters)
  const { data: stats, isLoading: statsLoading } = useReportStats()
  const { data: selectedReport, isLoading: reportDetailLoading } = useReportDetail(
    selectedReportId ?? ''
  )
  const updateMutation = useUpdateReportStatus()
  const deleteMutation = useDeleteReport()

  const handleFilterChange = (key: keyof ReportQueryParams, value: unknown) => {
    setFilters((prev) => ({ ...prev, [key]: value, page: 1 }))
  }

  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    setFilters((prev) => ({ ...prev, page }))
  }

  const handleViewReport = (id: string) => {
    setSelectedReportId(id)
    setViewDialogOpen(true)
  }

  const handleOpenUpdateDialog = (report: Report) => {
    setSelectedReportId(report.id)
    setUpdateStatus(report.status === 'Pending' ? 'UnderReview' : report.status)
    setUpdateResolution(report.resolution ?? '')
    setUpdateDialogOpen(true)
  }

  const handleUpdateStatus = async () => {
    if (!selectedReportId) {
      return
    }
    setActionError(null)
    try {
      await updateMutation.mutateAsync({
        id: selectedReportId,
        data: { status: updateStatus, resolution: updateResolution || undefined },
      })
      setUpdateDialogOpen(false)
      setSelectedReportId(null)
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  const handleDeleteReport = async () => {
    if (!selectedReportId) {
      return
    }
    setActionError(null)
    try {
      await deleteMutation.mutateAsync(selectedReportId)
      setDeleteDialogOpen(false)
      setSelectedReportId(null)
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  const handleOpenDeleteDialog = (id: string) => {
    setSelectedReportId(id)
    setDeleteDialogOpen(true)
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 700,
            color: 'primary.main',
            mb: 1,
          }}
        >
          {t('reports.title')}
        </Typography>
        <Typography color="text.secondary">{t('reports.description')}</Typography>
      </Box>

      {(reportsError || actionError) && (
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {actionError ?? getErrorMessage(reportsError)}
        </InlineAlert>
      )}

      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statsLoading &&
          [1, 2, 3, 4, 5, 6, 7].map((i) => (
            <Grid key={i} size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Skeleton height={80} width={120} />
            </Grid>
          ))}
        {!statsLoading && stats && (
          <>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120 }}>
                <Typography variant="h4" fontWeight={700}>
                  {formatNumber(stats.totalReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.total')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'warning.50' }}>
                <Typography variant="h4" fontWeight={700} color="warning.main">
                  {formatNumber(stats.pendingReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.pending')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'info.50' }}>
                <Typography variant="h4" fontWeight={700} color="info.main">
                  {formatNumber(stats.underReviewReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.underReview')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'success.50' }}>
                <Typography variant="h4" fontWeight={700} color="success.main">
                  {formatNumber(stats.resolvedReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.resolved')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120 }}>
                <Typography variant="h4" fontWeight={700} color="text.secondary">
                  {formatNumber(stats.dismissedReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.dismissed')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'error.50' }}>
                <Typography variant="h4" fontWeight={700} color="error.main">
                  {formatNumber(stats.criticalReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.critical')}
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'warning.50' }}>
                <Typography variant="h4" fontWeight={700} color="warning.dark">
                  {formatNumber(stats.highPriorityReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {t('reports.highPriority')}
                </Typography>
              </Card>
            </Grid>
          </>
        )}
        {!statsLoading && !stats && (
          <Grid size={{ xs: 12 }}>
            <InlineAlert severity="error">{t('reports.failedToLoad')}</InlineAlert>
          </Grid>
        )}
      </Grid>

      <Card sx={{ mb: 3, p: 2 }}>
        <TableToolbar
          filters={toolbarFilters}
          filterValues={toolbarFilterValues}
          onFilterChange={handleToolbarFilterChange}
          onClearFilters={handleClearFilters}
          onRefresh={() => refetch()}
        >
          <TextField
            size="small"
            placeholder={t('filter.reportedUsername')}
            value={filters.reportedUsername ?? ''}
            onChange={(e) => handleFilterChange('reportedUsername', e.target.value || undefined)}
            sx={{ minWidth: 180 }}
          />
        </TableToolbar>
      </Card>

      <Card>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>{t('reports.reporter')}</TableCell>
                <TableCell>{t('reports.reportedUser')}</TableCell>
                <TableCell>{t('reports.type')}</TableCell>
                <TableCell>{t('reports.priority')}</TableCell>
                <TableCell>{t('reports.reason')}</TableCell>
                <TableCell>{t('reports.status')}</TableCell>
                <TableCell>{t('reports.created')}</TableCell>
                <TableCell align="right">{t('reports.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {reportsLoading &&
                [1, 2, 3, 4, 5].map((i) => (
                  <TableRow key={i}>
                    {[1, 2, 3, 4, 5, 6, 7, 8].map((j) => (
                      <TableCell key={j}>
                        <Skeleton />
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              {!reportsLoading &&
                reportsData?.items &&
                reportsData.items.length > 0 &&
                reportsData.items.map((report) => (
                  <TableRow key={report.id} hover>
                    <TableCell>{report.reporterUsername}</TableCell>
                    <TableCell>{report.reportedUsername}</TableCell>
                    <TableCell>{getTypeLabel(report.type)}</TableCell>
                    <TableCell>{getPriorityChip(report.priority)}</TableCell>
                    <TableCell sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {report.reason}
                    </TableCell>
                    <TableCell>{getStatusChip(report.status)}</TableCell>
                    <TableCell>
                      {new Date(report.createdAt).toLocaleDateString(getCurrentLocale())}
                    </TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                        <Tooltip title={t('reports.viewDetails')}>
                          <IconButton size="small" onClick={() => handleViewReport(report.id)}>
                            <Visibility fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title={t('reports.updateStatus')}>
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenUpdateDialog(report)}
                          >
                            <CheckCircle fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title={t('reports.delete')}>
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleOpenDeleteDialog(report.id)}
                          >
                            <Delete fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              {!reportsLoading && (!reportsData?.items || reportsData.items.length === 0) && (
                <TableEmptyStateRow colSpan={8} title={t('reports.noReports')} cellSx={{ py: 4 }} />
              )}
            </TableBody>
          </Table>
        </TableContainer>
        {reportsData && reportsData.totalPages > 1 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
            <Pagination
              count={reportsData.totalPages}
              page={filters.page ?? 1}
              onChange={handlePageChange}
              color="primary"
            />
          </Box>
        )}
      </Card>

      <Dialog
        open={viewDialogOpen}
        onClose={() => setViewDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{t('reports.reportDetails')}</DialogTitle>
        <DialogContent>
          {reportDetailLoading && (
            <Stack spacing={2} sx={{ py: 2 }}>
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} height={40} />
              ))}
            </Stack>
          )}
          {!reportDetailLoading && selectedReport && (
            <Stack spacing={2} sx={{ pt: 1 }}>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.reporter')}
                </Typography>
                <Typography>{selectedReport.reporterUsername}</Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.reportedUser')}
                </Typography>
                <Typography>{selectedReport.reportedUsername}</Typography>
              </Box>
              {selectedReport.auctionId && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('reports.auctionId')}
                  </Typography>
                  <Typography>{selectedReport.auctionId}</Typography>
                </Box>
              )}
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.type')}
                </Typography>
                <Typography>{getTypeLabel(selectedReport.type)}</Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.priority')}
                </Typography>
                <Box>{getPriorityChip(selectedReport.priority)}</Box>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.status')}
                </Typography>
                <Box>{getStatusChip(selectedReport.status)}</Box>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.reason')}
                </Typography>
                <Typography>{selectedReport.reason}</Typography>
              </Box>
              {selectedReport.description && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('reports.descriptionLabel')}
                  </Typography>
                  <Typography>{selectedReport.description}</Typography>
                </Box>
              )}
              {selectedReport.resolution && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('reports.resolution')}
                  </Typography>
                  <Typography>{selectedReport.resolution}</Typography>
                </Box>
              )}
              {selectedReport.resolvedBy && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('reports.resolvedBy')}
                  </Typography>
                  <Typography>
                    {selectedReport.resolvedBy} {t('reports.at')}{' '}
                    {selectedReport.resolvedAt
                      ? new Date(selectedReport.resolvedAt).toLocaleString(getCurrentLocale())
                      : t('reports.notAvailable')}
                  </Typography>
                </Box>
              )}
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('reports.createdAt')}
                </Typography>
                <Typography>
                  {new Date(selectedReport.createdAt).toLocaleString(getCurrentLocale())}
                </Typography>
              </Box>
            </Stack>
          )}
          {!reportDetailLoading && !selectedReport && (
            <Typography color="text.secondary">{t('reports.notFound')}</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>{t('reports.close')}</Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={updateDialogOpen}
        onClose={() => setUpdateDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{t('reports.updateStatus')}</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>{t('reports.status')}</InputLabel>
              <Select
                value={updateStatus}
                label={t('reports.status')}
                onChange={(e) => setUpdateStatus(e.target.value as ReportStatus)}
              >
                {REPORT_STATUS_OPTIONS.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {t(`reportStatus.${opt.value}`)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('reports.resolutionNotes')}
              multiline
              rows={4}
              value={updateResolution}
              onChange={(e) => setUpdateResolution(e.target.value)}
              placeholder={t('reports.resolutionPlaceholder')}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUpdateDialogOpen(false)}>{t('reports.cancel')}</Button>
          <Button
            variant="contained"
            onClick={handleUpdateStatus}
            disabled={updateMutation.isPending}
          >
            {updateMutation.isPending ? t('reports.updating') : t('reports.update')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>{t('reports.deleteReport')}</DialogTitle>
        <DialogContent>
          <Typography>{t('reports.confirmDelete')}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>{t('reports.cancel')}</Button>
          <Button
            color="error"
            variant="contained"
            onClick={handleDeleteReport}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? t('reports.deleting') : t('reports.delete')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
