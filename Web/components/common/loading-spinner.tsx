"use client";

import { motion } from "framer-motion";
import { cn } from "@/lib/utils";

interface LoadingSpinnerProps {
    className?: string;
    size?: "sm" | "md" | "lg";
    label?: string;
}

const sizeClasses = {
    sm: "h-4 w-4 border-2",
    md: "h-8 w-8 border-3",
    lg: "h-12 w-12 border-4",
};

export function LoadingSpinner({ className, size = "md", label }: LoadingSpinnerProps) {
    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className={cn("flex flex-col items-center justify-center gap-3", className)}
        >
            <motion.div
                animate={{ rotate: 360 }}
                transition={{
                    duration: 1,
                    repeat: Infinity,
                    ease: "linear",
                }}
                className={cn(
                    "rounded-full border-purple-500/30 border-t-purple-500 dark:border-purple-400/30 dark:border-t-purple-400",
                    sizeClasses[size]
                )}
            />
            {label && (
                <motion.span
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.2 }}
                    className="text-sm text-slate-500 dark:text-slate-400"
                >
                    {label}
                </motion.span>
            )}
        </motion.div>
    );
}
