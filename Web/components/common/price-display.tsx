"use client";

import { cn } from "@/lib/utils";

type PriceDisplayVariant = "default" | "gradient" | "large";

interface PriceDisplayProps {
    amount: number;
    label?: string;
    variant?: PriceDisplayVariant;
    gradientColors?: string;
    className?: string;
}

const variantStyles: Record<PriceDisplayVariant, { price: string; label: string }> = {
    default: {
        price: "text-xl font-bold text-slate-900 dark:text-white",
        label: "text-xs text-slate-500 dark:text-slate-400 mb-0.5",
    },
    gradient: {
        price: "text-2xl font-bold bg-clip-text text-transparent",
        label: "text-xs text-slate-500 dark:text-slate-400 mb-0.5",
    },
    large: {
        price: "text-3xl font-bold text-slate-900 dark:text-white",
        label: "text-sm text-slate-500 dark:text-slate-400 mb-1",
    },
};

export function PriceDisplay({
    amount,
    label,
    variant = "default",
    gradientColors = "bg-linear-to-r from-purple-600 to-pink-600",
    className,
}: PriceDisplayProps) {
    const styles = variantStyles[variant];
    const formattedPrice = `$${(amount || 0).toLocaleString()}`;

    return (
        <div className={className}>
            {label && <p className={styles.label}>{label}</p>}
            <p className={cn(styles.price, variant === "gradient" && gradientColors)}>
                {formattedPrice}
            </p>
        </div>
    );
}
