import { useState, useCallback, useRef, useEffect, useMemo } from 'react'
import { useMutation } from '@tanstack/react-query'
import { storageApi } from '@/services/storage'
import type {
  FileUploadProgress,
  FileAttachment,
  StoredFileDto,
  FileValidationConfig,
  FileValidationError,
} from '@/shared/types/storage.types'
import {
  MAX_FILE_SIZE_BYTES,
  MAX_FILES_PER_UPLOAD,
  ALL_ACCEPTED_TYPES,
} from '@/shared/constants/storage.constants'

interface UseFileUploadOptions {
  maxFileSize?: number
  maxFiles?: number
  acceptedTypes?: readonly string[]
  subFolder?: string
  onUploadComplete?: (attachment: FileAttachment) => void
  onUploadError?: (file: File, error: string) => void
}

interface UseFileUploadReturn {
  uploads: FileUploadProgress[]
  attachments: FileAttachment[]
  uploadFiles: (files: File[]) => Promise<FileValidationError[]>
  removeAttachment: (fileId: string) => void
  setPrimaryAttachment: (fileId: string) => void
  reorderAttachments: (fromIndex: number, toIndex: number) => void
  clearUploads: () => void
  clearAll: () => void
  isUploading: boolean
  hasErrors: boolean
  totalProgress: number
  remainingSlots: number
}

