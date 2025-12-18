"use client";

import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faHeart } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";

type WishlistIconVariant = "circle" | "square" | "minimal";

interface WishlistIconButtonProps {
    isWishlisted: boolean;
    onClick: (e: React.MouseEvent) => void;
    variant?: WishlistIconVariant;
    className?: string;
}

const variantStyles: Record<WishlistIconVariant, { base: string; active: string; inactive: string }> = {
    circle: {
        base: "w-10 h-10 rounded-full shadow-lg flex items-center justify-center hover:scale-110 transition-transform",
        active: "bg-red-500 text-white",
        inactive: "bg-white/90 dark:bg-slate-800/90 text-slate-400 dark:text-slate-500",
    },
    square: {
        base: "w-9 h-9 rounded-full flex items-center justify-center transition-all shadow-lg",
        active: "bg-red-500 text-white",
        inactive: "bg-white/90 dark:bg-slate-800/90 text-slate-400 dark:text-slate-500 hover:text-red-500 hover:bg-white dark:hover:bg-slate-800",
    },
    minimal: {
        base: "p-2 rounded-full transition-colors",
        active: "text-red-500",
        inactive: "text-slate-400 hover:text-red-500",
    },
};

export function WishlistIconButton({
    isWishlisted,
    onClick,
    variant = "circle",
    className,
}: WishlistIconButtonProps) {
    const styles = variantStyles[variant];

    if (variant === "square") {
        return (
            <motion.button
                whileHover={{ scale: 1.1 }}
                whileTap={{ scale: 0.9 }}
                onClick={onClick}
                className={cn(
                    styles.base,
                    isWishlisted ? styles.active : styles.inactive,
                    className
                )}
            >
                <FontAwesomeIcon
                    icon={faHeart}
                    className={cn("w-4 h-4", isWishlisted && "animate-pulse")}
                />
            </motion.button>
        );
    }

    return (
        <button
            onClick={onClick}
            className={cn(
                styles.base,
                isWishlisted ? styles.active : styles.inactive,
                className
            )}
        >
            <FontAwesomeIcon
                icon={faHeart}
                className={cn(
                    "w-4 h-4 transition-colors",
                    isWishlisted ? "text-red-500" : "text-slate-400 dark:text-slate-500"
                )}
            />
        </button>
    );
}
