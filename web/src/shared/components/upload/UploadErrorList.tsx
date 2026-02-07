import { Box, Typography, alpha } from '@mui/material'
import { motion, AnimatePresence } from 'framer-motion'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp } from '@/shared/lib/animations'
import type { FileUploadProgress } from '@/shared/types/storage.types'
import { ErrorOutline as ErrorIcon } from '@mui/icons-material'

interface UploadErrorListProps {
  errors: FileUploadProgress[]
}

export function UploadErrorList({ errors }: Readonly<UploadErrorListProps>) {
  if (errors.length === 0) {
    return null
  }

  return (
    <AnimatePresence>
      <Box role="alert" aria-live="assertive" sx={{ mt: 2 }}>
        {errors.map((upload) => (
          <motion.div
            key={upload.id}
            variants={fadeInUp}
            initial="initial"
            animate="animate"
            exit="exit"
          >
            <Box
              sx={{
                display: 'flex',
                alignItems: 'flex-start',
                gap: 1.5,
                p: 1.5,
                mb: 1,
                borderRadius: 1.5,
                border: '1px solid',
                borderColor: alpha(palette.semantic.error, 0.3),
                bgcolor: alpha(palette.semantic.error, 0.04),
              }}
            >
              <ErrorIcon
                sx={{ fontSize: 20, color: palette.semantic.error, mt: 0.25, flexShrink: 0 }}
              />
              <Box sx={{ minWidth: 0 }}>
                <Typography variant="body2" noWrap sx={{ fontWeight: 600 }}>
                  {upload.file.name}
                </Typography>
                <Typography
                  variant="caption"
                  sx={{ color: palette.semantic.error }}
                >
                  {upload.error ?? 'Upload failed'}
                </Typography>
              </Box>
            </Box>
          </motion.div>
        ))}
      </Box>
    </AnimatePresence>
  )
}
