import { useState, useMemo } from 'react'
import {
  Container,
  Typography,
  Box,
  Card,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Skeleton,
  Pagination,
  Tooltip,
  Stack,
} from '@mui/material'
import { InlineAlert, StatusBadge, TableEmptyStateRow, TableToolbar } from '@/shared/ui'
import type { FilterConfig } from '@/shared/ui'
import { Visibility, Delete, CheckCircle } from '@mui/icons-material'
import { useReportList, useReportStats, useReportDetail } from '../hooks/useAnalytics'
import { useUpdateReportStatus, useDeleteReport } from '../hooks/useReportMutations'
import { formatNumber } from '@/shared/utils/formatters'
import type { ReportStatus, ReportType, ReportPriority, ReportQueryParams, Report } from '../types'

const REPORT_STATUS_OPTIONS: {
  value: ReportStatus
  label: string
  color: 'default' | 'warning' | 'success' | 'error'
}[] = [
    { value: 'Pending', label: 'Pending', color: 'warning' },
    { value: 'UnderReview', label: 'Under Review', color: 'default' },
    { value: 'Resolved', label: 'Resolved', color: 'success' },
    { value: 'Dismissed', label: 'Dismissed', color: 'error' },
  ]

const REPORT_TYPE_OPTIONS: { value: ReportType; label: string }[] = [
  { value: 'Fraud', label: 'Fraud' },
  { value: 'FakeItem', label: 'Fake Item' },
  { value: 'NonPayment', label: 'Non-Payment' },
  { value: 'Harassment', label: 'Harassment' },
  { value: 'InappropriateContent', label: 'Inappropriate Content' },
  { value: 'SuspiciousActivity', label: 'Suspicious Activity' },
  { value: 'Other', label: 'Other' },
]

const REPORT_PRIORITY_OPTIONS: {
  value: ReportPriority
  label: string
  color: 'default' | 'info' | 'warning' | 'error'
}[] = [
    { value: 'Low', label: 'Low', color: 'default' },
    { value: 'Medium', label: 'Medium', color: 'info' },
    { value: 'High', label: 'High', color: 'warning' },
    { value: 'Critical', label: 'Critical', color: 'error' },
  ]

const getStatusChip = (status: ReportStatus) => {
  return <StatusBadge status={status} />
}

const getPriorityChip = (priority: ReportPriority) => {
  const option = REPORT_PRIORITY_OPTIONS.find((o) => o.value === priority)
  return (
    <Chip
      label={option?.label ?? priority}
      color={option?.color ?? 'default'}
      size="small"
      variant="outlined"
    />
  )
}

const getTypeLabel = (type: ReportType) => {
  const option = REPORT_TYPE_OPTIONS.find((o) => o.value === type)
  return option?.label ?? type
}

