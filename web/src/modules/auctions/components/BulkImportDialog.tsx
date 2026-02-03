import { useState, useCallback, useEffect, useRef } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  LinearProgress,
  Alert,
  AlertTitle,
  Chip,
  Stack,
  IconButton,
  Paper,
  List,
  ListItem,
  ListItemText,
  Divider,
} from '@mui/material'
import {
  CloudUpload as UploadIcon,
  Close as CloseIcon,
  Cancel as CancelIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Schedule as PendingIcon,
  Sync as ProcessingIcon,
} from '@mui/icons-material'
import { useDropzone } from 'react-dropzone'
import { auctionsApi } from '../api/auctions.api'
import type { BulkImportProgress, BulkImportStatus } from '../types/import-export.types'

interface BulkImportDialogProps {
  open: boolean
  onClose: () => void
  onComplete?: () => void
}

const POLL_INTERVAL = 1000

const statusConfig: Record<BulkImportStatus, { color: 'default' | 'primary' | 'success' | 'error' | 'warning'; icon: React.ReactElement; label: string }> = {
  Pending: { color: 'default', icon: <PendingIcon />, label: 'Pending' },
  Counting: { color: 'primary', icon: <ProcessingIcon />, label: 'Counting rows...' },
  Processing: { color: 'primary', icon: <ProcessingIcon />, label: 'Processing' },
  Completed: { color: 'success', icon: <SuccessIcon />, label: 'Completed' },
  Failed: { color: 'error', icon: <ErrorIcon />, label: 'Failed' },
  Cancelled: { color: 'warning', icon: <CancelIcon />, label: 'Cancelled' },
}

function formatTimeRemaining(seconds: number): string {
  if (seconds <= 0) return 'Calculating...'
  if (seconds < 60) return `${seconds}s remaining`
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ${seconds % 60}s remaining`
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}h ${minutes}m remaining`
}

function formatNumber(num: number): string {
  return num.toLocaleString()
}

