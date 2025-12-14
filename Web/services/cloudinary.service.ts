const CLOUD_NAME = process.env.NEXT_PUBLIC_CLOUDINARY_CLOUD_NAME || 'dg6qyxc0a';
const UPLOAD_PRESET = 'auction_unsigned';

export interface CloudinaryUploadResult {
  publicId: string;
  url: string;
  secureUrl: string;
  width: number;
  height: number;
  format: string;
  bytes: number;
  originalFilename: string;
}

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

export interface CloudinarySignature {
  signature: string;
  timestamp: number;
  apiKey: string;
  cloudName: string;
  folder: string;
}

async function getSignature(folder: string = 'auction'): Promise<CloudinarySignature> {
  const response = await fetch('/api/cloudinary/signature', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ folder }),
  });

  if (!response.ok) {
    throw new Error('Failed to get upload signature');
  }

  return response.json();
}

async function uploadToCloudinary(
  file: File,
  onProgress?: (progress: UploadProgress) => void,
  folder: string = 'auction'
): Promise<CloudinaryUploadResult> {
  const { signature, timestamp, apiKey, cloudName, folder: signedFolder } = await getSignature(folder);

  const formData = new FormData();
  formData.append('file', file);
  formData.append('api_key', apiKey);
  formData.append('timestamp', timestamp.toString());
  formData.append('signature', signature);
  formData.append('folder', signedFolder);

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
          publicId: response.public_id,
          url: response.url,
          secureUrl: response.secure_url,
          width: response.width,
          height: response.height,
          format: response.format,
          bytes: response.bytes,
          originalFilename: response.original_filename,
        });
      } else {
        let errorMessage = `Upload failed (${xhr.status})`;
        try {
          const errorResponse = JSON.parse(xhr.responseText);
          errorMessage = errorResponse.error?.message || errorMessage;
        } catch {
          /* ignore parse errors */
        }
        reject(new Error(errorMessage));
      }
    };

    xhr.onerror = () => reject(new Error('Network error - please check your connection'));

    xhr.open('POST', `https://api.cloudinary.com/v1_1/${cloudName}/image/upload`);
    xhr.send(formData);
  });
}

async function uploadMultiple(
  files: File[],
  onProgress?: (fileIndex: number, progress: UploadProgress) => void,
  folder: string = 'auctions'
): Promise<CloudinaryUploadResult[]> {
  const results: CloudinaryUploadResult[] = [];

  for (let i = 0; i < files.length; i++) {
    const result = await uploadToCloudinary(
      files[i],
      (progress) => onProgress?.(i, progress),
      folder
    );
    results.push(result);
  }

  return results;
}

function deleteImage(publicId: string): Promise<void> {
  return fetch('/api/cloudinary/delete', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ publicId }),
  }).then((res) => {
    if (!res.ok) throw new Error('Failed to delete image');
  });
}

function getOptimizedUrl(
  publicId: string,
  options: {
    width?: number;
    height?: number;
    quality?: number | 'auto';
    format?: 'auto' | 'webp' | 'avif' | 'jpg' | 'png';
    crop?: 'fill' | 'fit' | 'scale' | 'thumb';
  } = {}
): string {
  const { width, height, quality = 'auto', format = 'auto', crop = 'fill' } = options;

  const transformations: string[] = [];

  if (width || height) {
    const dims = [width && `w_${width}`, height && `h_${height}`, `c_${crop}`]
      .filter(Boolean)
      .join(',');
    transformations.push(dims);
  }

  transformations.push(`q_${quality}`);
  transformations.push(`f_${format}`);

  const transformString = transformations.join('/');

  return `https://res.cloudinary.com/${CLOUD_NAME}/image/upload/${transformString}/${publicId}`;
}

function getThumbnailUrl(publicId: string, size: number = 150): string {
  return getOptimizedUrl(publicId, {
    width: size,
    height: size,
    crop: 'thumb',
    quality: 80,
  });
}

function getResponsiveUrl(publicId: string, width: number): string {
  return getOptimizedUrl(publicId, {
    width,
    quality: 'auto',
    format: 'auto',
    crop: 'scale',
  });
}

export const cloudinaryService = {
  upload: uploadToCloudinary,
  uploadMultiple,
  delete: deleteImage,
  getOptimizedUrl,
  getThumbnailUrl,
  getResponsiveUrl,
  CLOUD_NAME,
};
