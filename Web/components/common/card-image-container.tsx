"use client";

import Image from "next/image";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGavel } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";

interface CardImageContainerProps {
    imageUrl?: string;
    alt: string;
    aspectRatio?: "4/3" | "square" | "16/9";
    showOverlay?: boolean;
    overlayDirection?: "t" | "b" | "r" | "l";
    hoverZoom?: boolean;
    className?: string;
    children?: React.ReactNode;
}

const aspectClasses = {
    "4/3": "aspect-4/3",
    square: "aspect-square",
    "16/9": "aspect-video",
};

const overlayClasses = {
    t: "bg-linear-to-t from-black/80 via-black/20 to-transparent",
    b: "bg-linear-to-b from-black/80 via-black/20 to-transparent",
    r: "bg-linear-to-r from-black/80 via-black/20 to-transparent",
    l: "bg-linear-to-l from-black/80 via-black/20 to-transparent",
};

export function CardImageContainer({
    imageUrl,
    alt,
    aspectRatio = "4/3",
    showOverlay = false,
    overlayDirection = "t",
    hoverZoom = true,
    className,
    children,
}: CardImageContainerProps) {
    return (
        <div className={cn("relative overflow-hidden bg-slate-100 dark:bg-slate-800", aspectClasses[aspectRatio], className)}>
            {imageUrl ? (
                <Image
                    src={imageUrl}
                    alt={alt}
                    fill
                    className={cn(
                        "object-cover",
                        hoverZoom && "transition-transform duration-500 group-hover:scale-110"
                    )}
                    unoptimized={imageUrl.includes("unsplash")}
                />
            ) : (
                <div className="flex h-full items-center justify-center">
                    <FontAwesomeIcon icon={faGavel} className="w-12 h-12 text-slate-300 dark:text-slate-600" />
                </div>
            )}

            {showOverlay && (
                <div className={cn("absolute inset-0", overlayClasses[overlayDirection])} />
            )}

            {children}
        </div>
    );
}
