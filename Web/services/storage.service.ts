import apiClient from "@/lib/api/axios";
import { API_ENDPOINTS } from "@/constants/api";

export interface FileMetadata {
  id: string;
  fileName: string;
  originalFileName?: string;
  contentType: string;
  size: number;
  ownerService: string;
  ownerId?: string;
  uploadedBy?: string;
  createdAt: string;
  url?: string;
  path?: string;
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

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

export interface UploadResult {
  id: string;
  fileName: string;
  url: string;
  size: number;
  contentType: string;
}

async function uploadToPresignedUrl(
  uploadUrl: string,
  file: File,
  requiredHeaders?: Record<string, string>,
  onProgress?: (progress: UploadProgress) => void
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable && onProgress) {
        onProgress({
          loaded: event.loaded,
          total: event.total,
          percentage: Math.round((event.loaded / event.total) * 100),
        });
      }
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve();
      } else {
        reject(new Error(`Upload failed with status ${xhr.status}`));
      }
    };

    xhr.onerror = () => reject(new Error("Network error during upload"));

    xhr.open("PUT", uploadUrl);

    if (requiredHeaders) {
      Object.entries(requiredHeaders).forEach(([key, value]) => {
        xhr.setRequestHeader(key, value);
      });
    }

    xhr.send(file);
  });
}

async function directUpload(
  file: File,
  ownerService: string = "AuctionService",
  onProgress?: (progress: UploadProgress) => void
): Promise<UploadResult> {
  const formData = new FormData();
  formData.append("file", file);

  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable && onProgress) {
        onProgress({
          loaded: event.loaded,
          total: event.total,
          percentage: Math.round((event.loaded / event.total) * 100),
        });
      }
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        const response = JSON.parse(xhr.responseText);
        resolve({
          id: response.id,
          fileName: response.originalFileName || response.fileName,
          url: response.url || response.path,
          size: response.size,
          contentType: response.contentType,
        });
      } else {
        let errorMessage = `Upload failed (${xhr.status})`;
        try {
          const errorResponse = JSON.parse(xhr.responseText);
          errorMessage = errorResponse.error || errorResponse.message || errorMessage;
        } catch {
          /* ignore parse errors */
        }
        reject(new Error(errorMessage));
      }
    };

    xhr.onerror = () => reject(new Error("Network error - please check your connection"));

    xhr.open("POST", `${API_ENDPOINTS.STORAGE.DIRECT_UPLOAD}?ownerService=${encodeURIComponent(ownerService)}`);
    
    const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;
    if (token) {
      xhr.setRequestHeader("Authorization", `Bearer ${token}`);
    }

    xhr.send(formData);
  });
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

  upload: async (
    file: File,
    onProgress?: (progress: UploadProgress) => void,
    ownerService: string = "AuctionService",
    ownerId?: string
  ): Promise<UploadResult> => {
    const uploadResponse = await storageService.requestUpload({
      fileName: file.name,
      contentType: file.type,
      size: file.size,
      ownerService,
    });

    await uploadToPresignedUrl(
      uploadResponse.uploadUrl,
      file,
      uploadResponse.requiredHeaders,
      onProgress
    );

    const metadata = await storageService.confirmUpload({
      fileId: uploadResponse.fileId,
      ownerId,
    });

    return {
      id: metadata.id,
      fileName: metadata.originalFileName || metadata.fileName,
      url: metadata.url || "",
      size: metadata.size,
      contentType: metadata.contentType,
    };
  },

  directUpload,

  uploadMultiple: async (
    files: File[],
    onProgress?: (fileIndex: number, progress: UploadProgress) => void,
    ownerService: string = "AuctionService"
  ): Promise<UploadResult[]> => {
    const results: UploadResult[] = [];

    for (let i = 0; i < files.length; i++) {
      const result = await directUpload(
        files[i],
        ownerService,
        (progress) => onProgress?.(i, progress)
      );
      results.push(result);
    }

    return results;
  },

  getImageUrl: (url: string, options?: { width?: number; height?: number }): string => {
    if (!url) return "";
    
    if (options?.width || options?.height) {
      const params = new URLSearchParams();
      if (options.width) params.append("w", options.width.toString());
      if (options.height) params.append("h", options.height.toString());
      
      const separator = url.includes("?") ? "&" : "?";
      return `${url}${separator}${params.toString()}`;
    }
    
    return url;
  },

  getThumbnailUrl: (url: string, size: number = 150): string => {
    return storageService.getImageUrl(url, { width: size, height: size });
  },
} as const;
