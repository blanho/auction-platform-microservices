import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, useCallback } from 'react'
import { storageApi } from '../api'
import type { EntityType, UploadProgress } from '../types'

export const storageKeys = {
  all: ['storage'] as const,
  file: (id: string) => [...storageKeys.all, 'file', id] as const,
  entityFiles: (entityType: string, entityId: string) =>
    [...storageKeys.all, 'entity', entityType, entityId] as const,
  scanStatus: (id: string) => [...storageKeys.all, 'scan', id] as const,
}

export const useFileMetadata = (fileId: string) => {
  return useQuery({
    queryKey: storageKeys.file(fileId),
    queryFn: () => storageApi.getFileMetadata(fileId),
    enabled: !!fileId,
  })
}

export const useEntityFiles = (entityType: string, entityId: string) => {
  return useQuery({
    queryKey: storageKeys.entityFiles(entityType, entityId),
    queryFn: () => storageApi.getFilesByEntity(entityType, entityId),
    enabled: !!entityType && !!entityId,
  })
}

export const useScanStatus = (fileId: string, enabled = true) => {
  return useQuery({
    queryKey: storageKeys.scanStatus(fileId),
    queryFn: () => storageApi.getScanStatus(fileId),
    enabled: enabled && !!fileId,
    refetchInterval: (query) => {
      const status = query.state.data?.status
      if (status === 'pending' || status === 'scanning') {
        return 2000
      }
      return false
    },
  })
}

export const useDeleteFile = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (fileId: string) => storageApi.deleteFile(fileId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: storageKeys.all })
    },
  })
}

export const useGetDownloadUrl = () => {
  return useMutation({
    mutationFn: (fileId: string) => storageApi.getDownloadUrl(fileId),
  })
}

export const useFileUpload = () => {
  const [uploads, setUploads] = useState<Map<string, UploadProgress>>(new Map())

  const uploadFile = useCallback(
    async (
      file: File,
      entityType: EntityType,
      entityId?: string,
      onComplete?: (fileId: string) => void
    ) => {
      const tempId = `temp-${Date.now()}-${file.name}`

      setUploads((prev) => {
        const next = new Map(prev)
        next.set(tempId, {
          fileId: tempId,
          fileName: file.name,
          progress: 0,
          status: 'uploading',
        })
        return next
      })

      try {
        const { fileId, uploadUrl } = await storageApi.requestUpload({
          fileName: file.name,
          contentType: file.type,
          size: file.size,
          entityType,
          entityId,
        })

        setUploads((prev) => {
          const next = new Map(prev)
          const upload = next.get(tempId)
          if (upload) {
            next.delete(tempId)
            next.set(fileId, { ...upload, fileId })
          }
          return next
        })

        await storageApi.uploadToPresignedUrl(uploadUrl, file, (progress) => {
          setUploads((prev) => {
            const next = new Map(prev)
            const upload = next.get(fileId)
            if (upload) {
              next.set(fileId, { ...upload, progress })
            }
            return next
          })
        })

        await storageApi.confirmUpload({ fileId })

        setUploads((prev) => {
          const next = new Map(prev)
          const upload = next.get(fileId)
          if (upload) {
            next.set(fileId, { ...upload, progress: 100, status: 'completed' })
          }
          return next
        })

        onComplete?.(fileId)
        return fileId
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Upload failed'

        setUploads((prev) => {
          const next = new Map(prev)
          const currentId = next.has(tempId) ? tempId : Array.from(next.keys()).pop()
          if (currentId) {
            const upload = next.get(currentId)
            if (upload) {
              next.set(currentId, { ...upload, status: 'failed', error: errorMessage })
            }
          }
          return next
        })

        throw error
      }
    },
    []
  )

  const clearUpload = useCallback((fileId: string) => {
    setUploads((prev) => {
      const next = new Map(prev)
      next.delete(fileId)
      return next
    })
  }, [])

  const clearAllUploads = useCallback(() => {
    setUploads(new Map())
  }, [])

  return {
    uploads: Array.from(uploads.values()),
    uploadFile,
    clearUpload,
    clearAllUploads,
  }
}
