import { useState, useMemo } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Grid,
  Card,
  Typography,
  Box,
  Stack,
  LinearProgress,
  Chip,
  IconButton,
  Tooltip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Skeleton,
  Alert,
  Button,
} from '@mui/material'
import {
  Schedule as PendingIcon,
  PlayCircle as RunningIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Cancel as CancelledIcon,
  Close as CancelIcon,
  Download as DownloadIcon,
  Refresh as RefreshIcon,
  Work as JobIcon,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import {
  useBackgroundJobs,
  useCancelJob,
  useDownloadJobResult,
} from '@/shared/hooks/useBackgroundJobs'
import type { BackgroundJobProgress, BackgroundJobStatus } from '@/shared/types/background-jobs.types'
import { formatRelativeTime } from '@/shared/utils/formatters'

interface StatusConfig {
  label: string
  color: string
  icon: React.ReactElement
  chipColor: 'default' | 'primary' | 'success' | 'error' | 'warning'
}

const statusConfig: Record<BackgroundJobStatus, StatusConfig> = {
  Pending: {
    label: 'Pending',
    color: palette.neutral[500],
    icon: <PendingIcon />,
    chipColor: 'default',
  },
  Running: {
    label: 'Running',
    color: palette.brand.primary,
    icon: <RunningIcon />,
    chipColor: 'primary',
  },
  Completed: {
    label: 'Completed',
    color: palette.semantic.success,
    icon: <SuccessIcon />,
    chipColor: 'success',
  },
  Failed: {
    label: 'Failed',
    color: palette.semantic.error,
    icon: <ErrorIcon />,
    chipColor: 'error',
  },
  Cancelled: {
    label: 'Cancelled',
    color: palette.semantic.warning,
    icon: <CancelledIcon />,
    chipColor: 'warning',
  },
}

interface StatCardProps {
  label: string
  value: number
  icon: React.ReactNode
  color: string
  isLoading?: boolean
}

function StatCard({ label, value, icon, color, isLoading }: StatCardProps) {
  return (
    <Card sx={{ p: 3, height: '100%' }}>
      {isLoading ? (
        <Box>
          <Skeleton width={80} height={20} />
          <Skeleton width={60} height={40} sx={{ my: 1 }} />
        </Box>
      ) : (
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {label}
            </Typography>
            <Typography variant="h4" fontWeight={700}>
              {value}
            </Typography>
          </Box>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              bgcolor: `${color}15`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: color,
            }}
          >
            {icon}
          </Box>
        </Box>
      )}
    </Card>
  )
}

function formatTimeRemaining(seconds: number): string {
  if (seconds <= 0) {
    return 'Almost done...'
  }
  if (seconds < 60) {
    return `${seconds}s`
  }
  if (seconds < 3600) {
    return `${Math.floor(seconds / 60)}m ${seconds % 60}s`
  }
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}h ${minutes}m`
}

function formatFileSize(bytes?: number): string {
  if (!bytes) {
    return ''
  }
  if (bytes < 1024) {
    return `${bytes} B`
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`
  }
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

type SortField = 'name' | 'status' | 'createdAt' | 'progressPercentage'
type SortDirection = 'asc' | 'desc'

