import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { formatNumber, formatRelativeTime } from '@/shared/utils/formatters'
import {
  ArrowBack as BackIcon,
  Cancel as CancelIcon,
  Cancel as CancelledIcon,
  CheckCircle as CompletedIcon,
  Error as FailedIcon,
  HourglassEmpty as InitializingIcon,
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
  Divider,
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
  Tooltip,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'
import {
  JOB_ITEM_STATUS_COLORS,
  JOB_ITEM_STATUS_LABELS,
  JOB_PRIORITY_COLORS,
  JOB_PRIORITY_LABELS,
  JOB_STATUS_COLORS,
  JOB_STATUS_LABELS,
  JOB_TYPE_LABELS,
} from '../constants'
import { useCancelJob, useJob, useJobItems, useRetryJob } from '../hooks'
import type { JobItemFilterParams, JobItemStatus, JobStatus } from '../types'
import { getJobDuration, isJobActive } from '../utils'

const statusIcons: Record<JobStatus, React.ReactElement> = {
  Initializing: <InitializingIcon />,
  Pending: <PendingIcon />,
  Processing: <ProcessingIcon />,
  Completed: <CompletedIcon />,
  CompletedWithErrors: <PartialIcon />,
  Failed: <FailedIcon />,
  Cancelled: <CancelledIcon />,
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

interface MetadataRowProps {
  label: string
  children: React.ReactNode
}

function MetadataRow({ label, children }: Readonly<MetadataRowProps>) {
  return (
    <Box sx={{ display: 'flex', py: 1.5 }}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 160, flexShrink: 0 }}>
        {label}
      </Typography>
      <Box sx={{ flex: 1 }}>{children}</Box>
    </Box>
  )
}

function DetailSkeleton() {
  return (
    <Card sx={{ p: 3 }}>
      <Stack spacing={2}>
        {Array.from({ length: 6 }).map((_, i) => (
          <Box key={`detail-skeleton-${i.toString()}`} sx={{ display: 'flex', gap: 2 }}>
            <Skeleton width={140} height={20} />
            <Skeleton width={200} height={20} />
          </Box>
        ))}
      </Stack>
    </Card>
  )
}

const PAGE_SIZE_OPTIONS = [10, 25, 50]
const DEFAULT_PAGE_SIZE = 10

