'use client';

import { useState, useCallback, useRef, useEffect, forwardRef, useImperativeHandle } from 'react';
import Image from 'next/image';
import { motion, AnimatePresence } from 'framer-motion';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faCloudArrowUp,
    faImage,
    faXmark,
    faSpinner,
    faCheck,
    faStar,
    faTriangleExclamation,
    faRotateRight,
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { storageService, UploadProgress } from '@/services/storage.service';
import { cn } from '@/lib/utils';

export interface UploadedImage {
    id: string;
    fileId: string;
    url: string;
    name: string;
    isPrimary: boolean;
    status: 'uploading' | 'success' | 'error';
    progress: number;
    error?: string;
    file?: File;
}

export interface ImageUploadRef {
    clearAll: () => void;
    retryFailed: () => void;
    getImages: () => UploadedImage[];
    hasErrors: () => boolean;
    isUploading: () => boolean;
}

interface ImageUploadProps {
    value: UploadedImage[];
    onChange: (images: UploadedImage[]) => void;
    maxImages?: number;
    maxSizeMB?: number;
    ownerService?: string;
    className?: string;
    disabled?: boolean;
}

const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

const generateId = () => `img_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;

export const ImageUpload = forwardRef<ImageUploadRef, ImageUploadProps>(
    function ImageUpload({
        value,
        onChange,
        maxImages = 10,
        maxSizeMB = 10,
        ownerService = 'AuctionService',
        className,
        disabled = false,
    }, ref) {
    const [isDragging, setIsDragging] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const objectUrlsRef = useRef<Set<string>>(new Set());

    useEffect(() => {
        const urls = objectUrlsRef.current;
        return () => {
            urls.forEach((url) => URL.revokeObjectURL(url));
            urls.clear();
        };
    }, []);

    const validateFile = useCallback(
        (file: File): string | null => {
            if (!ACCEPTED_TYPES.includes(file.type)) {
                return 'Invalid file type. Accepted: JPG, PNG, WebP, GIF';
            }
            if (file.size > maxSizeMB * 1024 * 1024) {
                return `File too large. Max size: ${maxSizeMB}MB`;
            }
            return null;
        },
        [maxSizeMB]
    );

    const updateImage = useCallback(
        (id: string, updates: Partial<UploadedImage>) => {
            onChange(value.map((img) => (img.id === id ? { ...img, ...updates } : img)));
        },
        [onChange, value]
    );

    const uploadSingleFile = useCallback(
        async (image: UploadedImage): Promise<void> => {
            if (!image.file) return;

            try {
                const result = await storageService.upload(
                    image.file,
                    (progress: UploadProgress) => {
                        updateImage(image.id, { progress: progress.percentage });
                    },
                    ownerService
                );

                if (image.url.startsWith('blob:')) {
                    URL.revokeObjectURL(image.url);
                    objectUrlsRef.current.delete(image.url);
                }

                updateImage(image.id, {
                    fileId: result.id,
                    url: result.url,
                    status: 'success',
                    progress: 100,
                    file: undefined,
                });

                toast.success(`"${image.name}" uploaded`);
            } catch (error) {
                const errorMessage = error instanceof Error ? error.message : 'Upload failed';
                updateImage(image.id, { status: 'error', error: errorMessage });
                toast.error(`Failed to upload "${image.name}"`);
            }
        },
        [ownerService, updateImage]
    );

    const handleFiles = useCallback(
        (files: FileList | File[]) => {
            const fileArray = Array.from(files);
            const remainingSlots = maxImages - value.length;

            if (remainingSlots <= 0) {
                toast.warning(`Maximum ${maxImages} images allowed`);
                return;
            }

            const filesToProcess = fileArray.slice(0, remainingSlots);
            const newImages: UploadedImage[] = [];
            let invalidCount = 0;

            filesToProcess.forEach((file) => {
                const error = validateFile(file);
                if (error) {
                    invalidCount++;
                    return;
                }

                const tempUrl = URL.createObjectURL(file);
                objectUrlsRef.current.add(tempUrl);

                newImages.push({
                    id: generateId(),
                    fileId: '',
                    url: tempUrl,
                    name: file.name,
                    isPrimary: value.length === 0 && newImages.length === 0,
                    status: 'uploading',
                    progress: 0,
                    file,
                });
            });

            if (invalidCount > 0) {
                toast.error(`${invalidCount} file(s) skipped (invalid type or too large)`);
            }

            if (newImages.length === 0) return;

            const updatedImages = [...value, ...newImages];
            onChange(updatedImages);

            newImages.forEach((image) => {
                uploadSingleFile(image);
            });
        },
        [maxImages, value, validateFile, onChange, uploadSingleFile]
    );

    const retryUpload = useCallback(
        (id: string) => {
            const image = value.find((img) => img.id === id);
            if (!image?.file) {
                toast.error('Cannot retry - original file not available');
                return;
            }

            onChange(value.map((img) => (img.id === id ? { ...img, status: 'uploading', progress: 0, error: undefined } : img)));
            uploadSingleFile({ ...image, status: 'uploading', progress: 0, error: undefined });
        },
        [value, onChange, uploadSingleFile]
    );

    const removeImage = useCallback(
        async (id: string) => {
            const image = value.find((img) => img.id === id);
            if (!image) return;

            if (image.url.startsWith('blob:')) {
                URL.revokeObjectURL(image.url);
                objectUrlsRef.current.delete(image.url);
            }

            const wasFirst = image.isPrimary;
            const filteredImages = value.filter((img) => img.id !== id);

            if (wasFirst && filteredImages.length > 0) {
                filteredImages[0] = { ...filteredImages[0], isPrimary: true };
            }

            onChange(filteredImages);

            if (image.status === 'success' && image.fileId) {
                storageService.deleteFile(image.fileId).catch(() => { });
            }
        },
        [value, onChange]
    );

    const setPrimary = useCallback(
        (id: string) => {
            onChange(value.map((img) => ({ ...img, isPrimary: img.id === id })));
        },
        [value, onChange]
    );

    const retryAllFailed = useCallback(() => {
        value
            .filter((img) => img.status === 'error' && img.file)
            .forEach((img) => retryUpload(img.id));
    }, [value, retryUpload]);

    const clearAll = useCallback(() => {
        value.forEach((img) => {
            if (img.url.startsWith('blob:')) {
                URL.revokeObjectURL(img.url);
                objectUrlsRef.current.delete(img.url);
            }
            if (img.status === 'success' && img.fileId) {
                storageService.deleteFile(img.fileId).catch(() => { });
            }
        });
        onChange([]);
    }, [value, onChange]);

    useImperativeHandle(ref, () => ({
        clearAll,
        retryFailed: retryAllFailed,
        getImages: () => value,
        hasErrors: () => value.some((img) => img.status === 'error'),
        isUploading: () => value.some((img) => img.status === 'uploading'),
    }));

    const handleDragEnter = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (!disabled) setIsDragging(true);
    }, [disabled]);

    const handleDragLeave = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setIsDragging(false);
    }, []);

    const handleDragOver = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
    }, []);

    const handleDrop = useCallback(
        (e: React.DragEvent) => {
            e.preventDefault();
            e.stopPropagation();
            setIsDragging(false);
            if (disabled) return;
            handleFiles(e.dataTransfer.files);
        },
        [disabled, handleFiles]
    );

    const handleClick = useCallback(() => {
        if (!disabled) fileInputRef.current?.click();
    }, [disabled]);

    const handleFileChange = useCallback(
        (e: React.ChangeEvent<HTMLInputElement>) => {
            if (e.target.files) handleFiles(e.target.files);
            e.target.value = '';
        },
        [handleFiles]
    );

    const uploadingCount = value.filter((img) => img.status === 'uploading').length;
    const errorCount = value.filter((img) => img.status === 'error').length;
    const successCount = value.filter((img) => img.status === 'success').length;

    return (
        <div className={cn('space-y-4', className)}>
            <div
                onDragEnter={handleDragEnter}
                onDragLeave={handleDragLeave}
                onDragOver={handleDragOver}
                onDrop={handleDrop}
                onClick={handleClick}
                className={cn(
                    'relative border-2 border-dashed rounded-xl p-8 transition-all cursor-pointer',
                    'hover:border-purple-400 hover:bg-purple-50/50 dark:hover:bg-purple-900/10',
                    isDragging && 'border-purple-500 bg-purple-50 dark:bg-purple-900/20 scale-[1.02]',
                    disabled && 'opacity-50 cursor-not-allowed',
                    'border-slate-300 dark:border-slate-700'
                )}
            >
                <input
                    ref={fileInputRef}
                    type="file"
                    accept={ACCEPTED_TYPES.join(',')}
                    multiple
                    onChange={handleFileChange}
                    className="hidden"
                    disabled={disabled}
                />
                <div className="flex flex-col items-center justify-center gap-3 text-center">
                    <div className={cn(
                        'w-16 h-16 rounded-full flex items-center justify-center transition-colors',
                        isDragging
                            ? 'bg-purple-100 dark:bg-purple-900/30'
                            : 'bg-slate-100 dark:bg-slate-800'
                    )}>
                        <FontAwesomeIcon
                            icon={faCloudArrowUp}
                            className={cn(
                                'w-8 h-8 transition-colors',
                                isDragging ? 'text-purple-500' : 'text-slate-400'
                            )}
                        />
                    </div>
                    <div>
                        <p className="font-medium text-slate-700 dark:text-slate-200">
                            {isDragging ? 'Drop images here' : 'Drag & drop images'}
                        </p>
                        <p className="text-sm text-slate-500 dark:text-slate-400">
                            or click to browse • Max {maxSizeMB}MB per file
                        </p>
                    </div>
                    <div className="flex items-center gap-2 text-xs text-slate-400">
                        <FontAwesomeIcon icon={faImage} className="w-3 h-3" />
                        <span>JPG, PNG, WebP, GIF • {value.length}/{maxImages} images</span>
                    </div>
                </div>
            </div>

            {(uploadingCount > 0 || errorCount > 0) && (
                <div className="flex items-center justify-between px-3 py-2 bg-slate-100 dark:bg-slate-800 rounded-lg text-sm">
                    <div className="flex items-center gap-4">
                        {uploadingCount > 0 && (
                            <span className="flex items-center gap-1.5 text-blue-600 dark:text-blue-400">
                                <FontAwesomeIcon icon={faSpinner} className="w-3 h-3 animate-spin" />
                                {uploadingCount} uploading
                            </span>
                        )}
                        {successCount > 0 && (
                            <span className="flex items-center gap-1.5 text-green-600 dark:text-green-400">
                                <FontAwesomeIcon icon={faCheck} className="w-3 h-3" />
                                {successCount} uploaded
                            </span>
                        )}
                        {errorCount > 0 && (
                            <span className="flex items-center gap-1.5 text-red-600 dark:text-red-400">
                                <FontAwesomeIcon icon={faTriangleExclamation} className="w-3 h-3" />
                                {errorCount} failed
                            </span>
                        )}
                    </div>
                    {errorCount > 0 && (
                        <Button variant="ghost" size="sm" onClick={retryAllFailed} className="text-xs h-7">
                            <FontAwesomeIcon icon={faRotateRight} className="w-3 h-3 mr-1" />
                            Retry all
                        </Button>
                    )}
                </div>
            )}

            <AnimatePresence mode="popLayout">
                {value.length > 0 && (
                    <motion.div
                        initial={{ opacity: 0, height: 0 }}
                        animate={{ opacity: 1, height: 'auto' }}
                        exit={{ opacity: 0, height: 0 }}
                        className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3"
                    >
                        {value.map((image, index) => (
                            <motion.div
                                key={image.id}
                                layout
                                initial={{ opacity: 0, scale: 0.8 }}
                                animate={{ opacity: 1, scale: 1 }}
                                exit={{ opacity: 0, scale: 0.8 }}
                                className={cn(
                                    'group relative aspect-square rounded-lg overflow-hidden',
                                    'border-2 transition-all',
                                    image.isPrimary
                                        ? 'border-purple-500 ring-2 ring-purple-200 dark:ring-purple-800'
                                        : 'border-slate-200 dark:border-slate-700',
                                    image.status === 'error' && 'border-red-400'
                                )}
                            >
                                <Image
                                    src={image.url}
                                    alt={image.name}
                                    fill
                                    className="object-cover"
                                    sizes="(max-width: 640px) 50vw, (max-width: 768px) 33vw, 20vw"
                                />

                                {image.status === 'uploading' && (
                                    <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
                                        <div className="w-3/4">
                                            <Progress value={image.progress} className="h-2" />
                                            <p className="text-white text-xs text-center mt-1">{image.progress}%</p>
                                        </div>
                                    </div>
                                )}

                                {image.status === 'error' && (
                                    <div className="absolute inset-0 bg-red-500/80 flex flex-col items-center justify-center p-2">
                                        <FontAwesomeIcon icon={faTriangleExclamation} className="w-6 h-6 text-white mb-1" />
                                        <p className="text-white text-xs text-center line-clamp-2">{image.error}</p>
                                        <Button
                                            variant="secondary"
                                            size="sm"
                                            className="mt-2 h-6 text-xs"
                                            onClick={(e) => { e.stopPropagation(); retryUpload(image.id); }}
                                        >
                                            Retry
                                        </Button>
                                    </div>
                                )}

                                {image.status === 'success' && (
                                    <div className="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-colors">
                                        <div className="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                variant="secondary"
                                                size="icon"
                                                className="h-7 w-7"
                                                onClick={(e) => { e.stopPropagation(); removeImage(image.id); }}
                                            >
                                                <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                                            </Button>
                                        </div>
                                        {!image.isPrimary && (
                                            <Button
                                                variant="secondary"
                                                size="sm"
                                                className="absolute bottom-2 left-1/2 -translate-x-1/2 h-7 text-xs opacity-0 group-hover:opacity-100 transition-opacity"
                                                onClick={(e) => { e.stopPropagation(); setPrimary(image.id); }}
                                            >
                                                <FontAwesomeIcon icon={faStar} className="w-3 h-3 mr-1" />
                                                Set primary
                                            </Button>
                                        )}
                                    </div>
                                )}

                                {image.isPrimary && image.status === 'success' && (
                                    <div className="absolute top-2 left-2 px-2 py-0.5 bg-purple-500 text-white text-xs font-medium rounded">
                                        Primary
                                    </div>
                                )}

                                <div className="absolute bottom-0 left-0 right-0 p-1.5 bg-gradient-to-t from-black/60 to-transparent">
                                    <p className="text-white text-xs truncate">{index + 1}. {image.name}</p>
                                </div>
                            </motion.div>
                        ))}
                    </motion.div>
                )}
            </AnimatePresence>

            {value.length > 0 && (
                <div className="flex justify-end">
                    <Button variant="ghost" size="sm" onClick={clearAll} className="text-red-600 hover:text-red-700">
                        <FontAwesomeIcon icon={faXmark} className="w-3 h-3 mr-1" />
                        Clear all
                    </Button>
                </div>
            )}
        </div>
    );
});

ImageUpload.displayName = 'ImageUpload';

export default ImageUpload;
