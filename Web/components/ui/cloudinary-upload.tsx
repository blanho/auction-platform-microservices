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
import { cloudinaryService, UploadProgress } from '@/services/cloudinary.service';
import { cn } from '@/lib/utils';

export interface UploadedImage {
    id: string;
    publicId: string;
    url: string;
    name: string;
    isPrimary: boolean;
    status: 'uploading' | 'success' | 'error';
    progress: number;
    error?: string;
    file?: File;
}

export interface CloudinaryUploadRef {
    clearAll: () => void;
    retryFailed: () => void;
    getImages: () => UploadedImage[];
    hasErrors: () => boolean;
    isUploading: () => boolean;
}

interface CloudinaryUploadProps {
    value: UploadedImage[];
    onChange: (images: UploadedImage[]) => void;
    maxImages?: number;
    maxSizeMB?: number;
    folder?: string;
    className?: string;
    disabled?: boolean;
}

const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

const generateId = () => `img_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;

export const CloudinaryUpload = forwardRef<CloudinaryUploadRef, CloudinaryUploadProps>(
    function CloudinaryUpload({
        value,
        onChange,
        maxImages = 10,
        maxSizeMB = 10,
        folder = 'auction',
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
                const result = await cloudinaryService.upload(
                    image.file,
                    (progress: UploadProgress) => {
                        updateImage(image.id, { progress: progress.percentage });
                    },
                    folder
                );

                if (image.url.startsWith('blob:')) {
                    URL.revokeObjectURL(image.url);
                    objectUrlsRef.current.delete(image.url);
                }

                updateImage(image.id, {
                    publicId: result.publicId,
                    url: result.secureUrl,
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
        [folder, updateImage]
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
                    publicId: '',
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

    const handleDrop = useCallback(
        (e: React.DragEvent) => {
            e.preventDefault();
            setIsDragging(false);
            if (disabled) return;
            handleFiles(e.dataTransfer.files);
        },
        [disabled, handleFiles]
    );

    const handleDragOver = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(true);
    }, []);

    const handleDragLeave = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(false);
    }, []);

    const handleFileSelect = useCallback(
        (e: React.ChangeEvent<HTMLInputElement>) => {
            const files = e.target.files;
            if (files) {
                handleFiles(files);
            }
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        },
        [handleFiles]
    );

    const removeImage = useCallback(
        async (id: string) => {
            const image = value.find((img) => img.id === id);

            if (image?.url.startsWith('blob:')) {
                URL.revokeObjectURL(image.url);
                objectUrlsRef.current.delete(image.url);
            }

            if (image?.publicId && image.status === 'success') {
                cloudinaryService.delete(image.publicId).catch(() => { });
            }

            const updatedImages = value.filter((img) => img.id !== id);

            if (image?.isPrimary && updatedImages.length > 0) {
                const successImages = updatedImages.filter((img) => img.status === 'success');
                if (successImages.length > 0) {
                    successImages[0].isPrimary = true;
                } else {
                    updatedImages[0].isPrimary = true;
                }
            }

            onChange(updatedImages);
        },
        [value, onChange]
    );

    const setPrimaryImage = useCallback(
        (id: string) => {
            const targetImage = value.find((img) => img.id === id);
            if (!targetImage || targetImage.status !== 'success') return;

            onChange(value.map((img) => ({ ...img, isPrimary: img.id === id })));
        },
        [value, onChange]
    );

    const clearAll = useCallback(() => {
        value.forEach((image) => {
            if (image.url.startsWith('blob:')) {
                URL.revokeObjectURL(image.url);
                objectUrlsRef.current.delete(image.url);
            }
            if (image.publicId && image.status === 'success') {
                cloudinaryService.delete(image.publicId).catch(() => { });
            }
        });
        onChange([]);
    }, [value, onChange]);

    const retryAllFailed = useCallback(() => {
        const failedImages = value.filter((img) => img.status === 'error' && img.file);
        failedImages.forEach((image) => {
            onChange(value.map((img) => 
                img.id === image.id 
                    ? { ...img, status: 'uploading' as const, progress: 0, error: undefined } 
                    : img
            ));
            uploadSingleFile({ ...image, status: 'uploading', progress: 0, error: undefined });
        });
    }, [value, onChange, uploadSingleFile]);

    useImperativeHandle(ref, () => ({
        clearAll,
        retryFailed: retryAllFailed,
        getImages: () => value,
        hasErrors: () => value.some((img) => img.status === 'error'),
        isUploading: () => value.some((img) => img.status === 'uploading'),
    }), [clearAll, retryAllFailed, value]);

    const canUploadMore = value.length < maxImages;

    return (
        <div className={cn('space-y-4', className)}>
            <div
                className={cn(
                    'relative border-2 border-dashed rounded-xl p-8 text-center transition-all duration-300',
                    isDragging
                        ? 'border-purple-500 bg-purple-50 dark:bg-purple-900/20 scale-[1.01]'
                        : 'border-slate-300 dark:border-slate-700 hover:border-purple-400 dark:hover:border-purple-600',
                    disabled && 'opacity-50 cursor-not-allowed',
                    !canUploadMore && 'opacity-50'
                )}
                onDrop={handleDrop}
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
            >
                <input
                    ref={fileInputRef}
                    type="file"
                    accept={ACCEPTED_TYPES.join(',')}
                    multiple
                    className="hidden"
                    onChange={handleFileSelect}
                    disabled={disabled || !canUploadMore}
                />

                <motion.div
                    initial={false}
                    animate={{
                        scale: isDragging ? 1.05 : 1,
                        opacity: isDragging ? 0.8 : 1,
                    }}
                    className="space-y-4"
                >
                    <div className="w-16 h-16 mx-auto rounded-full bg-gradient-to-br from-purple-500 to-blue-500 flex items-center justify-center">
                        <FontAwesomeIcon
                            icon={faCloudArrowUp}
                            className="w-8 h-8 text-white"
                        />
                    </div>

                    <div>
                        <p className="text-lg font-medium text-slate-900 dark:text-white">
                            {isDragging ? 'Drop images here' : 'Drag and drop images'}
                        </p>
                        <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                            or click to browse • Max {maxImages} images • Up to {maxSizeMB}MB each
                        </p>
                    </div>

                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => fileInputRef.current?.click()}
                        disabled={disabled || !canUploadMore}
                        className="border-purple-300 dark:border-purple-700 hover:bg-purple-50 dark:hover:bg-purple-900/20"
                    >
                        <FontAwesomeIcon icon={faImage} className="w-4 h-4 mr-2" />
                        Select Images
                    </Button>

                    {!canUploadMore && (
                        <p className="text-sm text-amber-600 dark:text-amber-400">
                            Maximum {maxImages} images reached
                        </p>
                    )}
                </motion.div>

                {isDragging && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="absolute inset-0 bg-purple-500/10 rounded-xl pointer-events-none"
                    />
                )}
            </div>

            <AnimatePresence>
                {value.length > 0 && (
                    <motion.div
                        initial={{ opacity: 0, height: 0 }}
                        animate={{ opacity: 1, height: 'auto' }}
                        exit={{ opacity: 0, height: 0 }}
                        className="space-y-3"
                    >
                        <div className="flex items-center justify-between">
                            <p className="text-sm font-medium text-slate-600 dark:text-slate-400">
                                {value.length} of {maxImages} images uploaded
                            </p>
                            <p className="text-xs text-slate-500 dark:text-slate-500">
                                Click star to set primary image
                            </p>
                        </div>

                        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
                            <AnimatePresence mode="popLayout">
                                {value.map((image, index) => (
                                    <motion.div
                                        key={image.id}
                                        layout
                                        initial={{ opacity: 0, scale: 0.8 }}
                                        animate={{ opacity: 1, scale: 1 }}
                                        exit={{ opacity: 0, scale: 0.8 }}
                                        className={cn(
                                            'relative aspect-square rounded-xl overflow-hidden border-2 group',
                                            image.isPrimary
                                                ? 'border-amber-500 ring-2 ring-amber-500/30'
                                                : 'border-slate-200 dark:border-slate-700',
                                            image.status === 'error' && 'border-red-500'
                                        )}
                                    >
                                        <Image
                                            src={image.url}
                                            alt={image.name}
                                            fill
                                            className={cn(
                                                'object-cover transition-opacity',
                                                image.status === 'uploading' && 'opacity-50'
                                            )}
                                        />

                                        {image.isPrimary && (
                                            <div className="absolute top-2 left-2 px-2 py-0.5 rounded-full bg-amber-500 text-white text-xs font-medium z-10">
                                                Primary
                                            </div>
                                        )}

                                        {!image.isPrimary && (
                                            <div className="absolute top-2 left-2 px-1.5 py-0.5 rounded bg-black/60 text-white text-xs z-10">
                                                #{index + 1}
                                            </div>
                                        )}

                                        {image.status === 'uploading' && (
                                            <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/50 z-20">
                                                <FontAwesomeIcon
                                                    icon={faSpinner}
                                                    className="w-6 h-6 text-white animate-spin mb-2"
                                                />
                                                <div className="w-3/4">
                                                    <Progress value={image.progress} className="h-1.5" />
                                                </div>
                                                <span className="text-white text-xs mt-1">
                                                    {image.progress}%
                                                </span>
                                            </div>
                                        )}

                                        {image.status === 'success' && (
                                            <div className="absolute top-2 right-2 w-5 h-5 rounded-full bg-green-500 flex items-center justify-center z-10">
                                                <FontAwesomeIcon
                                                    icon={faCheck}
                                                    className="w-3 h-3 text-white"
                                                />
                                            </div>
                                        )}

                                        {image.status === 'error' && (
                                            <div className="absolute inset-0 flex flex-col items-center justify-center bg-red-500/90 z-20">
                                                <FontAwesomeIcon
                                                    icon={faTriangleExclamation}
                                                    className="w-5 h-5 text-white mb-1"
                                                />
                                                <p className="text-white text-[10px] text-center px-1 line-clamp-2 mb-2">
                                                    {image.error}
                                                </p>
                                                <div className="flex gap-1">
                                                    {image.file && (
                                                        <button
                                                            type="button"
                                                            onClick={() => retryUpload(image.id)}
                                                            className="flex items-center gap-1 px-2 py-1 rounded bg-white/20 hover:bg-white/30 text-white text-xs transition-colors"
                                                        >
                                                            <FontAwesomeIcon icon={faRotateRight} className="w-3 h-3" />
                                                            Retry
                                                        </button>
                                                    )}
                                                    <button
                                                        type="button"
                                                        onClick={() => removeImage(image.id)}
                                                        className="flex items-center gap-1 px-2 py-1 rounded bg-white/20 hover:bg-white/30 text-white text-xs transition-colors"
                                                    >
                                                        <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                                                        Remove
                                                    </button>
                                                </div>
                                            </div>
                                        )}

                                        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-colors" />

                                        <div className="absolute inset-x-0 bottom-0 p-2 opacity-0 group-hover:opacity-100 transition-opacity flex justify-center gap-1 z-10">
                                            {image.status === 'success' && (
                                                <button
                                                    type="button"
                                                    onClick={() => setPrimaryImage(image.id)}
                                                    className={cn(
                                                        'p-1.5 rounded-lg transition-colors',
                                                        image.isPrimary
                                                            ? 'bg-amber-500 text-white'
                                                            : 'bg-white/90 text-slate-700 hover:bg-amber-500 hover:text-white'
                                                    )}
                                                    title="Set as primary"
                                                >
                                                    <FontAwesomeIcon icon={faStar} className="w-3.5 h-3.5" />
                                                </button>
                                            )}

                                            <button
                                                type="button"
                                                onClick={() => removeImage(image.id)}
                                                className="p-1.5 rounded-lg bg-white/90 text-red-600 hover:bg-red-500 hover:text-white transition-colors"
                                                title="Remove image"
                                            >
                                                <FontAwesomeIcon icon={faXmark} className="w-3.5 h-3.5" />
                                            </button>
                                        </div>
                                    </motion.div>
                                ))}
                            </AnimatePresence>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>

            <p className="text-xs text-slate-500 dark:text-slate-500">
                Supported formats: JPG, PNG, WebP, GIF • Images are automatically optimized
            </p>
        </div>
    );
});

CloudinaryUpload.displayName = 'CloudinaryUpload';

export default CloudinaryUpload;
