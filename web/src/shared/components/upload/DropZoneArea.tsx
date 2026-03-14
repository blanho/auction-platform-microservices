import { Box, Typography, Chip, Stack, alpha } from '@mui/material'
import { palette } from '@/shared/theme/tokens'
import type { DropZoneAreaProps } from './FileUploadZone.types'
import { getUploadIcon } from './FileIcons'

export function DropZoneArea({
  isDragOver,
  remainingSlots,
  maxFiles,
  maxSizeMb,
  hasImages,
  hasDocuments,
  onDragOver,
  onDragLeave,
  onDrop,
  onClick,
  compact = false,
}: Readonly<DropZoneAreaProps>) {
  const isDisabled = remainingSlots <= 0

  return (
    <Box
      role="button"
      tabIndex={isDisabled ? -1 : 0}
      aria-label={`Upload files. ${remainingSlots} of ${maxFiles} slots remaining`}
      aria-disabled={isDisabled}
      onClick={isDisabled ? undefined : onClick}
      onDragOver={onDragOver}
      onDragLeave={onDragLeave}
      onDrop={onDrop}
      onKeyDown={(e) => {
        if ((e.key === 'Enter' || e.key === ' ') && !isDisabled) {
          e.preventDefault()
          onClick()
        }
      }}
      sx={{
        border: '2px dashed',
        borderColor: isDragOver
          ? palette.brand.primary
          : alpha(palette.neutral[400], 0.4),
        borderRadius: 2,
        p: compact ? 3 : 4,
        textAlign: 'center',
        cursor: isDisabled ? 'not-allowed' : 'pointer',
        bgcolor: isDragOver
          ? alpha(palette.brand.primary, 0.04)
          : 'transparent',
        transition: 'all 0.2s ease-out',
        opacity: isDisabled ? 0.5 : 1,
        outline: 'none',
        '&:hover': isDisabled
          ? {}
          : {
              borderColor: palette.brand.primary,
              bgcolor: alpha(palette.brand.primary, 0.02),
            },
        '&:focus-visible': {
          borderColor: palette.brand.primary,
          boxShadow: `0 0 0 3px ${alpha(palette.brand.primary, 0.2)}`,
        },
      }}
    >
      <Box
        sx={{
          color: isDragOver ? palette.brand.primary : palette.neutral[400],
          fontSize: compact ? 36 : 48,
          mb: 1,
          display: 'flex',
          justifyContent: 'center',
          '& .MuiSvgIcon-root': { fontSize: 'inherit' },
        }}
      >
        {getUploadIcon()}
      </Box>

      <Typography
        variant={compact ? 'subtitle1' : 'h6'}
        sx={{ fontWeight: 600, color: palette.neutral[700] }}
      >
        {isDragOver ? 'Drop files here' : 'Drag & drop files here'}
      </Typography>

      <Typography variant="body2" sx={{ color: palette.neutral[500], mt: 0.5 }}>
        or click to browse
      </Typography>

      <Stack direction="row" spacing={1} justifyContent="center" sx={{ mt: 1.5 }}>
        {hasImages && (
          <Chip label="Images" size="small" variant="outlined" sx={{ borderRadius: 1 }} />
        )}
        {hasDocuments && (
          <Chip label="PDF / CSV / Excel" size="small" variant="outlined" sx={{ borderRadius: 1 }} />
        )}
      </Stack>

      <Typography
        variant="caption"
        sx={{ color: palette.neutral[400], mt: 1, display: 'block' }}
      >
        {`Max ${maxSizeMb}MB per file · ${remainingSlots} of ${maxFiles} slots remaining`}
      </Typography>
    </Box>
  )
}
