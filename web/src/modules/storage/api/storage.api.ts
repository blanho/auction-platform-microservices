import { http } from '@/services/http'
import axios from 'axios'
import type {
  FileMetadata,
  RequestUploadRequest,
  RequestUploadResponse,
  ConfirmUploadRequest,
  SubmitFileRequest,
  DownloadUrlResponse,
  ScanStatus,
} from '../types'

export const storageApi = {
  async requestUpload(data: RequestUploadRequest): Promise<RequestUploadResponse> {
    const response = await http.post<RequestUploadResponse>('/files/request-upload', data)
    return response.data
  },

  async uploadToPresignedUrl(
    url: string,
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<void> {
    await axios.put(url, file, {
      headers: {
        'Content-Type': file.type,
      },
      onUploadProgress: (progressEvent) => {
        if (progressEvent.total && onProgress) {
          const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          onProgress(progress)
        }
      },
    })
  },

  async confirmUpload(data: ConfirmUploadRequest): Promise<FileMetadata> {
    const response = await http.post<FileMetadata>('/files/confirm-upload', data)
    return response.data
  },

  async getScanStatus(fileId: string): Promise<{ status: ScanStatus }> {
    const response = await http.get<{ status: ScanStatus }>(`/files/${fileId}/scan-status`)
    return response.data
  },

  async submitFile(data: SubmitFileRequest): Promise<FileMetadata> {
    const response = await http.post<FileMetadata>(`/files/${data.fileId}/submit`, data)
    return response.data
  },

  async getDownloadUrl(fileId: string): Promise<DownloadUrlResponse> {
    const response = await http.get<DownloadUrlResponse>(`/files/${fileId}/download-url`)
    return response.data
  },

  async getFileMetadata(fileId: string): Promise<FileMetadata> {
    const response = await http.get<FileMetadata>(`/files/${fileId}`)
    return response.data
  },

  async deleteFile(fileId: string): Promise<void> {
    await http.delete(`/files/${fileId}`)
  },

  async getFilesByEntity(entityType: string, entityId: string): Promise<FileMetadata[]> {
    const response = await http.get<FileMetadata[]>(`/files/entity/${entityType}/${entityId}`)
    return response.data
  },
}
