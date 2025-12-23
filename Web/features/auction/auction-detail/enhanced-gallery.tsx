"use client";

import { useState, useCallback, useEffect } from "react";
import Image from "next/image";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faChevronLeft,
    faChevronRight,
    faExpand,
    faXmark,
    faFire,
    faMagnifyingGlassPlus,
    faMagnifyingGlassMinus,
    faRotateRight,
} from "@fortawesome/free-solid-svg-icons";
import { AuctionStatus } from "@/types/auction";
import { getAuctionStatusStyle } from "@/constants/status";
import { cn } from "@/lib/utils";

interface EnhancedGalleryProps {
    images: string[];
    title: string;
    status?: AuctionStatus;
    isUrgent?: boolean;
}

export function EnhancedGallery({ images, title, status, isUrgent }: EnhancedGalleryProps) {
    const [selectedIndex, setSelectedIndex] = useState(0);
    const [isLightboxOpen, setIsLightboxOpen] = useState(false);
    const [isZoomed, setIsZoomed] = useState(false);
    const [zoomLevel, setZoomLevel] = useState(1);
    const [mousePosition, setMousePosition] = useState({ x: 50, y: 50 });

    const statusConfig = status ? getAuctionStatusStyle(status) : null;
    const currentImage = images[selectedIndex] || "/placeholder-car.jpg";
    const hasMultipleImages = images.length > 1;

    const navigateImage = useCallback((direction: "prev" | "next") => {
        if (direction === "prev") {
            setSelectedIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
        } else {
            setSelectedIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
        }
        setZoomLevel(1);
        setIsZoomed(false);
    }, [images.length]);

    const handleKeyDown = useCallback((e: KeyboardEvent) => {
        if (!isLightboxOpen) return;
        
        switch (e.key) {
            case "ArrowLeft":
                navigateImage("prev");
                break;
            case "ArrowRight":
                navigateImage("next");
                break;
            case "Escape":
                setIsLightboxOpen(false);
                setZoomLevel(1);
                break;
            case "+":
            case "=":
                setZoomLevel((prev) => Math.min(prev + 0.5, 3));
                break;
            case "-":
                setZoomLevel((prev) => Math.max(prev - 0.5, 1));
                break;
        }
    }, [isLightboxOpen, navigateImage]);

    useEffect(() => {
        document.addEventListener("keydown", handleKeyDown);
        return () => document.removeEventListener("keydown", handleKeyDown);
    }, [handleKeyDown]);

    useEffect(() => {
        if (isLightboxOpen) {
            document.body.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "";
        }
        return () => {
            document.body.style.overflow = "";
        };
    }, [isLightboxOpen]);

    const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
        if (!isZoomed) return;
        const rect = e.currentTarget.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        const y = ((e.clientY - rect.top) / rect.height) * 100;
        setMousePosition({ x, y });
    };

    const toggleZoom = () => {
        setIsZoomed(!isZoomed);
        if (isZoomed) {
            setMousePosition({ x: 50, y: 50 });
        }
    };

    return (
        <>
            <div className="space-y-3">
                <div className="relative rounded-2xl overflow-hidden bg-slate-100 dark:bg-slate-900 shadow-xl group">
                    <div
                        className="relative aspect-4/3 w-full cursor-zoom-in"
                        onMouseMove={handleMouseMove}
                        onClick={toggleZoom}
                    >
                        <motion.div
                            className="w-full h-full relative"
                            style={{
                                transformOrigin: `${mousePosition.x}% ${mousePosition.y}%`,
                            }}
                            animate={{
                                scale: isZoomed ? 2 : 1,
                            }}
                            transition={{ duration: 0.3 }}
                        >
                            <Image
                                src={currentImage}
                                alt={title}
                                fill
                                className="object-cover"
                                priority
                            />
                        </motion.div>

                        {statusConfig && (
                            <div
                                className={cn(
                                    "absolute top-4 left-4 px-4 py-2 rounded-full bg-linear-to-r text-white text-sm font-semibold shadow-lg flex items-center gap-2",
                                    statusConfig.gradient,
                                    statusConfig.bgGlow
                                )}
                            >
                                {statusConfig.pulse && (
                                    <span className="w-2 h-2 rounded-full bg-white animate-pulse" />
                                )}
                                {status}
                            </div>
                        )}

                        {status === AuctionStatus.Live && isUrgent && (
                            <motion.div
                                initial={{ scale: 0.9 }}
                                animate={{ scale: [0.9, 1, 0.9] }}
                                transition={{ duration: 1.5, repeat: Infinity }}
                                className="absolute top-4 right-4 px-3 py-1.5 rounded-full bg-linear-to-r from-red-500 to-orange-500 text-white text-xs font-semibold shadow-lg flex items-center gap-1.5"
                            >
                                <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                                Ending Soon
                            </motion.div>
                        )}

                        <div className="absolute bottom-4 left-4 flex items-center gap-2">
                            <span className="px-3 py-1.5 rounded-full bg-black/50 backdrop-blur-sm text-white text-xs font-medium">
                                {selectedIndex + 1} / {images.length || 1}
                            </span>
                            {isZoomed && (
                                <span className="px-3 py-1.5 rounded-full bg-purple-500/80 backdrop-blur-sm text-white text-xs font-medium">
                                    Click to zoom out
                                </span>
                            )}
                        </div>

                        <motion.button
                            whileHover={{ scale: 1.1 }}
                            whileTap={{ scale: 0.9 }}
                            onClick={(e) => {
                                e.stopPropagation();
                                setIsLightboxOpen(true);
                            }}
                            className="absolute bottom-4 right-4 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all"
                        >
                            <FontAwesomeIcon icon={faExpand} className="w-4 h-4 text-slate-700 dark:text-white" />
                        </motion.button>

                        {hasMultipleImages && !isZoomed && (
                            <>
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        navigateImage("prev");
                                    }}
                                    className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all"
                                >
                                    <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 text-slate-700 dark:text-white" />
                                </motion.button>
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        navigateImage("next");
                                    }}
                                    className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all"
                                >
                                    <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4 text-slate-700 dark:text-white" />
                                </motion.button>
                            </>
                        )}
                    </div>
                </div>

                {hasMultipleImages && (
                    <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-thin scrollbar-thumb-slate-300 dark:scrollbar-thumb-slate-700">
                        {images.map((img, idx) => (
                            <motion.button
                                key={idx}
                                whileHover={{ scale: 1.05, y: -2 }}
                                whileTap={{ scale: 0.95 }}
                                onClick={() => {
                                    setSelectedIndex(idx);
                                    setIsZoomed(false);
                                }}
                                className={cn(
                                    "relative w-20 h-20 shrink-0 rounded-xl overflow-hidden transition-all",
                                    selectedIndex === idx
                                        ? "ring-2 ring-purple-500 ring-offset-2 ring-offset-white dark:ring-offset-slate-900 shadow-lg shadow-purple-500/20"
                                        : "opacity-60 hover:opacity-100"
                                )}
                            >
                                <Image
                                    src={img}
                                    alt={`${title} - Image ${idx + 1}`}
                                    fill
                                    className="object-cover"
                                />
                                {selectedIndex === idx && (
                                    <motion.div
                                        layoutId="thumbnail-indicator"
                                        className="absolute inset-0 border-2 border-purple-500 rounded-xl"
                                    />
                                )}
                            </motion.button>
                        ))}
                    </div>
                )}
            </div>

            <AnimatePresence>
                {isLightboxOpen && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="fixed inset-0 z-50 bg-black/95 backdrop-blur-xl flex items-center justify-center"
                        onClick={() => {
                            setIsLightboxOpen(false);
                            setZoomLevel(1);
                        }}
                    >
                        <div className="absolute top-4 left-4 right-4 flex items-center justify-between z-10">
                            <span className="px-4 py-2 rounded-full bg-white/10 backdrop-blur-sm text-white text-sm font-medium">
                                {selectedIndex + 1} / {images.length}
                            </span>
                            <div className="flex items-center gap-2">
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setZoomLevel((prev) => Math.max(prev - 0.5, 1));
                                    }}
                                    className="w-10 h-10 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faMagnifyingGlassMinus} className="w-4 h-4" />
                                </motion.button>
                                <span className="px-3 py-1 rounded-full bg-white/10 text-white text-sm">
                                    {Math.round(zoomLevel * 100)}%
                                </span>
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setZoomLevel((prev) => Math.min(prev + 0.5, 3));
                                    }}
                                    className="w-10 h-10 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faMagnifyingGlassPlus} className="w-4 h-4" />
                                </motion.button>
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setZoomLevel(1);
                                    }}
                                    className="w-10 h-10 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faRotateRight} className="w-4 h-4" />
                                </motion.button>
                                <motion.button
                                    whileHover={{ scale: 1.1 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setIsLightboxOpen(false);
                                        setZoomLevel(1);
                                    }}
                                    className="w-10 h-10 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faXmark} className="w-5 h-5" />
                                </motion.button>
                            </div>
                        </div>

                        <motion.div
                            className="relative w-full h-full max-w-6xl max-h-[80vh] mx-4 overflow-hidden"
                            onClick={(e) => e.stopPropagation()}
                        >
                            <motion.div
                                drag={zoomLevel > 1}
                                dragConstraints={{ left: -200, right: 200, top: -200, bottom: 200 }}
                                style={{ scale: zoomLevel }}
                                className="w-full h-full relative cursor-grab active:cursor-grabbing"
                            >
                                <Image
                                    src={currentImage}
                                    alt={title}
                                    fill
                                    className="object-contain"
                                    quality={100}
                                />
                            </motion.div>
                        </motion.div>

                        {hasMultipleImages && (
                            <>
                                <motion.button
                                    whileHover={{ scale: 1.1, x: -5 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        navigateImage("prev");
                                    }}
                                    className="absolute left-4 top-1/2 -translate-y-1/2 w-14 h-14 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faChevronLeft} className="w-6 h-6" />
                                </motion.button>
                                <motion.button
                                    whileHover={{ scale: 1.1, x: 5 }}
                                    whileTap={{ scale: 0.9 }}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        navigateImage("next");
                                    }}
                                    className="absolute right-4 top-1/2 -translate-y-1/2 w-14 h-14 rounded-full bg-white/10 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={faChevronRight} className="w-6 h-6" />
                                </motion.button>
                            </>
                        )}

                        {hasMultipleImages && (
                            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2 p-2 rounded-xl bg-white/10 backdrop-blur-sm">
                                {images.map((img, idx) => (
                                    <motion.button
                                        key={idx}
                                        whileHover={{ scale: 1.1 }}
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            setSelectedIndex(idx);
                                            setZoomLevel(1);
                                        }}
                                        className={cn(
                                            "relative w-16 h-16 rounded-lg overflow-hidden transition-all",
                                            selectedIndex === idx
                                                ? "ring-2 ring-white"
                                                : "opacity-50 hover:opacity-100"
                                        )}
                                    >
                                        <Image
                                            src={img}
                                            alt={`Thumbnail ${idx + 1}`}
                                            fill
                                            className="object-cover"
                                        />
                                    </motion.button>
                                ))}
                            </div>
                        )}

                        <div className="absolute bottom-20 left-1/2 -translate-x-1/2 text-white/60 text-sm">
                            Use arrow keys to navigate • Press ESC to close • +/- to zoom
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </>
    );
}
