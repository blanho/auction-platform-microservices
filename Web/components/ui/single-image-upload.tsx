'use client';

import { useState, useCallback, useRef } from 'react';
import Image from 'next/image';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faCloudArrowUp,
    faXmark,
    faSpinner,
    faImage,
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { cloudinaryService } from '@/services/cloudinary.service';
import { cn } from '@/lib/utils';

interface SingleImageUploadProps {
    value: string;
    onChange: (url: string) => void;
    folder?: string;
    className?: string;
    disabled?: boolean;
    placeholder?: string;
    aspectRatio?: 'square' | 'video' | 'banner';
}

const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
const MAX_SIZE_MB = 5;

export function SingleImageUpload({
    value,
    onChange,
    folder = 'auction',
    className,
    disabled = false,
    placeholder = 'Upload an image or enter URL',
    aspectRatio = 'square',
}: SingleImageUploadProps) {
    const [isUploading, setIsUploading] = useState(false);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [showUrlInput, setShowUrlInput] = useState(false);
    const [urlInput, setUrlInput] = useState('');
    const fileInputRef = useRef<HTMLInputElement>(null);

    const aspectClasses = {
        square: 'aspect-square',
        video: 'aspect-video',
        banner: 'aspect-[3/1]',
    };

    const validateFile = useCallback((file: File): string | null => {
        if (!ACCEPTED_TYPES.includes(file.type)) {
            return 'Invalid file type. Accepted: JPG, PNG, WebP, GIF';
        }
        if (file.size > MAX_SIZE_MB * 1024 * 1024) {
            return `File too large. Max size: ${MAX_SIZE_MB}MB`;
        }
        return null;
    }, []);

    const handleFileSelect = useCallback(
        async (e: React.ChangeEvent<HTMLInputElement>) => {
            const file = e.target.files?.[0];
            if (!file) return;

            const error = validateFile(file);
            if (error) {
                toast.error(error);
                return;
            }

            setIsUploading(true);
            setUploadProgress(0);

            try {
                const result = await cloudinaryService.upload(
                    file,
                    (progress) => setUploadProgress(progress.percentage),
                    folder
                );
                onChange(result.secureUrl);
                toast.success('Image uploaded successfully');
            } catch {
                toast.error('Failed to upload image');
            } finally {
                setIsUploading(false);
                setUploadProgress(0);
                if (fileInputRef.current) {
                    fileInputRef.current.value = '';
                }
            }
        },
        [folder, onChange, validateFile]
    );

    const handleUrlSubmit = useCallback(() => {
        if (!urlInput.trim()) return;
        
        try {
            new URL(urlInput);
            onChange(urlInput.trim());
            setUrlInput('');
            setShowUrlInput(false);
            toast.success('Image URL set');
        } catch {
            toast.error('Invalid URL');
        }
    }, [urlInput, onChange]);

    const handleRemove = useCallback(() => {
        onChange('');
    }, [onChange]);

    const handleDrop = useCallback(
        (e: React.DragEvent) => {
            e.preventDefault();
            if (disabled || isUploading) return;

            const file = e.dataTransfer.files[0];
            if (file) {
                const input = fileInputRef.current;
                if (input) {
                    const dataTransfer = new DataTransfer();
                    dataTransfer.items.add(file);
                    input.files = dataTransfer.files;
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
            }
        },
        [disabled, isUploading]
    );

    const handleDragOver = useCallback((e: React.DragEvent) => {
        e.preventDefault();
    }, []);

    return (
        <div className={cn('space-y-2', className)}>
            <div
                className={cn(
                    'relative border-2 border-dashed rounded-lg overflow-hidden transition-colors',
                    aspectClasses[aspectRatio],
                    disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer hover:border-purple-400',
                    isUploading ? 'border-purple-400 bg-purple-50 dark:bg-purple-950/20' : 'border-slate-200 dark:border-slate-700'
                )}
                onDrop={handleDrop}
                onDragOver={handleDragOver}
                onClick={() => !disabled && !isUploading && fileInputRef.current?.click()}
            >
                {value ? (
                    <div className="relative w-full h-full group">
                        <Image
                            src={value}
                            alt="Uploaded"
                            fill
                            className="object-cover"
                            unoptimized
                        />
                        {!disabled && (
                            <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                <Button
                                    type="button"
                                    variant="destructive"
                                    size="sm"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleRemove();
                                    }}
                                >
                                    <FontAwesomeIcon icon={faXmark} className="w-4 h-4 mr-1" />
                                    Remove
                                </Button>
                            </div>
                        )}
                    </div>
                ) : isUploading ? (
                    <div className="absolute inset-0 flex flex-col items-center justify-center p-4">
                        <FontAwesomeIcon
                            icon={faSpinner}
                            className="w-8 h-8 text-purple-500 animate-spin mb-2"
                        />
                        <span className="text-sm text-slate-600 dark:text-slate-400">
                            Uploading... {uploadProgress}%
                        </span>
                        <div className="w-full max-w-32 h-2 bg-slate-200 dark:bg-slate-700 rounded-full mt-2 overflow-hidden">
                            <div
                                className="h-full bg-purple-500 transition-all duration-300"
                                style={{ width: `${uploadProgress}%` }}
                            />
                        </div>
                    </div>
                ) : (
                    <div className="absolute inset-0 flex flex-col items-center justify-center p-4 text-center">
                        <FontAwesomeIcon
                            icon={faCloudArrowUp}
                            className="w-8 h-8 text-slate-400 mb-2"
                        />
                        <span className="text-sm text-slate-600 dark:text-slate-400">
                            Click or drag to upload
                        </span>
                        <span className="text-xs text-slate-400 mt-1">
                            JPG, PNG, WebP, GIF (max {MAX_SIZE_MB}MB)
                        </span>
                    </div>
                )}

                <input
                    ref={fileInputRef}
                    type="file"
                    accept={ACCEPTED_TYPES.join(',')}
                    className="hidden"
                    onChange={handleFileSelect}
                    disabled={disabled || isUploading}
                />
            </div>

            {!value && !isUploading && (
                <div className="flex items-center gap-2">
                    {showUrlInput ? (
                        <>
                            <Input
                                value={urlInput}
                                onChange={(e) => setUrlInput(e.target.value)}
                                placeholder="https://example.com/image.jpg"
                                className="flex-1"
                                onKeyDown={(e) => e.key === 'Enter' && handleUrlSubmit()}
                            />
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={handleUrlSubmit}
                                disabled={!urlInput.trim()}
                            >
                                Set
                            </Button>
                            <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                onClick={() => {
                                    setShowUrlInput(false);
                                    setUrlInput('');
                                }}
                            >
                                Cancel
                            </Button>
                        </>
                    ) : (
                        <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            className="text-xs"
                            onClick={() => setShowUrlInput(true)}
                        >
                            <FontAwesomeIcon icon={faImage} className="w-3 h-3 mr-1" />
                            Or enter URL
                        </Button>
                    )}
                </div>
            )}
        </div>
    );
}
