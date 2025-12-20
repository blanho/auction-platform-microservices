"use client";

import { useState } from "react";
import Image from "next/image";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChevronLeft, faChevronRight, faFire } from "@fortawesome/free-solid-svg-icons";
import { AuctionStatus } from "@/types/auction";
import { getAuctionStatusStyle } from "@/constants/status";

interface AuctionGalleryProps {
    images: string[];
    title: string;
    status: AuctionStatus;
    isUrgent?: boolean;
}

export function AuctionGallery({ images, title, status, isUrgent }: AuctionGalleryProps) {
    const [selectedImageIndex, setSelectedImageIndex] = useState(0);
    const statusConfig = getAuctionStatusStyle(status);
    const currentImage = images[selectedImageIndex] || "/placeholder-car.jpg";

    const navigateImage = (direction: "prev" | "next") => {
        if (direction === "prev") {
            setSelectedImageIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
        } else {
            setSelectedImageIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
        }
    };

    return (
        <div className="space-y-4">
            <div className="relative rounded-2xl overflow-hidden bg-slate-100 dark:bg-slate-900 shadow-xl">
                <div className="relative aspect-[4/3] w-full group">
                    <Image
                        src={currentImage}
                        alt={title}
                        fill
                        className="object-cover transition-transform duration-500 group-hover:scale-105"
                        priority
                    />

                    <div
                        className={`absolute top-4 left-4 px-4 py-2 rounded-full bg-linear-to-r ${statusConfig.gradient} text-white text-sm font-semibold shadow-lg ${statusConfig.bgGlow} flex items-center gap-2`}
                    >
                        {statusConfig.pulse && <span className="w-2 h-2 rounded-full bg-white animate-pulse" />}
                        {status}
                    </div>

                    {status === AuctionStatus.Live && isUrgent && (
                        <div className="absolute top-4 right-4 px-3 py-1.5 rounded-full bg-linear-to-r from-red-500 to-orange-500 text-white text-xs font-semibold shadow-lg animate-pulse flex items-center gap-1.5">
                            <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                            Ending Soon
                        </div>
                    )}

                    {images.length > 1 && (
                        <>
                            <button
                                onClick={() => navigateImage("prev")}
                                className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all hover:scale-110"
                            >
                                <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 text-slate-700 dark:text-white" />
                            </button>
                            <button
                                onClick={() => navigateImage("next")}
                                className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all hover:scale-110"
                            >
                                <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4 text-slate-700 dark:text-white" />
                            </button>
                        </>
                    )}

                    <div className="absolute bottom-4 left-4 px-3 py-1.5 rounded-full bg-black/50 backdrop-blur-sm text-white text-xs font-medium">
                        {selectedImageIndex + 1} / {images.length || 1}
                    </div>
                </div>
            </div>

            {images.length > 1 && (
                <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-thin">
                    {images.map((img, idx) => (
                        <motion.button
                            key={idx}
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                            onClick={() => setSelectedImageIndex(idx)}
                            className={`relative w-20 h-20 shrink-0 rounded-xl overflow-hidden transition-all ${
                                selectedImageIndex === idx
                                    ? "ring-2 ring-purple-500 ring-offset-2 ring-offset-white dark:ring-offset-slate-900"
                                    : "opacity-60 hover:opacity-100"
                            }`}
                        >
                            <Image src={img} alt={`${title} - Image ${idx + 1}`} fill className="object-cover" />
                        </motion.button>
                    ))}
                </div>
            )}
        </div>
    );
}
