import type { FileAttachment, FileUploadProgress } from '@/shared/types/storage.types'

export interface FileUploadZoneProps {
  attachments: FileAttachment[]
  uploads: FileUploadProgress[]
  isUploading: boolean
  onFilesSelected: (files: File[]) => void
  onRemove: (fileId: string) => void
  onSetPrimary: (fileId: string) => void
  maxFiles?: number
  acceptedTypes?: readonly string[]
  compact?: boolean
}

export interface DropZoneAreaProps {
  isDragOver: boolean
  remainingSlots: number
  maxFiles: number
  maxSizeMb: number
  hasImages: boolean
  hasDocuments: boolean
  onDragOver: (e: React.DragEvent) => void
  onDragLeave: (e: React.DragEvent) => void
  onDrop: (e: React.DragEvent) => void
  onClick: () => void
  compact?: boolean
}

export interface UploadProgressItemProps {
  upload: FileUploadProgress
}

export interface AttachmentItemProps {
  attachment: FileAttachment
  isUploading: boolean
  onRemove: (fileId: string) => void
  onSetPrimary: (fileId: string) => void
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`
  }
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
