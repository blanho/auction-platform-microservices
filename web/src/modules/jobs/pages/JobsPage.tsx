import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { formatNumber, formatRelativeTime } from '@/shared/utils/formatters'
import {
  Close as CancelIcon,
  Cancel as CancelledIcon,
  CheckCircle as CompletedIcon,
  OpenInNew as DetailIcon,
  Error as FailedIcon,
  HourglassEmpty as InitializingIcon,
  Work as JobIcon,
  WarningAmber as PartialIcon,
  Schedule as PendingIcon,
  PlayCircle as ProcessingIcon,
  Refresh as RefreshIcon,
  Replay as RetryIcon,
} from '@mui/icons-material'
import {
  Alert,
  Box,
  Button,
  Card,
  Chip,
  Container,
  FormControl,
  Grid,
  IconButton,
  InputLabel,
  LinearProgress,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  Tooltip,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import {
  JOB_PRIORITY_COLORS,
  JOB_STATUS_COLORS,
  JOB_STATUS_LABELS,
  JOB_TYPE_LABELS,
} from '../constants'
import { useCancelJob, useJobs, useRetryJob } from '../hooks'
import type { JobFilterParams, JobStatus, JobSummaryDto, SortDirection, SortField } from '../types'
import { isJobActive } from '../utils'

const statusIcons: Record<JobStatus, React.ReactElement> = {
  Initializing: <InitializingIcon fontSize="small" />,
  Pending: <PendingIcon fontSize="small" />,
  Processing: <ProcessingIcon fontSize="small" />,
  Completed: <CompletedIcon fontSize="small" />,
  CompletedWithErrors: <PartialIcon fontSize="small" />,
  Failed: <FailedIcon fontSize="small" />,
  Cancelled: <CancelledIcon fontSize="small" />,
}

const statusPaletteColors: Record<JobStatus, string> = {
  Initializing: palette.neutral[400],
  Pending: palette.brand.secondary,
  Processing: palette.brand.primary,
  Completed: palette.semantic.success,
  CompletedWithErrors: palette.semantic.warning,
  Failed: palette.semantic.error,
  Cancelled: palette.neutral[500],
}

interface StatCardProps {
  label: string
  value: number
  icon: React.ReactNode
  color: string
  isLoading?: boolean
}

function StatCard({ label, value, icon, color, isLoading }: Readonly<StatCardProps>) {
  return (
    <Card sx={{ p: 2.5, height: '100%' }}>
      {isLoading ? (
        <Box>
          <Skeleton width={80} height={18} />
          <Skeleton width={50} height={36} sx={{ my: 0.5 }} />
        </Box>
      ) : (
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
              {label}
            </Typography>
            <Typography variant="h4" fontWeight={700}>
              {value}
            </Typography>
          </Box>
          <Box
            sx={{
              width: 44,
              height: 44,
              borderRadius: 2,
              bgcolor: `${color}15`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color,
            }}
          >
            {icon}
          </Box>
        </Box>
      )}
    </Card>
  )
}

const PAGE_SIZE_OPTIONS = [10, 25, 50]
const DEFAULT_PAGE_SIZE = 10

export function JobsPage() {
  const { t } = useTranslation('jobs')
  const navigate = useNavigate()
  const [statusFilter, setStatusFilter] = useState<JobStatus | 'all'>('all')
  const [typeFilter, setTypeFilter] = useState<string>('all')
  const [sortField, setSortField] = useState<SortField>('createdAt')
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc')
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)

  const queryParams: JobFilterParams = useMemo(
    () => ({
      ...(statusFilter !== 'all' && { status: statusFilter }),
      ...(typeFilter !== 'all' && { jobType: typeFilter }),
      page: page + 1,
      pageSize,
    }),
    [statusFilter, typeFilter, page, pageSize]
  )

  const { data, isLoading, error, refetch } = useJobs(queryParams)
  const cancelMutation = useCancelJob()
  const retryMutation = useRetryJob()

  const jobs = useMemo(() => data?.items ?? [], [data?.items])
  const totalCount = data?.totalCount ?? 0

  const hasActiveJobs = useMemo(() => jobs.some((j) => isJobActive(j.status)), [jobs])

  const jobStats = useMemo(
    () => ({
      processing: jobs.filter((j) => j.status === 'Processing').length,
      pending: jobs.filter((j) => j.status === 'Pending' || j.status === 'Initializing').length,
      completed: jobs.filter((j) => j.status === 'Completed').length,
      failed: jobs.filter((j) => j.status === 'Failed' || j.status === 'CompletedWithErrors')
        .length,
      total: totalCount,
    }),
    [jobs, totalCount]
  )

  const sortedJobs = useMemo(() => {
    const result = [...jobs]
    result.sort((a, b) => {
      let comparison = 0
      switch (sortField) {
        case 'jobType':
          comparison = a.jobType.localeCompare(b.jobType)
          break
        case 'status':
          comparison = a.status.localeCompare(b.status)
          break
        case 'createdAt':
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          break
        case 'progress':
          comparison = a.progress - b.progress
          break
        case 'priority':
          comparison = a.priority.localeCompare(b.priority)
          break
      }
      return sortDirection === 'asc' ? comparison : -comparison
    })
    return result
  }, [jobs, sortField, sortDirection])

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortDirection('desc')
    }
  }

  const handlePageChange = (_: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handlePageSizeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(Number.parseInt(event.target.value, 10))
    setPage(0)
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
                {t('jobs.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {t('jobs.subtitle')}
              </Typography>
            </Box>
            <Stack direction="row" spacing={1}>
              {hasActiveJobs && (
                <Chip
                  size="small"
                  label={t('jobs.live')}
                  color="success"
                  variant="outlined"
                  sx={{
                    animation: 'pulse 2s infinite',
                    '@keyframes pulse': { '0%, 100%': { opacity: 1 }, '50%': { opacity: 0.5 } },
                  }}
                />
              )}
              <Button
                variant="outlined"
                startIcon={<RefreshIcon />}
                onClick={() => refetch()}
                disabled={isLoading}
                size="small"
              >
                {t('jobs.refresh')}
              </Button>
            </Stack>
          </Box>
        </motion.div>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {t('jobs.loadFailed')}
          </Alert>
        )}

        <Grid container spacing={2.5} sx={{ mb: 4 }}>
          {[
            {
              label: t('jobs.processing'),
              value: jobStats.processing,
              icon: <ProcessingIcon />,
              color: palette.brand.primary,
            },
            {
              label: t('jobs.queued'),
              value: jobStats.pending,
              icon: <PendingIcon />,
              color: palette.brand.secondary,
            },
            {
              label: t('jobs.completed'),
              value: jobStats.completed,
              icon: <CompletedIcon />,
              color: palette.semantic.success,
            },
            {
              label: t('jobs.errors'),
              value: jobStats.failed,
              icon: <FailedIcon />,
              color: palette.semantic.error,
            },
            {
              label: t('jobs.total'),
              value: jobStats.total,
              icon: <JobIcon />,
              color: palette.neutral[400],
            },
          ].map((stat) => (
            <Grid key={stat.label} size={{ xs: 6, sm: 4, md: 2.4 }}>
              <motion.div variants={staggerItem}>
                <StatCard {...stat} isLoading={isLoading} />
              </motion.div>
            </Grid>
          ))}
        </Grid>

        <motion.div variants={staggerItem}>
          <Card sx={{ p: 0 }}>
            <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
              <Stack
                direction="row"
                alignItems="center"
                justifyContent="space-between"
                flexWrap="wrap"
                gap={2}
              >
                <Typography variant="h6" fontWeight={600}>
                  {t('jobs.listTitle')}
                </Typography>
                <Stack direction="row" spacing={1.5}>
                  <FormControl size="small" sx={{ minWidth: 140 }}>
                    <InputLabel>{t('jobs.status')}</InputLabel>
                    <Select
                      value={statusFilter}
                      label={t('jobs.status')}
                      onChange={(e) => {
                        setStatusFilter(e.target.value as JobStatus | 'all')
                        setPage(0)
                      }}
                    >
                      <MenuItem value="all">{t('jobs.allStatuses')}</MenuItem>
                      {Object.entries(JOB_STATUS_LABELS).map(([key, label]) => (
                        <MenuItem key={key} value={key}>
                          {t(`statuses.${key}`, { defaultValue: label })}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                  <FormControl size="small" sx={{ minWidth: 160 }}>
                    <InputLabel>{t('jobs.type')}</InputLabel>
                    <Select
                      value={typeFilter}
                      label={t('jobs.type')}
                      onChange={(e) => {
                        setTypeFilter(e.target.value)
                        setPage(0)
                      }}
                    >
                      <MenuItem value="all">{t('jobs.allTypes')}</MenuItem>
                      {Object.entries(JOB_TYPE_LABELS).map(([key, label]) => (
                        <MenuItem key={key} value={key}>
                          {t(`types.${key}`, { defaultValue: label })}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Stack>
              </Stack>
            </Box>

            <TableContainer sx={{ overflowX: 'auto' }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'jobType'}
                        direction={sortField === 'jobType' ? sortDirection : 'asc'}
                        onClick={() => handleSort('jobType')}
                      >
                        {t('jobs.type')}
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'status'}
                        direction={sortField === 'status' ? sortDirection : 'asc'}
                        onClick={() => handleSort('status')}
                      >
                        {t('jobs.status')}
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'priority'}
                        direction={sortField === 'priority' ? sortDirection : 'asc'}
                        onClick={() => handleSort('priority')}
                      >
                        {t('jobs.priority')}
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'progress'}
                        direction={sortField === 'progress' ? sortDirection : 'asc'}
                        onClick={() => handleSort('progress')}
                      >
                        {t('jobs.progress')}
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>{t('jobs.items')}</TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={sortField === 'createdAt'}
                        direction={sortField === 'createdAt' ? sortDirection : 'asc'}
                        onClick={() => handleSort('createdAt')}
                      >
                        {t('jobs.created')}
                      </TableSortLabel>
                    </TableCell>
                    <TableCell align="right">{t('jobs.actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading &&
                    Array.from({ length: 5 }).map((_, i) => (
                      <TableRow key={`skeleton-${i.toString()}`}>
                        {Array.from({ length: 7 }).map((__, j) => (
                          <TableCell key={`skeleton-cell-${j.toString()}`}>
                            <Skeleton width={j === 3 ? 120 : 80} />
                          </TableCell>
                        ))}
                      </TableRow>
                    ))}
                  {!isLoading && sortedJobs.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center" sx={{ py: 6 }}>
                        <Box sx={{ color: 'text.secondary' }}>
                          <JobIcon sx={{ fontSize: 48, opacity: 0.4, mb: 1 }} />
                          <Typography variant="body1">{t('jobs.noJobs')}</Typography>
                          <Typography variant="body2" sx={{ mt: 0.5 }}>
                            {t('jobs.noJobsHint')}
                          </Typography>
                        </Box>
                      </TableCell>
                    </TableRow>
                  )}
                  {!isLoading &&
                    sortedJobs.map((job) => (
                      <JobRow
                        key={job.id}
                        job={job}
                        onNavigate={() => navigate(`/admin/jobs/${job.id}`)}
                        onCancel={() => cancelMutation.mutate(job.id)}
                        onRetry={() => retryMutation.mutate(job.id)}
                        isCancelling={cancelMutation.isPending}
                        isRetrying={retryMutation.isPending}
                      />
                    ))}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={totalCount}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={pageSize}
              onRowsPerPageChange={handlePageSizeChange}
              rowsPerPageOptions={PAGE_SIZE_OPTIONS}
            />
          </Card>
        </motion.div>
      </motion.div>
    </Container>
  )
}

interface JobRowProps {
  job: JobSummaryDto
  onNavigate: () => void
  onCancel: () => void
  onRetry: () => void
  isCancelling: boolean
  isRetrying: boolean
}

function JobRow({
  job,
  onNavigate,
  onCancel,
  onRetry,
  isCancelling,
  isRetrying,
}: Readonly<JobRowProps>) {
  const { t } = useTranslation('jobs')
  const active = isJobActive(job.status)
  const canRetry = job.status === 'Failed' || job.status === 'CompletedWithErrors'
  const progressColor = statusPaletteColors[job.status]

  return (
    <TableRow
      hover
      onClick={onNavigate}
      sx={{ cursor: 'pointer', '&:last-child td': { borderBottom: 0 } }}
    >
      <TableCell>
        <Typography variant="body2" fontWeight={500}>
          {t(`types.${job.jobType}`, { defaultValue: JOB_TYPE_LABELS[job.jobType] })}
        </Typography>
        <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
          {job.id.slice(0, 8)}
        </Typography>
      </TableCell>
      <TableCell>
        <Chip
          size="small"
          icon={statusIcons[job.status]}
          label={t(`statuses.${job.status}`, { defaultValue: JOB_STATUS_LABELS[job.status] })}
          color={JOB_STATUS_COLORS[job.status]}
          sx={{ height: 26 }}
        />
      </TableCell>
      <TableCell>
        <Chip
          size="small"
          label={t(`priorities.${job.priority}`, { defaultValue: job.priority })}
          color={JOB_PRIORITY_COLORS[job.priority]}
          variant="outlined"
          sx={{ height: 24 }}
        />
      </TableCell>
      <TableCell>
        {active ? (
          <Box sx={{ minWidth: 130 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
              <Typography variant="caption" color="text.secondary">
                {job.progress.toFixed(1)}%
              </Typography>
            </Box>
            <LinearProgress
              variant="determinate"
              value={job.progress}
              sx={{
                height: 6,
                borderRadius: 1,
                bgcolor: `${progressColor}20`,
                '& .MuiLinearProgress-bar': { bgcolor: progressColor },
              }}
            />
          </Box>
        ) : (
          <Typography
            variant="body2"
            color={job.status === 'Completed' ? 'success.main' : 'text.secondary'}
          >
            {job.status === 'Completed' ? '100%' : `${job.progress.toFixed(0)}%`}
          </Typography>
        )}
      </TableCell>
      <TableCell>
        <Typography variant="body2">
          {formatNumber(job.completedItems)}
          {job.failedItems > 0 && (
            <Typography component="span" variant="body2" color="error.main">
              {' '}
              ({t('jobs.failedItems', { count: job.failedItems })})
            </Typography>
          )}
          <Typography component="span" variant="body2" color="text.secondary">
            {' '}
            / {formatNumber(job.totalItems)}
          </Typography>
        </Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2">{formatRelativeTime(job.createdAt)}</Typography>
      </TableCell>
      <TableCell align="right">
        <Stack
          direction="row"
          spacing={0.5}
          justifyContent="flex-end"
          onClick={(e) => e.stopPropagation()}
        >
          {active && (
            <Tooltip title={t('jobs.cancel')}>
              <IconButton size="small" color="error" onClick={onCancel} disabled={isCancelling}>
                <CancelIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {canRetry && (
            <Tooltip title={t('jobs.retry')}>
              <IconButton size="small" color="primary" onClick={onRetry} disabled={isRetrying}>
                <RetryIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          <Tooltip title={t('jobs.viewDetails')}>
            <IconButton size="small" onClick={onNavigate}>
              <DetailIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      </TableCell>
    </TableRow>
  )
}