export function BulkImportDialog({ open, onClose, onComplete }: BulkImportDialogProps) {
  const [file, setFile] = useState<File | null>(null)
  const [uploading, setUploading] = useState(false)
  const [jobId, setJobId] = useState<string | null>(null)
  const [progress, setProgress] = useState<BulkImportProgress | null>(null)
  const [error, setError] = useState<string | null>(null)
  const pollIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setFile(acceptedFiles[0])
      setError(null)
    }
  }, [])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
      'text/csv': ['.csv'],
    },
    maxFiles: 1,
    disabled: uploading || !!jobId,
  })

  const stopPolling = useCallback(() => {
    if (pollIntervalRef.current) {
      clearInterval(pollIntervalRef.current)
      pollIntervalRef.current = null
    }
  }, [])

  const pollProgress = useCallback(async (id: string) => {
    try {
      const progressData = await auctionsApi.getBulkImportProgress(id)
      setProgress(progressData)

      if (['Completed', 'Failed', 'Cancelled'].includes(progressData.status)) {
        stopPolling()
        if (progressData.status === 'Completed' && onComplete) {
          onComplete()
        }
      }
    } catch (err) {
      console.error('Failed to poll progress:', err)
    }
  }, [stopPolling, onComplete])

  const startPolling = useCallback((id: string) => {
    stopPolling()
    pollProgress(id)
    pollIntervalRef.current = setInterval(() => pollProgress(id), POLL_INTERVAL)
  }, [stopPolling, pollProgress])

  useEffect(() => {
    return () => {
      stopPolling()
    }
  }, [stopPolling])

  const handleStartImport = async () => {
    if (!file) return

    setUploading(true)
    setError(null)

    try {
      const response = await auctionsApi.startBulkImport(file)
      setJobId(response.jobId)
      startPolling(response.jobId)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start import')
    } finally {
      setUploading(false)
    }
  }

  const handleCancel = async () => {
    if (!jobId) return

    try {
      await auctionsApi.cancelBulkImport(jobId)
      stopPolling()
    } catch (err) {
      console.error('Failed to cancel import:', err)
    }
  }

  const handleClose = () => {
    if (progress?.status === 'Processing') {
      return
    }
    stopPolling()
    setFile(null)
    setJobId(null)
    setProgress(null)
    setError(null)
    onClose()
  }

  const handleReset = () => {
    stopPolling()
    setFile(null)
    setJobId(null)
    setProgress(null)
    setError(null)
  }

  const isProcessing = progress?.status === 'Processing' || progress?.status === 'Counting'
  const isComplete = progress?.status === 'Completed' || progress?.status === 'Failed' || progress?.status === 'Cancelled'

  return (
    <Dialog
      open={open}
      onClose={isProcessing ? undefined : handleClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h6">Bulk Import Auctions</Typography>
        {!isProcessing && (
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        )}
      </DialogTitle>

      <DialogContent dividers>
        {!jobId ? (
          <Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Upload an Excel or CSV file to import multiple auctions at once. 
              Supports up to 1 million records with background processing.
            </Typography>

            <Paper
              {...getRootProps()}
              variant="outlined"
              sx={{
                p: 4,
                textAlign: 'center',
                cursor: uploading ? 'not-allowed' : 'pointer',
                bgcolor: isDragActive ? 'action.hover' : 'background.paper',
                borderStyle: 'dashed',
                borderColor: isDragActive ? 'primary.main' : 'divider',
                transition: 'all 0.2s',
              }}
            >
              <input {...getInputProps()} />
              <UploadIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
              {isDragActive ? (
                <Typography>Drop the file here...</Typography>
              ) : file ? (
                <Box>
                  <Typography variant="subtitle1" fontWeight="medium">
                    {file.name}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {(file.size / 1024 / 1024).toFixed(2)} MB
                  </Typography>
                </Box>
              ) : (
                <Box>
                  <Typography>Drag & drop a file here, or click to select</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Supports .xlsx, .xls, .csv
                  </Typography>
                </Box>
              )}
            </Paper>

            {error && (
              <Alert severity="error" sx={{ mt: 2 }}>
                {error}
              </Alert>
            )}

            <Alert severity="info" sx={{ mt: 2 }}>
              <AlertTitle>Expected Columns</AlertTitle>
              Title, Description, Condition, YearManufactured, ReservePrice, Currency, AuctionEnd
            </Alert>
          </Box>
        ) : (
          <Box>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
              <Chip
                icon={statusConfig[progress?.status || 'Pending'].icon}
                label={statusConfig[progress?.status || 'Pending'].label}
                color={statusConfig[progress?.status || 'Pending'].color}
              />
              {progress?.estimatedSecondsRemaining !== undefined && progress.status === 'Processing' && (
                <Typography variant="body2" color="text.secondary">
                  {formatTimeRemaining(progress.estimatedSecondsRemaining)}
                </Typography>
              )}
            </Stack>

            {progress && (
              <Box>
                <Box sx={{ mb: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2">
                      Progress: {formatNumber(progress.processedRows)} / {formatNumber(progress.totalRows)}
                    </Typography>
                    <Typography variant="body2" fontWeight="medium">
                      {progress.progressPercentage.toFixed(1)}%
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={progress.progressPercentage}
                    sx={{ height: 8, borderRadius: 1 }}
                  />
                </Box>

                <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
                  <Paper variant="outlined" sx={{ flex: 1, p: 2, textAlign: 'center' }}>
                    <Typography variant="h5" color="success.main">
                      {formatNumber(progress.successCount)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Successful
                    </Typography>
                  </Paper>
                  <Paper variant="outlined" sx={{ flex: 1, p: 2, textAlign: 'center' }}>
                    <Typography variant="h5" color="error.main">
                      {formatNumber(progress.failureCount)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Failed
                    </Typography>
                  </Paper>
                </Stack>

                {progress.errorMessage && (
                  <Alert severity="error" sx={{ mb: 2 }}>
                    {progress.errorMessage}
                  </Alert>
                )}

                {progress.recentErrors.length > 0 && (
                  <Box>
                    <Typography variant="subtitle2" sx={{ mb: 1 }}>
                      Recent Errors ({progress.recentErrors.length})
                    </Typography>
                    <Paper variant="outlined" sx={{ maxHeight: 200, overflow: 'auto' }}>
                      <List dense disablePadding>
                        {progress.recentErrors.map((err, index) => (
                          <Box key={index}>
                            {index > 0 && <Divider />}
                            <ListItem>
                              <ListItemText
                                primary={`Row ${err.rowNumber}`}
                                secondary={err.error}
                                primaryTypographyProps={{ variant: 'body2', fontWeight: 'medium' }}
                                secondaryTypographyProps={{ variant: 'body2' }}
                              />
                            </ListItem>
                          </Box>
                        ))}
                      </List>
                    </Paper>
                  </Box>
                )}
              </Box>
            )}
          </Box>
        )}
      </DialogContent>

      <DialogActions>
        {!jobId ? (
          <>
            <Button onClick={handleClose}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleStartImport}
              disabled={!file || uploading}
              startIcon={uploading ? <ProcessingIcon className="spin" /> : <UploadIcon />}
            >
              {uploading ? 'Starting...' : 'Start Import'}
            </Button>
          </>
        ) : isComplete ? (
          <>
            <Button onClick={handleReset}>Import Another</Button>
            <Button variant="contained" onClick={handleClose}>
              Done
            </Button>
          </>
        ) : (
          <Button
            variant="outlined"
            color="warning"
            onClick={handleCancel}
            startIcon={<CancelIcon />}
          >
            Cancel Import
          </Button>
        )}
      </DialogActions>
    </Dialog>
  )
}
