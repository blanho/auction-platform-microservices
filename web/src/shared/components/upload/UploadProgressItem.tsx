import { Box, Typography, LinearProgress, alpha } from '@mui/material'
import { motion } from 'framer-motion'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp } from '@/shared/lib/animations'
import type { UploadProgressItemProps } from './FileUploadZone.types'
import { formatFileSize } from './FileUploadZone.types'
import { getFileIcon } from './FileIcons'

export function UploadProgressItem({ upload }: Readonly<UploadProgressItemProps>) {
  return (
    <motion.div variants={fadeInUp} initial="initial" animate="animate" exit="exit">
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          p: 1.5,
          borderRadius: 1.5,
          bgcolor: alpha(palette.brand.primary, 0.04),
          border: '1px solid',
          borderColor: alpha(palette.brand.primary, 0.12),
        }}
      >
        <Box sx={{ color: palette.brand.primary, display: 'flex' }}>
          {getFileIcon(upload.file.type)}
        </Box>

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
            {upload.file.name}
          </Typography>
          <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
            {formatFileSize(upload.file.size)}
          </Typography>
          <LinearProgress
            variant="determinate"
            value={upload.progress}
            aria-label={`Uploading ${upload.file.name}: ${upload.progress}%`}
            sx={{
              mt: 0.5,
              borderRadius: 1,
              height: 4,
              bgcolor: alpha(palette.brand.primary, 0.1),
              '& .MuiLinearProgress-bar': {
                bgcolor: palette.brand.primary,
                borderRadius: 1,
              },
            }}
          />
        </Box>

        <Typography
          variant="caption"
          sx={{ color: palette.brand.primary, fontWeight: 600, minWidth: 36, textAlign: 'right' }}
        >
          {`${upload.progress}%`}
        </Typography>
      </Box>
    </motion.div>
  )
}