export function BackgroundJobsPage() {
  const [statusFilter, setStatusFilter] = useState<BackgroundJobStatus | 'all'>('all')
  const [sortField, setSortField] = useState<SortField>('createdAt')
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc')

  const { data: jobs = [], isLoading, error, refetch } = useBackgroundJobs()
  const cancelMutation = useCancelJob()
  const downloadMutation = useDownloadJobResult()

  const jobStats = useMemo(() => {
    return {
      pending: jobs.filter((j) => j.status === 'Pending').length,
      running: jobs.filter((j) => j.status === 'Running').length,
      completed: jobs.filter((j) => j.status === 'Completed').length,
      failed: jobs.filter((j) => j.status === 'Failed').length,
      cancelled: jobs.filter((j) => j.status === 'Cancelled').length,
      total: jobs.length,
    }
  }, [jobs])

  const filteredAndSortedJobs = useMemo(() => {
    let result = [...jobs]

    if (statusFilter !== 'all') {
      result = result.filter((j) => j.status === statusFilter)
    }

    result.sort((a, b) => {
      let comparison = 0
      switch (sortField) {
        case 'name':
          comparison = a.name.localeCompare(b.name)
          break
        case 'status':
          comparison = a.status.localeCompare(b.status)
          break
        case 'createdAt':
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          break
        case 'progressPercentage':
          comparison = a.progressPercentage - b.progressPercentage
          break
      }
      return sortDirection === 'asc' ? comparison : -comparison
    })

    return result
  }, [jobs, statusFilter, sortField, sortDirection])

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('desc')
    }
  }

  const handleCancel = (jobId: string) => {
    cancelMutation.mutate(jobId)
  }

  const handleDownload = (jobId: string, filename: string) => {
    downloadMutation.mutate({ jobId, filename })
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: 'primary.main',
                }}
              >
                Background Jobs
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Monitor and manage your export and processing tasks
              </Typography>
            </Box>
            <Button
              variant="outlined"
              startIcon={<RefreshIcon />}
              onClick={() => refetch()}
              disabled={isLoading}
            >
              Refresh
            </Button>
          </Box>
        </motion.div>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            Failed to load background jobs. Please try again later.
          </Alert>
        )}

        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid size={{ xs: 12, sm: 6, md: 2.4 }}>
            <motion.div variants={staggerItem}>
              <StatCard
                label="Pending"
                value={jobStats.pending}
                icon={<PendingIcon />}
                color={palette.neutral[500]}
                isLoading={isLoading}
              />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.4 }}>
            <motion.div variants={staggerItem}>
              <StatCard
                label="Running"
                value={jobStats.running}
                icon={<RunningIcon />}
                color={palette.brand.primary}
                isLoading={isLoading}
              />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.4 }}>
            <motion.div variants={staggerItem}>
              <StatCard
                label="Completed"
                value={jobStats.completed}
                icon={<SuccessIcon />}
                color={palette.semantic.success}
                isLoading={isLoading}
              />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.4 }}>
            <motion.div variants={staggerItem}>
              <StatCard
                label="Failed"
                value={jobStats.failed}
                icon={<ErrorIcon />}
                color={palette.semantic.error}
                isLoading={isLoading}
              />
            </motion.div>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.4 }}>
            <motion.div variants={staggerItem}>
              <StatCard
                label="Total Jobs"
                value={jobStats.total}
                icon={<JobIcon />}
                color={palette.brand.secondary}
                isLoading={isLoading}
              />
            </motion.div>
          </Grid>
        </Grid>

        <motion.div variants={staggerItem}>
          <Card sx={{ p: 0 }}>
            <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
              <Stack direction="row" alignItems="center" justifyContent="space-between">
                <Typography variant="h6" fontWeight={600}>
                  Jobs History
                </Typography>
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={statusFilter}
                    label="Status"
                    onChange={(e) => setStatusFilter(e.target.value as BackgroundJobStatus | 'all')}
                  >
                    <MenuItem value="all">All Status</MenuItem>
                    <MenuItem value="Pending">Pending</MenuItem>
                    <MenuItem value="Running">Running</MenuItem>
                    <MenuItem value="Completed">Completed</MenuItem>
                    <MenuItem value="Failed">Failed</MenuItem>
                    <MenuItem value="Cancelled">Cancelled</MenuItem>
                  </Select>
                </FormControl>
              </Stack>
            </Box>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'name'}
                        direction={sortField === 'name' ? sortDirection : 'asc'}
                        onClick={() => handleSort('name')}
                      >
                        Job Name
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'status'}
                        direction={sortField === 'status' ? sortDirection : 'asc'}
                        onClick={() => handleSort('status')}
                      >
                        Status
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'progressPercentage'}
                        direction={sortField === 'progressPercentage' ? sortDirection : 'asc'}
                        onClick={() => handleSort('progressPercentage')}
                      >
                        Progress
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'createdAt'}
                        direction={sortField === 'createdAt' ? sortDirection : 'asc'}
                        onClick={() => handleSort('createdAt')}
                      >
                        Created
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>Details</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading && (
                    Array.from({ length: 5 }).map((_, i) => (
                      <TableRow key={i}>
                        <TableCell><Skeleton width={150} /></TableCell>
                        <TableCell><Skeleton width={80} /></TableCell>
                        <TableCell><Skeleton width={120} /></TableCell>
                        <TableCell><Skeleton width={100} /></TableCell>
                        <TableCell><Skeleton width={150} /></TableCell>
                        <TableCell><Skeleton width={80} /></TableCell>
                      </TableRow>
                    ))
                  )}
                  {!isLoading && filteredAndSortedJobs.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                        <Box sx={{ color: 'text.secondary' }}>
                          <JobIcon sx={{ fontSize: 48, opacity: 0.5, mb: 1 }} />
                          <Typography variant="body1">No background jobs found</Typography>
                          <Typography variant="body2" sx={{ mt: 0.5 }}>
                            Jobs will appear here when you start export or report tasks
                          </Typography>
                        </Box>
                      </TableCell>
                    </TableRow>
                  )}
                  {!isLoading && filteredAndSortedJobs.length > 0 && (
                    filteredAndSortedJobs.map((job) => (
                      <JobRow
                        key={job.jobId}
                        job={job}
                        onCancel={handleCancel}
                        onDownload={handleDownload}
                        isCancelling={cancelMutation.isPending}
                        isDownloading={downloadMutation.isPending}
                      />
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </motion.div>
      </motion.div>
    </Container>
  )
}

interface JobRowProps {
  job: BackgroundJobProgress
  onCancel: (jobId: string) => void
  onDownload: (jobId: string, filename: string) => void
  isCancelling: boolean
  isDownloading: boolean
}

function JobRow({ job, onCancel, onDownload, isCancelling, isDownloading }: JobRowProps) {
  const config = statusConfig[job.status]
  const isActive = job.status === 'Running' || job.status === 'Pending'
  const canDownload = job.status === 'Completed' && job.resultFileName

  return (
    <TableRow hover>
      <TableCell>
        <Typography variant="body2" fontWeight={500}>
          {job.name}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {job.jobType}
        </Typography>
      </TableCell>
      <TableCell>
        <Chip
          size="small"
          icon={config.icon}
          label={config.label}
          color={config.chipColor}
          sx={{ height: 24 }}
        />
      </TableCell>
      <TableCell>
        {isActive && (
          <Box sx={{ minWidth: 150 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
              <Typography variant="caption" color="text.secondary">
                {job.processedItems.toLocaleString()} / {job.totalItems.toLocaleString()}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {job.progressPercentage.toFixed(1)}%
              </Typography>
            </Box>
            <LinearProgress
              variant="determinate"
              value={job.progressPercentage}
              sx={{ height: 6, borderRadius: 1 }}
            />
            {job.estimatedSecondsRemaining > 0 && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                ~{formatTimeRemaining(job.estimatedSecondsRemaining)} remaining
              </Typography>
            )}
          </Box>
        )}
        {!isActive && job.status === 'Completed' && (
          <Typography variant="body2" color="success.main">
            100%
          </Typography>
        )}
        {!isActive && job.status !== 'Completed' && (
          <Typography variant="body2" color="text.secondary">
            —
          </Typography>
        )}
      </TableCell>
      <TableCell>
        <Typography variant="body2">
          {formatRelativeTime(job.createdAt)}
        </Typography>
      </TableCell>
      <TableCell>
        {job.status === 'Completed' && job.resultFileName && (
          <Typography variant="caption" color="text.secondary">
            {job.resultFileName}
            {job.resultFileSizeBytes && ` (${formatFileSize(job.resultFileSizeBytes)})`}
          </Typography>
        )}
        {job.status === 'Failed' && job.errorMessage && (
          <Tooltip title={job.errorMessage}>
            <Typography
              variant="caption"
              color="error"
              sx={{
                display: 'block',
                maxWidth: 200,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
              }}
            >
              {job.errorMessage}
            </Typography>
          </Tooltip>
        )}
        {!['Completed', 'Failed'].includes(job.status) && (
          <Typography variant="caption" color="text.secondary">
            —
          </Typography>
        )}
      </TableCell>
      <TableCell align="right">
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          {isActive && (
            <Tooltip title="Cancel Job">
              <IconButton
                size="small"
                color="error"
                onClick={() => onCancel(job.jobId)}
                disabled={isCancelling}
              >
                <CancelIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {canDownload && job.resultFileName && (
            <Tooltip title="Download Result">
              <IconButton
                size="small"
                color="primary"
                onClick={() => onDownload(job.jobId, job.resultFileName ?? '')}
                disabled={isDownloading}
              >
                <DownloadIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      </TableCell>
    </TableRow>
  )
}
