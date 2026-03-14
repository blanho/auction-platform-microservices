import axios from 'axios'
import { http } from '@/services/http'
import type {
  StoredFileDto,
  BatchUploadResponse,
  FileUrlDto,
  PresignedUploadRequest,
  PresignedUploadDto,
  PresignedDownloadDto,
} from '@/shared/types/storage.types'
import { getAccessToken } from '@/modules/auth/utils/token.utils'
import { getCsrfToken } from '@/modules/auth/utils/csrf.utils'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

function buildAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = {}
  const token = getAccessToken()
  const csrfToken = getCsrfToken()

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }
  if (csrfToken) {
    headers['X-XSRF-TOKEN'] = csrfToken
  }

  return headers
}

function createProgressHandler(onProgress?: (progress: number) => void) {
  if (!onProgress) {
    return undefined
  }
  return (event: { loaded: number; total?: number }) => {
    if (event.total) {
      onProgress(Math.round((event.loaded * 100) / event.total))
    }
  }
}

export const storageApi = {
  async uploadFile(
    file: File,
    options?: {
      subFolder?: string
      onProgress?: (progress: number) => void
    }
  ): Promise<StoredFileDto> {
    const formData = new FormData()
    formData.append('file', file)

    if (options?.subFolder) {
      formData.append('subFolder', options.subFolder)
    }

    const response = await axios.post<StoredFileDto>(
      `${API_BASE_URL}/files`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
          ...buildAuthHeaders(),
        },
        withCredentials: true,
        onUploadProgress: createProgressHandler(options?.onProgress),
      }
    )

    return response.data
  },

  async uploadMultipleFiles(
    files: File[],
    options?: {
      subFolder?: string
      onProgress?: (progress: number) => void
    }
  ): Promise<BatchUploadResponse> {
    const formData = new FormData()

    for (const file of files) {
      formData.append('files', file)
    }

    if (options?.subFolder) {
      formData.append('subFolder', options.subFolder)
    }

    const response = await axios.post<BatchUploadResponse>(
      `${API_BASE_URL}/files/batch`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
          ...buildAuthHeaders(),
        },
        withCredentials: true,
        onUploadProgress: createProgressHandler(options?.onProgress),
      }
    )

    return response.data
  },

  async getFileUrl(fileId: string): Promise<FileUrlDto> {
    const response = await http.get<FileUrlDto>(`/files/${fileId}/url`)
    return response.data
  },

  async getFileMetadata(fileId: string): Promise<StoredFileDto> {
    const response = await http.get<StoredFileDto>(`/files/${fileId}`)
    return response.data
  },

  async deleteFile(fileId: string): Promise<void> {
    await http.delete(`/files/${fileId}`)
  },

  async getPresignedUploadUrl(request: PresignedUploadRequest): Promise<PresignedUploadDto> {
    const response = await http.post<PresignedUploadDto>('/files/presigned-upload', request)
    return response.data
  },

  async getPresignedDownloadUrl(fileId: string): Promise<PresignedDownloadDto> {
    const response = await http.get<PresignedDownloadDto>(`/files/${fileId}/download-url`)
    return response.data
  },

  async uploadToPresignedUrl(
    uploadUrl: string,
    file: File,
    headers: Record<string, string>,
    onProgress?: (progress: number) => void
  ): Promise<void> {
    await axios.put(uploadUrl, file, {
      headers: {
        ...headers,
        'Content-Type': file.type,
      },
      onUploadProgress: createProgressHandler(onProgress),
    })
  },

  async uploadLargeFile(
    file: File,
    options?: {
      subFolder?: string
      ownerId?: string
      onProgress?: (progress: number) => void
    }
  ): Promise<StoredFileDto> {
    const presigned = await this.getPresignedUploadUrl({
      fileName: file.name,
      contentType: file.type,
      fileSize: file.size,
      subFolder: options?.subFolder,
      ownerId: options?.ownerId,
    })

    if (!presigned.uploadUrl) {
      return this.uploadFile(file, options)
    }

    await this.uploadToPresignedUrl(
      presigned.uploadUrl,
      file,
      presigned.headers,
      options?.onProgress
    )

    return {
      fileId: presigned.fileId,
      fileName: file.name,
      contentType: file.type,
      fileSize: file.size,
      url: '',
      uploadedAt: new Date().toISOString(),
    }
  },
}
