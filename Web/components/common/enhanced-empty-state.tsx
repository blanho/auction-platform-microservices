"use client";

import { ReactNode } from "react";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faGavel,
    faSearch,
    faHeart,
    faBell,
    faShoppingCart,
    faBox,
    faUserSlash,
    faWifi,
    faExclamationTriangle,
    faPlus,
    faArrowRight,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type EmptyStateVariant =
    | "default"
    | "auctions"
    | "bids"
    | "search"
    | "wishlist"
    | "notifications"
    | "orders"
    | "error"
    | "offline"
    | "noAccess";

interface IllustrationConfig {
    icon: IconDefinition;
    gradient: string;
    bgGlow: string;
}

const ILLUSTRATIONS: Record<EmptyStateVariant, IllustrationConfig> = {
    default: {
        icon: faBox,
        gradient: "from-slate-400 to-slate-600",
        bgGlow: "bg-slate-500/10",
    },
    auctions: {
        icon: faGavel,
        gradient: "from-purple-500 to-blue-600",
        bgGlow: "bg-purple-500/10",
    },
    bids: {
        icon: faGavel,
        gradient: "from-amber-500 to-orange-600",
        bgGlow: "bg-amber-500/10",
    },
    search: {
        icon: faSearch,
        gradient: "from-blue-500 to-cyan-600",
        bgGlow: "bg-blue-500/10",
    },
    wishlist: {
        icon: faHeart,
        gradient: "from-pink-500 to-rose-600",
        bgGlow: "bg-pink-500/10",
    },
    notifications: {
        icon: faBell,
        gradient: "from-purple-500 to-pink-600",
        bgGlow: "bg-purple-500/10",
    },
    orders: {
        icon: faShoppingCart,
        gradient: "from-emerald-500 to-green-600",
        bgGlow: "bg-emerald-500/10",
    },
    error: {
        icon: faExclamationTriangle,
        gradient: "from-red-500 to-orange-600",
        bgGlow: "bg-red-500/10",
    },
    offline: {
        icon: faWifi,
        gradient: "from-slate-500 to-slate-700",
        bgGlow: "bg-slate-500/10",
    },
    noAccess: {
        icon: faUserSlash,
        gradient: "from-red-500 to-pink-600",
        bgGlow: "bg-red-500/10",
    },
};

interface EnhancedEmptyStateProps {
    variant?: EmptyStateVariant;
    title: string;
    description?: string;
    primaryAction?: {
        label: string;
        onClick: () => void;
        icon?: IconDefinition;
    };
    secondaryAction?: {
        label: string;
        onClick: () => void;
    };
    customIcon?: ReactNode;
    className?: string;
    size?: "sm" | "md" | "lg";
}

