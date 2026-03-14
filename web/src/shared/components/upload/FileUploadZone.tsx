import { useState, useCallback, useRef, useMemo } from 'react'
import { Box, Typography, Stack } from '@mui/material'
import { AnimatePresence } from 'framer-motion'
import { palette } from '@/shared/theme/tokens'
import {
  ALL_ACCEPTED_TYPES,
  MAX_FILE_SIZE_BYTES,
  MAX_FILES_PER_UPLOAD,
  ACCEPTED_IMAGE_TYPES,
  ACCEPTED_DOCUMENT_TYPES,
} from '@/shared/constants/storage.constants'
import type { FileUploadZoneProps } from './FileUploadZone.types'
import { DropZoneArea } from './DropZoneArea'
import { UploadProgressItem } from './UploadProgressItem'
import { AttachmentItem } from './AttachmentItem'
import { UploadErrorList } from './UploadErrorList'

export function FileUploadZone({
  attachments,
  uploads,
  isUploading,
  onFilesSelected,
  onRemove,
  onSetPrimary,
  maxFiles = MAX_FILES_PER_UPLOAD,
  acceptedTypes = ALL_ACCEPTED_TYPES,
  compact = false,
}: Readonly<FileUploadZoneProps>) {
  const [isDragOver, setIsDragOver] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const acceptString = useMemo(
    () => Array.from(acceptedTypes).join(','),
    [acceptedTypes]
  )

  const hasImages = useMemo(
    () => acceptedTypes.some((t) => (ACCEPTED_IMAGE_TYPES as readonly string[]).includes(t)),
    [acceptedTypes]
  )

  const hasDocuments = useMemo(
    () => acceptedTypes.some((t) => (ACCEPTED_DOCUMENT_TYPES as readonly string[]).includes(t)),
    [acceptedTypes]
  )

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragOver(true)
  }, [])

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragOver(false)
  }, [])

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault()
      e.stopPropagation()
      setIsDragOver(false)

      const droppedFiles = Array.from(e.dataTransfer.files)
      if (droppedFiles.length > 0) {
        onFilesSelected(droppedFiles)
      }
    },
    [onFilesSelected]
  )

  const handleFileInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const selectedFiles = Array.from(e.target.files ?? [])
      if (selectedFiles.length > 0) {
        onFilesSelected(selectedFiles)
      }
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
    },
    [onFilesSelected]
  )

  const handleZoneClick = useCallback(() => {
    fileInputRef.current?.click()
  }, [])

  const remainingSlots = maxFiles - attachments.length
  const maxSizeMb = MAX_FILE_SIZE_BYTES / (1024 * 1024)
  const activeUploads = uploads.filter((u) => u.status === 'uploading')
  const errorUploads = uploads.filter((u) => u.status === 'error')

  return (
    <Box>
      <input
        ref={fileInputRef}
        type="file"
        multiple
        accept={acceptString}
        onChange={handleFileInputChange}
        aria-hidden="true"
        tabIndex={-1}
        style={{ display: 'none' }}
      />

      <DropZoneArea
        isDragOver={isDragOver}
        remainingSlots={remainingSlots}
        maxFiles={maxFiles}
        maxSizeMb={maxSizeMb}
        hasImages={hasImages}
        hasDocuments={hasDocuments}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={handleZoneClick}
        compact={compact}
      />

      <AnimatePresence>
        {activeUploads.length > 0 && (
          <Stack spacing={1} sx={{ mt: 2 }}>
            {activeUploads.map((upload) => (
              <UploadProgressItem key={upload.id} upload={upload} />
            ))}
          </Stack>
        )}
      </AnimatePresence>

      {attachments.length > 0 && (
        <Stack spacing={1} sx={{ mt: 2 }}>
          <Typography
            variant="subtitle2"
            sx={{ color: palette.neutral[500], fontWeight: 600 }}
          >
            {`Uploaded files (${attachments.length})`}
          </Typography>
          <AnimatePresence mode="popLayout">
            {attachments.map((attachment) => (
              <AttachmentItem
                key={attachment.fileId}
                attachment={attachment}
                isUploading={isUploading}
                onRemove={onRemove}
                onSetPrimary={onSetPrimary}
              />
            ))}
          </AnimatePresence>
        </Stack>
      )}

      <UploadErrorList errors={errorUploads} />
    </Box>
  )
}
