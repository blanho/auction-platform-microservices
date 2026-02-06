import { useState } from 'react'
import {
  Box,
  Paper,
  Typography,
  LinearProgress,
  IconButton,
  Chip,
  Stack,
  Collapse,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Tooltip,
} from '@mui/material'
import {
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Cancel as CancelIcon,
  Download as DownloadIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Schedule as PendingIcon,
  Sync as RunningIcon,
  Close as CloseIcon,
} from '@mui/icons-material'
import { useBackgroundJobs, useCancelJob, useDownloadJobResult } from '@/shared/hooks/useBackgroundJobs'
import type { BackgroundJobProgress, BackgroundJobStatus } from '@/shared/types/background-jobs.types'

const statusConfig: Record<BackgroundJobStatus, { color: 'default' | 'primary' | 'success' | 'error' | 'warning'; icon: React.ReactNode }> = {
  Pending: { color: 'default', icon: <PendingIcon fontSize="small" /> },
  Running: { color: 'primary', icon: <RunningIcon fontSize="small" /> },
  Completed: { color: 'success', icon: <SuccessIcon fontSize="small" /> },
  Failed: { color: 'error', icon: <ErrorIcon fontSize="small" /> },
  Cancelled: { color: 'warning', icon: <CloseIcon fontSize="small" /> },
}

function formatTimeRemaining(seconds: number): string {
  if (seconds <= 0) {return 'Almost done...'}
  if (seconds < 60) {return `${seconds}s`}
  if (seconds < 3600) {return `${Math.floor(seconds / 60)}m ${seconds % 60}s`}
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}h ${minutes}m`
}

function formatFileSize(bytes?: number): string {
  if (!bytes) {return ''}
  if (bytes < 1024) {return `${bytes} B`}
  if (bytes < 1024 * 1024) {return `${(bytes / 1024).toFixed(1)} KB`}
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

interface JobItemProps {
  job: BackgroundJobProgress
  onCancel: (jobId: string) => void
  onDownload: (jobId: string, filename: string) => void
}

function JobItem({ job, onCancel, onDownload }: JobItemProps) {
  const isRunning = job.status === 'Running' || job.status === 'Pending'
  const canDownload = job.status === 'Completed' && job.resultFileName

  return (
    <ListItem
      sx={{
        bgcolor: 'background.paper',
        borderRadius: 1,
        mb: 1,
        border: '1px solid',
        borderColor: 'divider',
      }}
    >
      <ListItemText
        primary={
          <Stack direction="row" alignItems="center" spacing={1}>
            <Typography variant="body2" fontWeight="medium">
              {job.name}
            </Typography>
            <Chip
              size="small"
              icon={statusConfig[job.status].icon}
              label={job.status}
              color={statusConfig[job.status].color}
              sx={{ height: 20 }}
            />
          </Stack>
        }
        secondary={
          <Box sx={{ mt: 1 }}>
            {isRunning && (
              <>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="caption" color="text.secondary">
                    {job.processedItems.toLocaleString()} / {job.totalItems.toLocaleString()}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {job.progressPercentage.toFixed(1)}%
                    {job.estimatedSecondsRemaining > 0 && ` • ${formatTimeRemaining(job.estimatedSecondsRemaining)}`}
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={job.progressPercentage}
                  sx={{ height: 4, borderRadius: 1 }}
                />
              </>
            )}
            {job.status === 'Completed' && job.resultFileName && (
              <Typography variant="caption" color="text.secondary">
                {job.resultFileName} ({formatFileSize(job.resultFileSizeBytes)})
              </Typography>
            )}
            {job.status === 'Failed' && job.errorMessage && (
              <Typography variant="caption" color="error">
                {job.errorMessage}
              </Typography>
            )}
          </Box>
        }
      />
      <ListItemSecondaryAction>
        {isRunning && (
          <Tooltip title="Cancel">
            <IconButton size="small" onClick={() => onCancel(job.jobId)}>
              <CancelIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        )}
        {canDownload && job.resultFileName && (
          <Tooltip title="Download">
            <IconButton
              size="small"
              color="primary"
              onClick={() => onDownload(job.jobId, job.resultFileName ?? '')}
            >
              <DownloadIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        )}
      </ListItemSecondaryAction>
    </ListItem>
  )
}

interface BackgroundJobsPanelProps {
  type?: string
  title?: string
}

export function BackgroundJobsPanel({ type, title = 'Background Jobs' }: BackgroundJobsPanelProps) {
  const [expanded, setExpanded] = useState(true)
  const { data: jobs = [], isLoading } = useBackgroundJobs(type)
  const cancelMutation = useCancelJob()
  const downloadMutation = useDownloadJobResult()

  const activeJobs = jobs.filter((j) => j.status === 'Running' || j.status === 'Pending')
  const recentJobs = jobs.filter((j) => j.status !== 'Running' && j.status !== 'Pending').slice(0, 5)

  const handleCancel = (jobId: string) => {
    cancelMutation.mutate(jobId)
  }

  const handleDownload = (jobId: string, filename: string) => {
    downloadMutation.mutate({ jobId, filename })
  }

  if (isLoading || jobs.length === 0) {
    return null
  }

  return (
    <Paper variant="outlined" sx={{ mb: 3 }}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          px: 2,
          py: 1.5,
          cursor: 'pointer',
          '&:hover': { bgcolor: 'action.hover' },
        }}
        onClick={() => setExpanded(!expanded)}
      >
        <Stack direction="row" alignItems="center" spacing={1}>
          <Typography variant="subtitle1" fontWeight="medium">
            {title}
          </Typography>
          {activeJobs.length > 0 && (
            <Chip
              size="small"
              label={`${activeJobs.length} running`}
              color="primary"
              sx={{ height: 20 }}
            />
          )}
        </Stack>
        <IconButton size="small">
          {expanded ? <CollapseIcon /> : <ExpandIcon />}
        </IconButton>
      </Box>

      <Collapse in={expanded}>
        <Box sx={{ px: 2, pb: 2 }}>
          {activeJobs.length > 0 && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
                Active
              </Typography>
              <List disablePadding>
                {activeJobs.map((job) => (
                  <JobItem
                    key={job.jobId}
                    job={job}
                    onCancel={handleCancel}
                    onDownload={handleDownload}
                  />
                ))}
              </List>
            </Box>
          )}

          {recentJobs.length > 0 && (
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
                Recent
              </Typography>
              <List disablePadding>
                {recentJobs.map((job) => (
                  <JobItem
                    key={job.jobId}
                    job={job}
                    onCancel={handleCancel}
                    onDownload={handleDownload}
                  />
                ))}
              </List>
            </Box>
          )}
        </Box>
      </Collapse>
    </Paper>
  )
}