export function EnhancedEmptyState({
    variant = "default",
    title,
    description,
    primaryAction,
    secondaryAction,
    customIcon,
    className,
    size = "md",
}: EnhancedEmptyStateProps) {
    const config = ILLUSTRATIONS[variant];

    const sizeClasses = {
        sm: {
            container: "py-8 px-6",
            icon: "w-12 h-12",
            iconInner: "w-6 h-6",
            title: "text-base",
            description: "text-sm",
        },
        md: {
            container: "py-12 px-8",
            icon: "w-20 h-20",
            iconInner: "w-8 h-8",
            title: "text-lg",
            description: "text-sm",
        },
        lg: {
            container: "py-16 px-12",
            icon: "w-28 h-28",
            iconInner: "w-12 h-12",
            title: "text-xl",
            description: "text-base",
        },
    };

    const sizes = sizeClasses[size];

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className={cn(
                "flex flex-col items-center justify-center text-center rounded-2xl",
                "border border-dashed border-slate-300 dark:border-slate-700",
                "bg-linear-to-b from-slate-50/50 to-white dark:from-slate-900/50 dark:to-slate-950",
                sizes.container,
                className
            )}
        >
            <motion.div
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ delay: 0.1, type: "spring", stiffness: 200 }}
                className={cn(
                    "relative mb-6 rounded-full flex items-center justify-center",
                    sizes.icon,
                    config.bgGlow
                )}
            >
                <motion.div
                    className={cn(
                        "absolute inset-0 rounded-full bg-linear-to-br opacity-20",
                        config.gradient
                    )}
                    animate={{
                        scale: [1, 1.2, 1],
                        opacity: [0.2, 0.1, 0.2],
                    }}
                    transition={{
                        duration: 3,
                        repeat: Infinity,
                        ease: "easeInOut",
                    }}
                />

                {customIcon || (
                    <motion.div
                        animate={{
                            y: [0, -3, 0],
                        }}
                        transition={{
                            duration: 2,
                            repeat: Infinity,
                            ease: "easeInOut",
                        }}
                    >
                        <FontAwesomeIcon
                            icon={config.icon}
                            className={cn(
                                "text-transparent bg-clip-text bg-linear-to-br",
                                config.gradient,
                                sizes.iconInner
                            )}
                            style={{
                                WebkitBackgroundClip: "text",
                                WebkitTextFillColor: "transparent",
                                background: `linear-gradient(to bottom right, var(--tw-gradient-stops))`,
                            }}
                        />
                        <div
                            className={cn(
                                "bg-linear-to-br rounded-full flex items-center justify-center",
                                config.gradient,
                                sizes.icon
                            )}
                            style={{ position: "absolute", inset: 0 }}
                        >
                            <FontAwesomeIcon
                                icon={config.icon}
                                className={cn("text-white", sizes.iconInner)}
                            />
                        </div>
                    </motion.div>
                )}
            </motion.div>

            <motion.h3
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.2 }}
                className={cn(
                    "font-semibold text-slate-900 dark:text-white mb-2",
                    sizes.title
                )}
            >
                {title}
            </motion.h3>

            {description && (
                <motion.p
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.3 }}
                    className={cn(
                        "text-slate-500 dark:text-slate-400 max-w-md mb-6",
                        sizes.description
                    )}
                >
                    {description}
                </motion.p>
            )}

            {(primaryAction || secondaryAction) && (
                <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.4 }}
                    className="flex flex-wrap items-center justify-center gap-3"
                >
                    {primaryAction && (
                        <Button
                            onClick={primaryAction.onClick}
                            className={cn(
                                "bg-linear-to-r text-white shadow-lg transition-all hover:shadow-xl hover:-translate-y-0.5",
                                config.gradient
                            )}
                        >
                            {primaryAction.icon ? (
                                <FontAwesomeIcon
                                    icon={primaryAction.icon}
                                    className="w-4 h-4 mr-2"
                                />
                            ) : (
                                <FontAwesomeIcon icon={faPlus} className="w-4 h-4 mr-2" />
                            )}
                            {primaryAction.label}
                        </Button>
                    )}
                    {secondaryAction && (
                        <Button
                            variant="outline"
                            onClick={secondaryAction.onClick}
                            className="gap-2"
                        >
                            {secondaryAction.label}
                            <FontAwesomeIcon icon={faArrowRight} className="w-3 h-3" />
                        </Button>
                    )}
                </motion.div>
            )}
        </motion.div>
    );
}

export const EMPTY_STATE_PRESETS = {
    noAuctions: {
        variant: "auctions" as const,
        title: "No auctions yet",
        description:
            "Start selling by creating your first auction. It only takes a few minutes!",
    },
    noBids: {
        variant: "bids" as const,
        title: "No bids yet",
        description:
            "Be the first to place a bid on this item and get ahead of the competition!",
    },
    noSearchResults: {
        variant: "search" as const,
        title: "No results found",
        description:
            "Try adjusting your search terms or filters to find what you're looking for.",
    },
    emptyWishlist: {
        variant: "wishlist" as const,
        title: "Your wishlist is empty",
        description:
            "Save items you love by clicking the heart icon. They'll appear here for easy access.",
    },
    noNotifications: {
        variant: "notifications" as const,
        title: "All caught up!",
        description:
            "You have no new notifications. We'll let you know when something important happens.",
    },
    noOrders: {
        variant: "orders" as const,
        title: "No orders yet",
        description:
            "When you win an auction or make a purchase, your orders will appear here.",
    },
    loadError: {
        variant: "error" as const,
        title: "Something went wrong",
        description:
            "We couldn't load this content. Please try again or contact support if the problem persists.",
    },
    offline: {
        variant: "offline" as const,
        title: "You're offline",
        description:
            "Check your internet connection and try again. Some features may be unavailable.",
    },
    noAccess: {
        variant: "noAccess" as const,
        title: "Access denied",
        description:
            "You don't have permission to view this content. Please sign in or contact support.",
    },
};
