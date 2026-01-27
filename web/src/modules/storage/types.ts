export interface FileMetadata {
  id: string
  fileName: string
  originalFileName: string
  contentType: string
  size: number
  url: string
  thumbnailUrl?: string
  uploadedBy: string
  entityType: EntityType
  entityId?: string
  status: FileStatus
  scanStatus: ScanStatus
  createdAt: string
  updatedAt: string
}

export type EntityType = 'auction' | 'user' | 'category' | 'message' | 'other'

export type FileStatus = 'pending' | 'processing' | 'ready' | 'failed' | 'deleted'

export type ScanStatus = 'pending' | 'scanning' | 'clean' | 'infected' | 'error'

export interface RequestUploadRequest {
  fileName: string
  contentType: string
  size: number
  entityType: EntityType
  entityId?: string
}

export interface RequestUploadResponse {
  fileId: string
  uploadUrl: string
  expiresAt: string
}

export interface ConfirmUploadRequest {
  fileId: string
}

export interface SubmitFileRequest {
  fileId: string
  entityType: EntityType
  entityId: string
}

export interface DownloadUrlResponse {
  url: string
  expiresAt: string
}

export interface UploadProgress {
  fileId: string
  fileName: string
  progress: number
  status: 'uploading' | 'completed' | 'failed'
  error?: string
}
