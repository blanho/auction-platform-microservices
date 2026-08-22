import { scaleIn } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import {
  CheckCircle as CheckIcon,
  Close as CloseIcon,
  StarBorder as StarBorderIcon,
  Star as StarIcon,
} from '@mui/icons-material'
import { alpha, Box, Chip, IconButton, Stack, Tooltip, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { useTranslation } from 'react-i18next'
import { getFileIcon } from './FileIcons'
import type { AttachmentItemProps } from './FileUploadZone.types'

export function AttachmentItem({
  attachment,
  isUploading,
  onRemove,
  onSetPrimary,
}: Readonly<AttachmentItemProps>) {
  const { t } = useTranslation('common')
  const isImage = attachment.fileType === 'image'

  return (
    <motion.div layout variants={scaleIn} initial="initial" animate="animate" exit="exit">
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1.5,
          p: 1.5,
          borderRadius: 1.5,
          border: '1px solid',
          borderColor: attachment.isPrimary
            ? alpha(palette.brand.primary, 0.4)
            : palette.neutral[200],
          bgcolor: attachment.isPrimary ? alpha(palette.brand.primary, 0.03) : 'transparent',
          transition: 'all 0.2s ease-out',
          '&:hover': {
            borderColor: attachment.isPrimary ? palette.brand.primary : palette.neutral[300],
            bgcolor: attachment.isPrimary
              ? alpha(palette.brand.primary, 0.05)
              : alpha(palette.neutral[100], 0.5),
          },
        }}
      >
        {attachment.previewUrl ? (
          <Box
            component="img"
            src={attachment.previewUrl}
            alt={attachment.fileName ?? t('upload.uploadedFile')}
            loading="lazy"
            sx={{
              width: 48,
              height: 48,
              borderRadius: 1,
              objectFit: 'cover',
              flexShrink: 0,
            }}
          />
        ) : (
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 1,
              bgcolor: alpha(palette.neutral[400], 0.08),
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              flexShrink: 0,
              color: palette.neutral[500],
            }}
          >
            {getFileIcon(isImage ? 'image/jpeg' : 'application/pdf')}
          </Box>
        )}

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>
            {attachment.fileName ?? attachment.fileId}
          </Typography>
          <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mt: 0.5 }}>
            {attachment.isPrimary && (
              <Chip
                icon={<StarIcon sx={{ fontSize: 14 }} />}
                label={t('upload.cover')}
                size="small"
                sx={{
                  height: 22,
                  bgcolor: alpha(palette.brand.primary, 0.1),
                  color: palette.brand.hover,
                  fontWeight: 600,
                  fontSize: '0.7rem',
                  '& .MuiChip-icon': { color: palette.brand.primary },
                }}
              />
            )}
            <Chip
              label={t(isImage ? 'upload.image' : 'upload.document')}
              size="small"
              variant="outlined"
              sx={{ height: 22, fontSize: '0.7rem' }}
            />
            <CheckIcon sx={{ fontSize: 16, color: palette.semantic.success }} />
          </Stack>
        </Box>

        <Tooltip title={t(attachment.isPrimary ? 'upload.coverPhoto' : 'upload.setCoverPhoto')}>
          <IconButton
            size="small"
            onClick={() => onSetPrimary(attachment.fileId)}
            aria-label={
              attachment.isPrimary
                ? t('upload.currentCoverPhoto')
                : t('upload.setFileAsCover', {
                    fileName: attachment.fileName ?? t('upload.file'),
                  })
            }
            sx={{
              color: attachment.isPrimary ? palette.brand.primary : palette.neutral[400],
              transition: 'color 0.15s ease-out',
              '&:hover': { color: palette.brand.primary },
            }}
          >
            {attachment.isPrimary ? (
              <StarIcon fontSize="small" />
            ) : (
              <StarBorderIcon fontSize="small" />
            )}
          </IconButton>
        </Tooltip>

        <Tooltip title={t('upload.removeFile')}>
          <span>
            <IconButton
              size="small"
              onClick={() => onRemove(attachment.fileId)}
              disabled={isUploading}
              aria-label={t('upload.removeNamedFile', {
                fileName: attachment.fileName ?? t('upload.file'),
              })}
              sx={{
                color: palette.neutral[400],
                transition: 'color 0.15s ease-out',
                '&:hover': { color: palette.semantic.error },
              }}
            >
              <CloseIcon fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
      </Box>
    </motion.div>
  )
}
