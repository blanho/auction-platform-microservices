import { useState, useRef, type DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  CircularProgress,
  Alert,
  Typography,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  LinearProgress,
} from '@mui/material'
import { FileUpload, CheckCircle, Error as ErrorIcon, InsertDriveFile } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { ImportAuctionsResult, ImportAuctionError } from '@/modules/auctions/types/import-export.types'
import { useImportAuctionsFile } from '@/modules/auctions/hooks/useImportExport'

interface ImportAuctionsDialogProps {
  open: boolean
  onClose: () => void
  onSuccess?: () => void
}

export function ImportAuctionsDialog({ open, onClose, onSuccess }: Readonly<ImportAuctionsDialogProps>) {
  const { t } = useTranslation('common')
  const [file, setFile] = useState<File | null>(null)
  const [isDragging, setIsDragging] = useState(false)
  const [result, setResult] = useState<ImportAuctionsResult | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const importMutation = useImportAuctionsFile()

  const handleFileSelect = (selectedFile: File) => {
    const validExtensions = ['.xlsx', '.xls', '.csv']
    const ext = selectedFile.name.toLowerCase().slice(selectedFile.name.lastIndexOf('.'))

    if (!validExtensions.includes(ext)) {
      return
    }

    setFile(selectedFile)
    setResult(null)
  }

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragging(false)

    const droppedFile = e.dataTransfer.files[0]
    if (droppedFile) {
      handleFileSelect(droppedFile)
    }
  }

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragging(true)
  }

  const handleDragLeave = () => {
    setIsDragging(false)
  }

  const handleImport = async () => {
    if (!file) {
      return
    }

    try {
      const importResult = await importMutation.mutateAsync(file)
      setResult(importResult)
      if (importResult.successCount > 0) {
        onSuccess?.()
      }
    } catch {
      setResult(null)
    }
  }

  const handleClose = () => {
    if (!importMutation.isPending) {
      onClose()
      setFile(null)
      setResult(null)
    }
  }

  const handleReset = () => {
    setFile(null)
    setResult(null)
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: 40,
            height: 40,
            borderRadius: 2,
            bgcolor: palette.brand.muted,
            color: palette.brand.primary,
          }}
        >
          <FileUpload />
        </Box>
        {t('import.title')}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ mt: 1 }}>
          {importMutation.isError && (
            <Alert severity="error">
              {t('import.importFailed')}
            </Alert>
          )}

          {!result && (
            <>
              <Box
                onDrop={handleDrop}
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onClick={() => fileInputRef.current?.click()}
                sx={{
                  border: '2px dashed',
                  borderColor: isDragging ? palette.brand.primary : 'divider',
                  borderRadius: 2,
                  p: 4,
                  textAlign: 'center',
                  cursor: 'pointer',
                  bgcolor: isDragging ? palette.brand.muted : 'transparent',
                  transition: 'all 0.2s ease',
                  '&:hover': {
                    borderColor: palette.brand.primary,
                    bgcolor: palette.brand.muted,
                  },
                }}
              >
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".xlsx,.xls,.csv"
                  hidden
                  onChange={(e) => {
                    const selectedFile = e.target.files?.[0]
                    if (selectedFile) {
                      handleFileSelect(selectedFile)
                    }
                  }}
                />
                <FileUpload sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
                <Typography variant="body1" gutterBottom>
                  {t('import.dragDropHere')}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {t('import.supportedFormats')}
                </Typography>
              </Box>

              {file && (
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 2,
                    p: 2,
                    bgcolor: 'grey.50',
                    borderRadius: 1,
                  }}
                >
                  <InsertDriveFile color="primary" />
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="body2" fontWeight={500}>
                      {file.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {(file.size / 1024).toFixed(1)} KB
                    </Typography>
                  </Box>
                  <Button size="small" color="error" onClick={handleReset}>
                    {t('actions.remove')}
                  </Button>
                </Box>
              )}

              <Alert severity="info">
                <Typography variant="body2" fontWeight={500} gutterBottom>
                  {t('import.requiredColumns')}
                </Typography>
                <Typography variant="caption" component="div">
                  Title, Description, ReservePrice, AuctionEnd
                </Typography>
                <Typography variant="body2" fontWeight={500} sx={{ mt: 1 }} gutterBottom>
                  {t('import.optionalColumns')}
                </Typography>
                <Typography variant="caption" component="div">
                  Condition, YearManufactured, Currency
                </Typography>
              </Alert>
            </>
          )}

          {result && (
            <>
              <Box sx={{ mb: 2 }}>
                <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
                  <Chip
                    icon={<CheckCircle />}
                    label={t('import.successful_other', { count: result.successCount })}
                    color="success"
                    variant="outlined"
                  />
                  {result.failureCount > 0 && (
                    <Chip
                      icon={<ErrorIcon />}
                      label={t('import.failed_other', { count: result.failureCount })}
                      color="error"
                      variant="outlined"
                    />
                  )}
                </Stack>

                <LinearProgress
                  variant="determinate"
                  value={(result.successCount / result.totalRows) * 100}
                  sx={{
                    height: 8,
                    borderRadius: 4,
                    bgcolor: 'grey.200',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: result.failureCount > 0 ? palette.semantic.warning : palette.semantic.success,
                    },
                  }}
                />
                <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                  {t('import.importedSuccessfully', { success: result.successCount, total: result.totalRows })}
                </Typography>
              </Box>

              {result.errors.length > 0 && (
                <TableContainer sx={{ maxHeight: 300 }}>
                  <Table size="small" stickyHeader>
                    <TableHead>
                      <TableRow>
                        <TableCell>{t('import.row')}</TableCell>
                        <TableCell>{t('import.field')}</TableCell>
                        <TableCell>{t('import.details')}</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {result.errors.map((item: ImportAuctionError) => (
                        <TableRow key={`${item.row}-${item.field}-${item.message}`}>
                          <TableCell>{item.row}</TableCell>
                          <TableCell>{item.field}</TableCell>
                          <TableCell>
                            <Typography variant="caption" color="error">
                              {item.message}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </>
          )}

          {importMutation.isPending && (
            <Box sx={{ textAlign: 'center', py: 2 }}>
              <CircularProgress size={32} />
              <Typography variant="body2" sx={{ mt: 1 }}>
                {t('import.importing')}
              </Typography>
            </Box>
          )}
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={handleClose} disabled={importMutation.isPending}>
          {result ? t('close') : t('cancel')}
        </Button>
        {!result && (
          <Button
            variant="contained"
            onClick={handleImport}
            disabled={!file || importMutation.isPending}
            startIcon={importMutation.isPending ? <CircularProgress size={16} /> : <FileUpload />}
            sx={{
              bgcolor: palette.brand.primary,
              '&:hover': { bgcolor: palette.brand.secondary },
            }}
          >
            {importMutation.isPending ? t('loading') : t('import.importButton')}
          </Button>
        )}
        {result && (
          <Button
            variant="outlined"
            onClick={handleReset}
            sx={{ borderColor: palette.brand.primary, color: palette.brand.primary }}
          >
            {t('import.importAnother')}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  )
}