export function JobDetailPage() {
  const { t } = useTranslation('jobs')
  const { jobId = '' } = useParams<{ jobId: string }>()
  const navigate = useNavigate()

  const [itemStatusFilter, setItemStatusFilter] = useState<JobItemStatus | 'all'>('all')
  const [itemPage, setItemPage] = useState(0)
  const [itemPageSize, setItemPageSize] = useState(DEFAULT_PAGE_SIZE)

  const { data: job, isLoading, error, refetch } = useJob(jobId)
  const cancelMutation = useCancelJob()
  const retryMutation = useRetryJob()

  const itemParams: JobItemFilterParams = useMemo(
    () => ({
      ...(itemStatusFilter !== 'all' && { status: itemStatusFilter }),
      page: itemPage + 1,
      pageSize: itemPageSize,
    }),
    [itemStatusFilter, itemPage, itemPageSize]
  )

  const { data: itemsData, isLoading: itemsLoading } = useJobItems(jobId, itemParams)

  const items = itemsData?.items ?? []
  const totalItems = itemsData?.totalCount ?? 0

  const active = job ? isJobActive(job.status) : false
  const canRetry = job?.status === 'Failed' || job?.status === 'CompletedWithErrors'
  const progressColor = job ? statusPaletteColors[job.status] : palette.neutral[400]

  if (error) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error">{t('jobs.detailLoadFailed')}</Alert>
        <Button startIcon={<BackIcon />} onClick={() => navigate('/admin/jobs')} sx={{ mt: 2 }}>
          {t('jobs.backToJobs')}
        </Button>
      </Container>
    )
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
            <Stack direction="row" alignItems="center" spacing={2}>
              <IconButton onClick={() => navigate('/admin/jobs')}>
                <BackIcon />
              </IconButton>
              {isLoading ? (
                <Skeleton width={300} height={36} />
              ) : (
                <Box>
                  <Stack direction="row" alignItems="center" spacing={1.5}>
                    <Typography variant="h5" fontWeight={700}>
                      {job
                        ? t(`types.${job.jobType}`, {
                            defaultValue: JOB_TYPE_LABELS[job.jobType],
                          })
                        : ''}
                    </Typography>
                    {job && (
                      <Chip
                        icon={statusIcons[job.status]}
                        label={t(`statuses.${job.status}`, {
                          defaultValue: JOB_STATUS_LABELS[job.status],
                        })}
                        color={JOB_STATUS_COLORS[job.status]}
                        size="small"
                      />
                    )}
                  </Stack>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{ fontFamily: 'monospace', mt: 0.25 }}
                  >
                    {jobId}
                  </Typography>
                </Box>
              )}
            </Stack>
            <Stack direction="row" spacing={1}>
              {active && (
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<CancelIcon />}
                  onClick={() => cancelMutation.mutate(jobId)}
                  disabled={cancelMutation.isPending}
                  size="small"
                >
                  {t('jobs.cancel')}
                </Button>
              )}
              {canRetry && (
                <Button
                  variant="outlined"
                  color="primary"
                  startIcon={<RetryIcon />}
                  onClick={() => retryMutation.mutate(jobId)}
                  disabled={retryMutation.isPending}
                  size="small"
                >
                  {t('jobs.retry')}
                </Button>
              )}
              <IconButton onClick={() => refetch()} disabled={isLoading}>
                <RefreshIcon />
              </IconButton>
            </Stack>
          </Stack>
        </motion.div>

        {isLoading ? (
          <DetailSkeleton />
        ) : (
          job && (
            <>
              <motion.div variants={staggerItem}>
                <Grid container spacing={3} sx={{ mb: 3 }}>
                  <Grid size={{ xs: 12, md: 7 }}>
                    <Card sx={{ p: 3, height: '100%' }}>
                      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
                        {t('jobs.details')}
                      </Typography>
                      <Divider sx={{ mb: 1 }} />
                      <MetadataRow label={t('jobs.type')}>
                        <Typography variant="body2">
                          {t(`types.${job.jobType}`, {
                            defaultValue: JOB_TYPE_LABELS[job.jobType],
                          })}
                        </Typography>
                      </MetadataRow>
                      <MetadataRow label={t('jobs.priority')}>
                        <Chip
                          size="small"
                          label={t(`priorities.${job.priority}`, {
                            defaultValue: JOB_PRIORITY_LABELS[job.priority],
                          })}
                          color={JOB_PRIORITY_COLORS[job.priority]}
                          variant="outlined"
                          sx={{ height: 24 }}
                        />
                      </MetadataRow>
                      <MetadataRow label={t('jobs.correlationId')}>
                        <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                          {job.correlationId}
                        </Typography>
                      </MetadataRow>
                      {job.requestedBy && (
                        <MetadataRow label={t('jobs.requestedBy')}>
                          <Typography variant="body2">{job.requestedBy}</Typography>
                        </MetadataRow>
                      )}
                      <MetadataRow label={t('jobs.created')}>
                        <Typography variant="body2">{formatRelativeTime(job.createdAt)}</Typography>
                      </MetadataRow>
                      {job.startedAt && (
                        <MetadataRow label={t('jobs.started')}>
                          <Typography variant="body2">
                            {formatRelativeTime(job.startedAt)}
                          </Typography>
                        </MetadataRow>
                      )}
                      {job.completedAt && (
                        <MetadataRow label={t('jobs.completed')}>
                          <Typography variant="body2">
                            {formatRelativeTime(job.completedAt)}
                          </Typography>
                        </MetadataRow>
                      )}
                      <MetadataRow label={t('jobs.duration')}>
                        <Typography variant="body2">
                          {getJobDuration(job.startedAt, job.completedAt)}
                        </Typography>
                      </MetadataRow>
                      {job.errorMessage && (
                        <MetadataRow label={t('jobs.error')}>
                          <Alert severity="error" variant="outlined" sx={{ py: 0.5, px: 1.5 }}>
                            {job.errorMessage}
                          </Alert>
                        </MetadataRow>
                      )}
                    </Card>
                  </Grid>

                  <Grid size={{ xs: 12, md: 5 }}>
                    <Card sx={{ p: 3, height: '100%' }}>
                      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                        {t('jobs.progress')}
                      </Typography>
                      <Box sx={{ textAlign: 'center', mb: 3 }}>
                        <Typography variant="h2" fontWeight={700} sx={{ color: progressColor }}>
                          {job.progress.toFixed(1)}%
                        </Typography>
                      </Box>
                      <LinearProgress
                        variant="determinate"
                        value={job.progress}
                        sx={{
                          height: 10,
                          borderRadius: 2,
                          mb: 3,
                          bgcolor: `${progressColor}20`,
                          '& .MuiLinearProgress-bar': { bgcolor: progressColor, borderRadius: 2 },
                        }}
                      />
                      <Grid container spacing={2}>
                        <Grid size={4}>
                          <Box sx={{ textAlign: 'center' }}>
                            <Typography variant="h5" fontWeight={700}>
                              {formatNumber(job.totalItems)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {t('jobs.total')}
                            </Typography>
                          </Box>
                        </Grid>
                        <Grid size={4}>
                          <Box sx={{ textAlign: 'center' }}>
                            <Typography variant="h5" fontWeight={700} color="success.main">
                              {formatNumber(job.completedItems)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {t('jobs.completed')}
                            </Typography>
                          </Box>
                        </Grid>
                        <Grid size={4}>
                          <Box sx={{ textAlign: 'center' }}>
                            <Typography variant="h5" fontWeight={700} color="error.main">
                              {formatNumber(job.failedItems)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {t('jobs.failed')}
                            </Typography>
                          </Box>
                        </Grid>
                      </Grid>
                    </Card>
                  </Grid>
                </Grid>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ p: 0 }}>
                  <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
                    <Stack direction="row" alignItems="center" justifyContent="space-between">
                      <Typography variant="h6" fontWeight={600}>
                        {t('jobs.items')}
                      </Typography>
                      <FormControl size="small" sx={{ minWidth: 140 }}>
                        <InputLabel>{t('jobs.status')}</InputLabel>
                        <Select
                          value={itemStatusFilter}
                          label={t('jobs.status')}
                          onChange={(e) => {
                            setItemStatusFilter(e.target.value as JobItemStatus | 'all')
                            setItemPage(0)
                          }}
                        >
                          <MenuItem value="all">{t('jobs.allStatuses')}</MenuItem>
                          {Object.entries(JOB_ITEM_STATUS_LABELS).map(([key, label]) => (
                            <MenuItem key={key} value={key}>
                              {t(`itemStatuses.${key}`, { defaultValue: label })}
                            </MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    </Stack>
                  </Box>

                  <TableContainer sx={{ overflowX: 'auto' }}>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>#</TableCell>
                          <TableCell>{t('jobs.status')}</TableCell>
                          <TableCell>{t('jobs.retries')}</TableCell>
                          <TableCell>{t('jobs.created')}</TableCell>
                          <TableCell>{t('jobs.processed')}</TableCell>
                          <TableCell>{t('jobs.error')}</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {itemsLoading &&
                          Array.from({ length: 5 }).map((_, i) => (
                            <TableRow key={`item-skeleton-${i.toString()}`}>
                              {Array.from({ length: 6 }).map((__, j) => (
                                <TableCell key={`item-skeleton-cell-${j.toString()}`}>
                                  <Skeleton width={j === 5 ? 150 : 60} />
                                </TableCell>
                              ))}
                            </TableRow>
                          ))}
                        {!itemsLoading && items.length === 0 && (
                          <TableRow>
                            <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                {t('jobs.noItems')}
                              </Typography>
                            </TableCell>
                          </TableRow>
                        )}
                        {!itemsLoading &&
                          items.map((item) => (
                            <TableRow key={item.id} hover>
                              <TableCell>
                                <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                                  {item.sequenceNumber}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                <Chip
                                  size="small"
                                  label={t(`itemStatuses.${item.status}`, {
                                    defaultValue: JOB_ITEM_STATUS_LABELS[item.status],
                                  })}
                                  color={JOB_ITEM_STATUS_COLORS[item.status]}
                                  sx={{ height: 22 }}
                                />
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2">
                                  {item.retryCount} / {item.maxRetryCount}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2">
                                  {formatRelativeTime(item.createdAt)}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2">
                                  {item.processedAt ? formatRelativeTime(item.processedAt) : '—'}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                {item.errorMessage ? (
                                  <Tooltip title={item.errorMessage}>
                                    <Typography
                                      variant="caption"
                                      color="error"
                                      sx={{
                                        display: 'block',
                                        maxWidth: 250,
                                        overflow: 'hidden',
                                        textOverflow: 'ellipsis',
                                        whiteSpace: 'nowrap',
                                      }}
                                    >
                                      {item.errorMessage}
                                    </Typography>
                                  </Tooltip>
                                ) : (
                                  <Typography variant="body2" color="text.secondary">
                                    —
                                  </Typography>
                                )}
                              </TableCell>
                            </TableRow>
                          ))}
                      </TableBody>
                    </Table>
                  </TableContainer>

                  <TablePagination
                    component="div"
                    count={totalItems}
                    page={itemPage}
                    onPageChange={(_, p) => setItemPage(p)}
                    rowsPerPage={itemPageSize}
                    onRowsPerPageChange={(e) => {
                      setItemPageSize(Number.parseInt(e.target.value, 10))
                      setItemPage(0)
                    }}
                    rowsPerPageOptions={PAGE_SIZE_OPTIONS}
                  />
                </Card>
              </motion.div>
            </>
          )
        )}
      </motion.div>
    </Container>
  )
}
