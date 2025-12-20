import apiClient from "@/lib/api/axios";
import { API_ENDPOINTS } from "@/constants/api";

export interface FileMetadata {
  id: string;
  fileName: string;
  contentType: string;
  size: number;
  ownerService: string;
  ownerId?: string;
  uploadedBy?: string;
  createdAt: string;
  url?: string;
}

export interface RequestUploadDto {
  fileName: string;
  contentType: string;
  size: number;
  ownerService: string;
  metadata?: Record<string, string>;
}

export interface UploadUrlResponse {
  fileId: string;
  uploadUrl: string;
  expiresAt: string;
  requiredHeaders?: Record<string, string>;
}

export interface ConfirmUploadRequest {
  fileId: string;
  ownerId?: string;
  checksum?: string;
}

export interface DownloadUrlResponse {
  fileId: string;
  downloadUrl: string;
  expiresAt: string;
  fileName: string;
  contentType: string;
  size: number;
}

export const storageService = {
  requestUpload: async (dto: RequestUploadDto): Promise<UploadUrlResponse> => {
    const { data } = await apiClient.post<UploadUrlResponse>(
      API_ENDPOINTS.STORAGE.REQUEST_UPLOAD,
      dto
    );
    return data;
  },

  confirmUpload: async (request: ConfirmUploadRequest): Promise<FileMetadata> => {
    const { data } = await apiClient.post<FileMetadata>(
      API_ENDPOINTS.STORAGE.CONFIRM_UPLOAD,
      request
    );
    return data;
  },

  getFileMetadata: async (fileId: string): Promise<FileMetadata> => {
    const { data } = await apiClient.get<FileMetadata>(
      API_ENDPOINTS.STORAGE.FILE_METADATA(fileId)
    );
    return data;
  },

  getDownloadUrl: async (fileId: string): Promise<DownloadUrlResponse> => {
    const { data } = await apiClient.get<DownloadUrlResponse>(
      API_ENDPOINTS.STORAGE.DOWNLOAD_URL(fileId)
    );
    return data;
  },

  deleteFile: async (fileId: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.STORAGE.DELETE_FILE(fileId));
  },

  uploadFile: async (
    dto: RequestUploadDto,
    file: File,
    ownerId?: string
  ): Promise<FileMetadata> => {
    const uploadResponse = await storageService.requestUpload(dto);
    
    await fetch(uploadResponse.uploadUrl, {
      method: "PUT",
      body: file,
      headers: {
        "Content-Type": dto.contentType,
        ...uploadResponse.requiredHeaders,
      },
    });

    return storageService.confirmUpload({
      fileId: uploadResponse.fileId,
      ownerId,
    });
  },
} as const;
