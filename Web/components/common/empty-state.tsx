"use client";

import { ReactNode } from "react";
import { motion } from "framer-motion";
import { cn } from "@/lib/utils";
import { cards } from "@/lib/styles";

interface EmptyStateProps {
    icon?: ReactNode;
    title: string;
    description?: string;
    action?: ReactNode;
    className?: string;
}

export function EmptyState({
    icon,
    title,
    description,
    action,
    className,
}: EmptyStateProps) {
    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className={cn(
                "flex flex-col items-center justify-center rounded-2xl border border-dashed border-slate-300 dark:border-slate-700 p-12 text-center",
                "bg-slate-50/50 dark:bg-slate-900/50",
                className
            )}
        >
            {icon && (
                <motion.div
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                    transition={{ delay: 0.2, duration: 0.3 }}
                    className="mb-4 text-slate-400 dark:text-slate-500"
                >
                    {icon}
                </motion.div>
            )}
            <motion.h3
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.3 }}
                className="mb-2 text-lg font-semibold text-slate-900 dark:text-white"
            >
                {title}
            </motion.h3>
            {description && (
                <motion.p
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.4 }}
                    className="mb-4 text-sm text-slate-500 dark:text-slate-400 max-w-sm"
                >
                    {description}
                </motion.p>
            )}
            {action && (
                <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.5 }}
                >
                    {action}
                </motion.div>
            )}
        </motion.div>
    );
}
