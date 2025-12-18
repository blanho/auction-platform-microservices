"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import type { AuctionCardVariant } from "./types";

interface AuctionCardSkeletonProps {
    variant?: AuctionCardVariant;
    className?: string;
}

export function AuctionCardSkeleton({ variant = "default", className }: AuctionCardSkeletonProps) {
    if (variant === "compact") {
        return (
            <div className={cn("flex gap-3 p-3 bg-white dark:bg-slate-900 rounded-xl border border-slate-200/80 dark:border-slate-800/80", className)}>
                <Skeleton className="w-20 h-20 rounded-lg flex-shrink-0" />
                <div className="flex-1 space-y-2">
                    <Skeleton className="h-4 w-3/4" />
                    <Skeleton className="h-6 w-20" />
                    <Skeleton className="h-3 w-16" />
                </div>
            </div>
        );
    }

    if (variant === "carousel") {
        return (
            <div className={cn("shrink-0 w-80 snap-start", className)}>
                <div className="bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800">
                    <Skeleton className="h-52 w-full" />
                    <div className="p-5 space-y-4">
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-6 w-full" />
                        <div className="flex justify-between pt-4">
                            <Skeleton className="h-10 w-24" />
                            <Skeleton className="h-10 w-20 rounded-xl" />
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (variant === "featured") {
        return (
            <div className={cn("bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800", className)}>
                <Skeleton className="h-56 w-full" />
                <div className="p-5 space-y-4">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-6 w-full" />
                    <div className="flex justify-between pt-4">
                        <Skeleton className="h-10 w-24" />
                        <Skeleton className="h-10 w-28 rounded-xl" />
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className={cn("bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800/80 overflow-hidden", className)}>
            <Skeleton className="aspect-[4/3] w-full" />
            <div className="p-4 space-y-3">
                <div className="space-y-2">
                    <Skeleton className="h-5 w-3/4" />
                    <Skeleton className="h-4 w-1/2" />
                </div>
                <div className="flex items-end justify-between">
                    <div className="space-y-1">
                        <Skeleton className="h-3 w-16" />
                        <Skeleton className="h-7 w-24" />
                    </div>
                    <Skeleton className="h-6 w-20" />
                </div>
                <div className="flex gap-2">
                    <Skeleton className="h-5 w-16 rounded-md" />
                    <Skeleton className="h-5 w-20 rounded-md" />
                </div>
            </div>
        </div>
    );
}
