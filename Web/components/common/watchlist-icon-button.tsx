"use client";

import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faEye } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";

type WatchlistIconVariant = "circle" | "square" | "minimal";

interface WatchlistIconButtonProps {
    isWatched: boolean;
    onClick: (e: React.MouseEvent) => void;
    variant?: WatchlistIconVariant;
    className?: string;
}

const variantStyles: Record<WatchlistIconVariant, { base: string; active: string; inactive: string }> = {
    circle: {
        base: "w-10 h-10 rounded-full shadow-lg flex items-center justify-center hover:scale-110 transition-transform",
        active: "bg-white/90 dark:bg-slate-800/90 text-blue-500",
        inactive: "bg-white/90 dark:bg-slate-800/90 text-slate-400 dark:text-slate-500 hover:text-blue-400",
    },
    square: {
        base: "w-9 h-9 rounded-full flex items-center justify-center transition-all shadow-lg",
        active: "bg-white/90 dark:bg-slate-800/90 text-blue-500",
        inactive: "bg-white/90 dark:bg-slate-800/90 text-slate-400 dark:text-slate-500 hover:text-blue-500 hover:bg-white dark:hover:bg-slate-800",
    },
    minimal: {
        base: "p-2 rounded-full transition-colors",
        active: "text-blue-500",
        inactive: "text-slate-400 hover:text-blue-500",
    },
};

export function WatchlistIconButton({
    isWatched,
    onClick,
    variant = "circle",
    className,
}: WatchlistIconButtonProps) {
    const styles = variantStyles[variant];

    if (variant === "square") {
        return (
            <motion.button
                whileHover={{ scale: 1.1 }}
                whileTap={{ scale: 0.9 }}
                onClick={onClick}
                className={cn(
                    styles.base,
                    isWatched ? styles.active : styles.inactive,
                    className
                )}
            >
                <FontAwesomeIcon
                    icon={faEye}
                    className={cn("w-4 h-4", isWatched && "animate-pulse")}
                />
            </motion.button>
        );
    }

    return (
        <button
            onClick={onClick}
            className={cn(
                styles.base,
                isWatched ? styles.active : styles.inactive,
                className
            )}
        >
            <FontAwesomeIcon
                icon={faEye}
                className="w-4 h-4 transition-colors"
            />
        </button>
    );
}
