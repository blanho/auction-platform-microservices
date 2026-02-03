import { useState } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Stack,
  Box,
  CircularProgress,
  Alert,
  Typography,
} from '@mui/material'
import { FileDownload } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { ExportFilters } from '../types/import-export.types'
import { useExportAuctions } from '../hooks/useImportExport'

interface ExportAuctionsDialogProps {
  open: boolean
  onClose: () => void
}

const statusOptions = [
  { value: '', label: 'All Statuses' },
  { value: 'Live', label: 'Live' },
  { value: 'Finished', label: 'Finished' },
  { value: 'Sold', label: 'Sold' },
  { value: 'ReserveNotMet', label: 'Reserve Not Met' },
  { value: 'Cancelled', label: 'Cancelled' },
]

const formatOptions = [
  { value: 'excel', label: 'Excel (.xlsx)' },
  { value: 'csv', label: 'CSV (.csv)' },
  { value: 'json', label: 'JSON' },
]

export function ExportAuctionsDialog({ open, onClose }: ExportAuctionsDialogProps) {
  const [filters, setFilters] = useState<ExportFilters>({
    status: '',
    seller: '',
    startDate: '',
    endDate: '',
    format: 'excel',
  })
  const [success, setSuccess] = useState(false)

  const exportMutation = useExportAuctions()

  const handleExport = async () => {
    setSuccess(false)
    try {
      await exportMutation.mutateAsync(filters)
      setSuccess(true)
      setTimeout(() => {
        onClose()
        setSuccess(false)
      }, 1500)
    } catch {
      // Error handled by mutation
    }
  }

  const handleClose = () => {
    if (!exportMutation.isPending) {
      onClose()
      setSuccess(false)
      setFilters({
        status: '',
        seller: '',
        startDate: '',
        endDate: '',
        format: 'excel',
      })
    }
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
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
          <FileDownload />
        </Box>
        Export Auctions
      </DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ mt: 1 }}>
          {exportMutation.isError && (
            <Alert severity="error">
              Export failed. Please try again.
            </Alert>
          )}

          {success && (
            <Alert severity="success">
              Export completed successfully!
            </Alert>
          )}

          <FormControl fullWidth size="small">
            <InputLabel>Format</InputLabel>
            <Select
              value={filters.format}
              label="Format"
              onChange={(e) => setFilters((prev) => ({ ...prev, format: e.target.value as ExportFilters['format'] }))}
            >
              {formatOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl fullWidth size="small">
            <InputLabel>Status</InputLabel>
            <Select
              value={filters.status}
              label="Status"
              onChange={(e) => setFilters((prev) => ({ ...prev, status: e.target.value }))}
            >
              {statusOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <TextField
            label="Seller Username"
            size="small"
            fullWidth
            value={filters.seller}
            onChange={(e) => setFilters((prev) => ({ ...prev, seller: e.target.value }))}
            placeholder="Filter by seller (optional)"
          />

          <Stack direction="row" spacing={2}>
            <TextField
              label="Start Date"
              type="date"
              size="small"
              fullWidth
              value={filters.startDate}
              onChange={(e) => setFilters((prev) => ({ ...prev, startDate: e.target.value }))}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="End Date"
              type="date"
              size="small"
              fullWidth
              value={filters.endDate}
              onChange={(e) => setFilters((prev) => ({ ...prev, endDate: e.target.value }))}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Stack>

          <Typography variant="caption" color="text.secondary">
            Leave filters empty to export all auctions.
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={handleClose} disabled={exportMutation.isPending}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleExport}
          disabled={exportMutation.isPending}
          startIcon={exportMutation.isPending ? <CircularProgress size={16} /> : <FileDownload />}
          sx={{
            bgcolor: palette.brand.primary,
            '&:hover': { bgcolor: palette.brand.secondary },
          }}
        >
          {exportMutation.isPending ? 'Exporting...' : 'Export'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
