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
  Tooltip,
} from '@mui/material'
import {
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Cancel as CancelIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Schedule as PendingIcon,
  Sync as ProcessingIcon,
  Close as CloseIcon,
} from '@mui/icons-material'
import { useJobs, useCancelJob } from '@/modules/jobs/hooks'
import { JOB_STATUS_LABELS, JOB_STATUS_COLORS } from '@/modules/jobs/constants'
import { isJobActive } from '@/modules/jobs/utils'
import type { JobSummaryDto, JobStatus } from '@/modules/jobs/types'

const statusIcons: Record<JobStatus, React.ReactElement> = {
  Initializing: <PendingIcon fontSize="small" />,
  Pending: <PendingIcon fontSize="small" />,
  Processing: <ProcessingIcon fontSize="small" />,
  Completed: <SuccessIcon fontSize="small" />,
  CompletedWithErrors: <ErrorIcon fontSize="small" />,
  Failed: <ErrorIcon fontSize="small" />,
  Cancelled: <CloseIcon fontSize="small" />,
}

interface JobItemProps {
  job: JobSummaryDto
  onCancel: (id: string) => void
}

function JobItem({ job, onCancel }: Readonly<JobItemProps>) {
  const active = isJobActive(job.status)

  return (
    <ListItem
      secondaryAction={
        active ? (
          <Tooltip title="Cancel">
            <IconButton size="small" onClick={() => onCancel(job.id)}>
              <CancelIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ) : undefined
      }
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
              {JOB_STATUS_LABELS[job.status]}
            </Typography>
            <Chip
              size="small"
              icon={statusIcons[job.status]}
              label={job.status}
              color={JOB_STATUS_COLORS[job.status]}
              sx={{ height: 20 }}
            />
          </Stack>
        }
        secondary={
          <Box sx={{ mt: 1 }}>
            {active && (
              <>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="caption" color="text.secondary">
                    {job.completedItems.toLocaleString()} / {job.totalItems.toLocaleString()}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {job.progress.toFixed(1)}%
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={job.progress}
                  sx={{ height: 4, borderRadius: 1 }}
                />
              </>
            )}
          </Box>
        }
      />
    </ListItem>
  )
}

interface BackgroundJobsPanelProps {
  jobType?: string
  title?: string
}

export function BackgroundJobsPanel({ jobType, title = 'Background Jobs' }: Readonly<BackgroundJobsPanelProps>) {
  const [expanded, setExpanded] = useState(true)
  const { data, isLoading } = useJobs(jobType ? { jobType } : undefined)
  const cancelMutation = useCancelJob()

  const jobs = data?.items ?? []
  const activeJobs = jobs.filter((j) => isJobActive(j.status))
  const recentJobs = jobs.filter((j) => !isJobActive(j.status)).slice(0, 5)

  const handleCancel = (id: string) => {
    cancelMutation.mutate(id)
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
              label={`${activeJobs.length} active`}
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
                  <JobItem key={job.id} job={job} onCancel={handleCancel} />
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
                  <JobItem key={job.id} job={job} onCancel={handleCancel} />
                ))}
              </List>
            </Box>
          )}
        </Box>
      </Collapse>
    </Paper>
  )
}