export const ReportsPage = () => {
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

  const toolbarFilters: FilterConfig[] = useMemo(
    () => [
      { key: 'status', label: 'Status', options: REPORT_STATUS_OPTIONS, minWidth: 120 },
      { key: 'type', label: 'Type', options: REPORT_TYPE_OPTIONS, minWidth: 150 },
      { key: 'priority', label: 'Priority', options: REPORT_PRIORITY_OPTIONS, minWidth: 120 },
    ],
    []
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

  const { data: reportsData, isLoading: reportsLoading, refetch } = useReportList(filters)
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
    if (!selectedReportId) { return }
    try {
      await updateMutation.mutateAsync({
        id: selectedReportId,
        data: { status: updateStatus, resolution: updateResolution || undefined },
      })
      setUpdateDialogOpen(false)
      setSelectedReportId(null)
    } catch (error) {
      console.error('Failed to update report:', error)
    }
  }

  const handleDeleteReport = async () => {
    if (!selectedReportId) { return }
    try {
      await deleteMutation.mutateAsync(selectedReportId)
      setDeleteDialogOpen(false)
      setSelectedReportId(null)
    } catch (error) {
      console.error('Failed to delete report:', error)
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
          Reports Management
        </Typography>
        <Typography color="text.secondary">Review and manage user reports</Typography>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statsLoading && (
          [1, 2, 3, 4, 5, 6, 7].map((i) => (
            <Grid key={i} size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Skeleton height={80} width={120} />
            </Grid>
          ))
        )}
        {!statsLoading && stats && (
          <>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120 }}>
                <Typography variant="h4" fontWeight={700}>
                  {formatNumber(stats.totalReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Total
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'warning.50' }}>
                <Typography variant="h4" fontWeight={700} color="warning.main">
                  {formatNumber(stats.pendingReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Pending
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'info.50' }}>
                <Typography variant="h4" fontWeight={700} color="info.main">
                  {formatNumber(stats.underReviewReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Under Review
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'success.50' }}>
                <Typography variant="h4" fontWeight={700} color="success.main">
                  {formatNumber(stats.resolvedReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Resolved
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120 }}>
                <Typography variant="h4" fontWeight={700} color="text.secondary">
                  {formatNumber(stats.dismissedReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Dismissed
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'error.50' }}>
                <Typography variant="h4" fontWeight={700} color="error.main">
                  {formatNumber(stats.criticalReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Critical
                </Typography>
              </Card>
            </Grid>
            <Grid size={{ xs: 6, sm: 4, md: 'auto' }}>
              <Card sx={{ p: 2, textAlign: 'center', minWidth: 120, bgcolor: 'warning.50' }}>
                <Typography variant="h4" fontWeight={700} color="warning.dark">
                  {formatNumber(stats.highPriorityReports)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  High Priority
                </Typography>
              </Card>
            </Grid>
          </>
        )}
        {!statsLoading && !stats && (
          <Grid size={{ xs: 12 }}>
            <InlineAlert severity="error">Failed to load report statistics</InlineAlert>
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
            placeholder="Reported Username"
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
                <TableCell>Reporter</TableCell>
                <TableCell>Reported User</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>Reason</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {reportsLoading && (
                [1, 2, 3, 4, 5].map((i) => (
                  <TableRow key={i}>
                    {[1, 2, 3, 4, 5, 6, 7, 8].map((j) => (
                      <TableCell key={j}>
                        <Skeleton />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              )}
              {!reportsLoading && reportsData?.items && reportsData.items.length > 0 && (
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
                    <TableCell>{new Date(report.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                        <Tooltip title="View Details">
                          <IconButton size="small" onClick={() => handleViewReport(report.id)}>
                            <Visibility fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Update Status">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenUpdateDialog(report)}
                          >
                            <CheckCircle fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
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
                ))
              )}
              {!reportsLoading && (!reportsData?.items || reportsData.items.length === 0) && (
                <TableEmptyStateRow colSpan={8} title="No reports found" cellSx={{ py: 4 }} />
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
        <DialogTitle>Report Details</DialogTitle>
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
                  Reporter
                </Typography>
                <Typography>{selectedReport.reporterUsername}</Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Reported User
                </Typography>
                <Typography>{selectedReport.reportedUsername}</Typography>
              </Box>
              {selectedReport.auctionId && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Auction ID
                  </Typography>
                  <Typography>{selectedReport.auctionId}</Typography>
                </Box>
              )}
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Type
                </Typography>
                <Typography>{getTypeLabel(selectedReport.type)}</Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Priority
                </Typography>
                <Box>{getPriorityChip(selectedReport.priority)}</Box>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Status
                </Typography>
                <Box>{getStatusChip(selectedReport.status)}</Box>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Reason
                </Typography>
                <Typography>{selectedReport.reason}</Typography>
              </Box>
              {selectedReport.description && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Description
                  </Typography>
                  <Typography>{selectedReport.description}</Typography>
                </Box>
              )}
              {selectedReport.resolution && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Resolution
                  </Typography>
                  <Typography>{selectedReport.resolution}</Typography>
                </Box>
              )}
              {selectedReport.resolvedBy && (
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    Resolved By
                  </Typography>
                  <Typography>
                    {selectedReport.resolvedBy} at{' '}
                    {selectedReport.resolvedAt
                      ? new Date(selectedReport.resolvedAt).toLocaleString()
                      : 'N/A'}
                  </Typography>
                </Box>
              )}
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Created At
                </Typography>
                <Typography>{new Date(selectedReport.createdAt).toLocaleString()}</Typography>
              </Box>
            </Stack>
          )}
          {!reportDetailLoading && !selectedReport && (
            <Typography color="text.secondary">Report not found</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={updateDialogOpen}
        onClose={() => setUpdateDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Update Report Status</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={updateStatus}
                label="Status"
                onChange={(e) => setUpdateStatus(e.target.value as ReportStatus)}
              >
                {REPORT_STATUS_OPTIONS.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Resolution Notes"
              multiline
              rows={4}
              value={updateResolution}
              onChange={(e) => setUpdateResolution(e.target.value)}
              placeholder="Enter resolution notes (optional)"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUpdateDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleUpdateStatus}
            disabled={updateMutation.isPending}
          >
            {updateMutation.isPending ? 'Updating...' : 'Update'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Report</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this report? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button
            color="error"
            variant="contained"
            onClick={handleDeleteReport}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
