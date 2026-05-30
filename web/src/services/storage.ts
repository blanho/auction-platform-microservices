import { http } from '@/services/http'
import type {
  StoredFileDto,
  BatchUploadResponse,
  FileUrlDto,
  PresignedUploadRequest,
  PresignedUploadDto,
  PresignedDownloadDto,
} from '@/shared/types/storage.types'

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

    const { data } = await http.postForm<StoredFileDto>('/files', formData, {
      onUploadProgress: createProgressHandler(options?.onProgress),
    })

    return data
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

    const { data } = await http.postForm<BatchUploadResponse>('/files/batch', formData, {
      onUploadProgress: createProgressHandler(options?.onProgress),
    })

    return data
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
    // Presigned URL uploads go to external storage (e.g. Azure Blob) — raw fetch is intentional here.
    await fetch(uploadUrl, {
      method: 'PUT',
      headers: { ...headers, 'Content-Type': file.type },
      body: file,
    })
    onProgress?.(100)
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
