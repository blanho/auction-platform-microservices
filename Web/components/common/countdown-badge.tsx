"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faClock } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";

type CountdownBadgeVariant = "overlay" | "inline" | "minimal";

interface CountdownBadgeProps {
    timeText: string;
    isUrgent?: boolean;
    variant?: CountdownBadgeVariant;
    className?: string;
}

const variantStyles: Record<CountdownBadgeVariant, { base: string; urgent: string; normal: string }> = {
    overlay: {
        base: "inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-semibold backdrop-blur-md shadow-lg",
        urgent: "bg-red-500/90 text-white",
        normal: "bg-white/90 dark:bg-slate-800/90 text-slate-700 dark:text-slate-200",
    },
    inline: {
        base: "bg-black/60 backdrop-blur-md rounded-xl px-3 py-2 flex items-center gap-2",
        urgent: "ring-1 ring-red-500/50",
        normal: "",
    },
    minimal: {
        base: "text-xs",
        urgent: "text-red-500 font-medium",
        normal: "text-slate-500",
    },
};

export function CountdownBadge({
    timeText,
    isUrgent = false,
    variant = "overlay",
    className,
}: CountdownBadgeProps) {
    const styles = variantStyles[variant];

    if (variant === "inline") {
        return (
            <div className={cn(styles.base, isUrgent ? styles.urgent : styles.normal, className)}>
                <FontAwesomeIcon
                    icon={faClock}
                    className={cn("w-3.5 h-3.5", isUrgent ? "text-red-400 animate-pulse" : "text-white")}
                />
                <span className={cn("font-mono text-sm font-bold", isUrgent ? "text-red-400" : "text-white")}>
                    {timeText}
                </span>
            </div>
        );
    }

    if (variant === "minimal") {
        return (
            <p className={cn(styles.base, isUrgent ? styles.urgent : styles.normal, className)}>
                {timeText}
            </p>
        );
    }

    return (
        <div className={cn(styles.base, isUrgent ? styles.urgent : styles.normal, className)}>
            <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
            {timeText}
        </div>
    );
}
