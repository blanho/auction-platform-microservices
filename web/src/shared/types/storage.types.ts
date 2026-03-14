export interface StoredFileDto {
  fileId: string
  fileName: string
  contentType: string
  fileSize: number
  url: string
  uploadedAt: string
}

export interface BatchUploadResponse {
  files: StoredFileDto[]
}

export interface FileUrlDto {
  fileId: string
  url: string
}

export type FileUploadStatus = 'idle' | 'uploading' | 'success' | 'error'

export interface FileUploadProgress {
  id: string
  file: File
  fileId?: string
  url?: string
  progress: number
  status: FileUploadStatus
  error?: string
}

export interface FileAttachment {
  fileId: string
  fileType: string
  displayOrder: number
  isPrimary: boolean
  fileName?: string
  url?: string
  previewUrl?: string
}

export interface FileValidationConfig {
  maxFileSize: number
  maxFiles: number
  acceptedTypes: readonly string[]
}

export interface FileValidationError {
  file: File
  reason: string
}

export interface PresignedUploadRequest {
  fileName: string
  contentType: string
  fileSize: number
  subFolder?: string
  ownerId?: string
}

export interface PresignedUploadDto {
  fileId: string
  uploadUrl: string
  headers: Record<string, string>
  expiresAt: string
}

export interface PresignedDownloadDto {
  fileId: string
  downloadUrl: string
  expiresAt: string
}