function generateUploadId(): string {
  if (globalThis.crypto?.randomUUID) {
    return globalThis.crypto.randomUUID()
  }
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`
}

function validateFile(file: File, config: FileValidationConfig): string | null {
  if (file.size === 0) {
    return 'File is empty'
  }

  if (file.size > config.maxFileSize) {
    const maxMb = config.maxFileSize / (1024 * 1024)
    return `File size exceeds ${maxMb}MB limit`
  }

  if (!config.acceptedTypes.includes(file.type)) {
    return `File type "${file.type || 'unknown'}" is not supported`
  }

  return null
}

function buildAttachment(
  file: File,
  result: StoredFileDto,
  displayOrder: number
): FileAttachment {
  const isImage = file.type.startsWith('image/')
  return {
    fileId: result.fileId,
    fileType: isImage ? 'image' : 'document',
    displayOrder,
    isPrimary: displayOrder === 0,
    fileName: file.name,
    url: result.url,
    previewUrl: isImage ? URL.createObjectURL(file) : undefined,
  }
}

export function useFileUpload(options: UseFileUploadOptions = {}): UseFileUploadReturn {
  const {
    maxFileSize = MAX_FILE_SIZE_BYTES,
    maxFiles = MAX_FILES_PER_UPLOAD,
    acceptedTypes = ALL_ACCEPTED_TYPES,
    subFolder,
    onUploadComplete,
    onUploadError,
  } = options

  const validationConfig = useMemo<FileValidationConfig>(() => ({
    maxFileSize,
    maxFiles,
    acceptedTypes,
  }), [maxFileSize, maxFiles, acceptedTypes])

  const [uploads, setUploads] = useState<FileUploadProgress[]>([])
  const [attachments, setAttachments] = useState<FileAttachment[]>([])
  const previewUrlsRef = useRef<Set<string>>(new Set())

  useEffect(() => {
    const urls = previewUrlsRef.current
    return () => {
      urls.forEach((url) => URL.revokeObjectURL(url))
    }
  }, [])

  const trackPreviewUrl = useCallback((url: string) => {
    previewUrlsRef.current.add(url)
  }, [])

  const revokePreviewUrl = useCallback((url: string) => {
    URL.revokeObjectURL(url)
    previewUrlsRef.current.delete(url)
  }, [])

  const uploadMutation = useMutation({
    mutationFn: ({
      file,
      onProgress,
    }: {
      file: File
      onProgress: (progress: number) => void
    }) => storageApi.uploadFile(file, { subFolder, onProgress }),
  })

  const updateUploadState = useCallback(
    (uploadId: string, updates: Partial<FileUploadProgress>) => {
      setUploads((prev) =>
        prev.map((u) => (u.id === uploadId ? { ...u, ...updates } : u))
      )
    },
    []
  )

  const uploadFiles = useCallback(
    async (files: File[]): Promise<FileValidationError[]> => {
      const errors: FileValidationError[] = []
      const validFiles: File[] = []

      if (files.length === 0) {
        return errors
      }

      const currentCount = attachments.length
      if (currentCount + files.length > validationConfig.maxFiles) {
        return [{
          file: files[0],
          reason: `Maximum ${validationConfig.maxFiles} files allowed. ${validationConfig.maxFiles - currentCount} slots remaining.`,
        }]
      }

      for (const file of files) {
        const error = validateFile(file, validationConfig)
        if (error) {
          errors.push({ file, reason: error })
        } else {
          validFiles.push(file)
        }
      }

      if (validFiles.length === 0) {
        return errors
      }

      const newUploads: FileUploadProgress[] = validFiles.map((file) => ({
        id: generateUploadId(),
        file,
        progress: 0,
        status: 'idle' as const,
      }))

      setUploads((prev) => [...prev, ...newUploads])

      for (const upload of newUploads) {
        updateUploadState(upload.id, { status: 'uploading' })

        try {
          const result = await uploadMutation.mutateAsync({
            file: upload.file,
            onProgress: (progress) => {
              updateUploadState(upload.id, { progress })
            },
          })

          updateUploadState(upload.id, {
            status: 'success',
            progress: 100,
            fileId: result.fileId,
            url: result.url,
          })

          const attachment = buildAttachment(
            upload.file,
            result,
            attachments.length + validFiles.indexOf(upload.file)
          )

          if (attachment.previewUrl) {
            trackPreviewUrl(attachment.previewUrl)
          }

          setAttachments((prev) => {
            const updated = [...prev, attachment]
            return updated.map((a, idx) => ({
              ...a,
              displayOrder: idx,
              isPrimary: idx === 0 && !prev.some((p) => p.isPrimary),
            }))
          })

          onUploadComplete?.(attachment)
        } catch (err) {
          const errorMessage = err instanceof Error ? err.message : 'Upload failed'
          updateUploadState(upload.id, { status: 'error', error: errorMessage })
          errors.push({ file: upload.file, reason: errorMessage })
          onUploadError?.(upload.file, errorMessage)
        }
      }

      return errors
    },
    [attachments.length, validationConfig, uploadMutation, updateUploadState, trackPreviewUrl, onUploadComplete, onUploadError]
  )

  const removeAttachment = useCallback((fileId: string) => {
    setAttachments((prev) => {
      const removed = prev.find((a) => a.fileId === fileId)
      if (removed?.previewUrl) {
        revokePreviewUrl(removed.previewUrl)
      }

      const filtered = prev.filter((a) => a.fileId !== fileId)
      const hadPrimary = removed?.isPrimary
      return filtered.map((a, idx) => {
        let isPrimary = a.isPrimary
        if (hadPrimary) {
          isPrimary = idx === 0
        }
        return { ...a, displayOrder: idx, isPrimary }
      })
    })
    setUploads((prev) => prev.filter((u) => u.fileId !== fileId))
  }, [revokePreviewUrl])

  const setPrimaryAttachment = useCallback((fileId: string) => {
    setAttachments((prev) =>
      prev.map((a) => ({ ...a, isPrimary: a.fileId === fileId }))
    )
  }, [])

  const reorderAttachments = useCallback((fromIndex: number, toIndex: number) => {
    setAttachments((prev) => {
      const updated = [...prev]
      const [moved] = updated.splice(fromIndex, 1)
      updated.splice(toIndex, 0, moved)
      return updated.map((a, idx) => ({ ...a, displayOrder: idx }))
    })
  }, [])

  const clearUploads = useCallback(() => {
    setUploads([])
  }, [])

  const clearAll = useCallback(() => {
    attachments.forEach((a) => {
      if (a.previewUrl) {
        revokePreviewUrl(a.previewUrl)
      }
    })
    setAttachments([])
    setUploads([])
  }, [attachments, revokePreviewUrl])

  const isUploading = uploads.some((u) => u.status === 'uploading')
  const hasErrors = uploads.some((u) => u.status === 'error')
  const remainingSlots = maxFiles - attachments.length
  const totalProgress = uploads.length > 0
    ? Math.round(uploads.reduce((sum, u) => sum + u.progress, 0) / uploads.length)
    : 0

  return {
    uploads,
    attachments,
    uploadFiles,
    removeAttachment,
    setPrimaryAttachment,
    reorderAttachments,
    clearUploads,
    clearAll,
    isUploading,
    hasErrors,
    totalProgress,
    remainingSlots,
  }
}

export function useDeleteFile() {
  return useMutation({
    mutationFn: (fileId: string) => storageApi.deleteFile(fileId),
  })
}
