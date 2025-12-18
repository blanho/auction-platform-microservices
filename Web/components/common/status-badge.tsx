"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faBolt,
    faCircleCheck,
    faTag,
    faStar,
    faFire,
    faRocket,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { AuctionStatus } from "@/types/auction";

type StatusBadgeType = "status" | "featured" | "hot" | "new" | "ending-soon";

interface StatusBadgeProps {
    type: StatusBadgeType;
    status?: string;
    className?: string;
}

interface BadgeConfig {
    bg: string;
    text: string;
    icon: IconDefinition | null;
    pulse?: boolean;
}

function getStatusConfig(status: string): BadgeConfig {
    const statusUpper = status.toUpperCase();
    switch (statusUpper) {
        case "LIVE":
        case AuctionStatus.Live.toUpperCase():
            return {
                bg: "bg-gradient-to-r from-green-500 to-emerald-500",
                text: "Live",
                icon: faBolt,
                pulse: true,
            };
        case "FINISHED":
        case AuctionStatus.Finished.toUpperCase():
            return {
                bg: "bg-slate-500",
                text: "Ended",
                icon: faCircleCheck,
                pulse: false,
            };
        case "RESERVENOTMET":
        case "RESERVE_NOT_MET":
        case AuctionStatus.ReserveNotMet.toUpperCase():
            return {
                bg: "bg-gradient-to-r from-amber-500 to-yellow-500",
                text: "Reserve Not Met",
                icon: faTag,
                pulse: false,
            };
        case "CANCELLED":
        case AuctionStatus.Cancelled.toUpperCase():
            return {
                bg: "bg-red-500",
                text: "Cancelled",
                icon: null,
                pulse: false,
            };
        default:
            return {
                bg: "bg-slate-500",
                text: status,
                icon: null,
                pulse: false,
            };
    }
}

const BADGE_CONFIGS: Record<Exclude<StatusBadgeType, "status">, BadgeConfig> = {
    featured: {
        bg: "bg-gradient-to-r from-purple-500 to-pink-500",
        text: "Featured",
        icon: faStar,
    },
    hot: {
        bg: "bg-gradient-to-r from-amber-500 to-orange-500",
        text: "Hot",
        icon: faFire,
    },
    new: {
        bg: "bg-gradient-to-r from-emerald-500 to-teal-500",
        text: "New",
        icon: faRocket,
    },
    "ending-soon": {
        bg: "bg-gradient-to-r from-red-500 to-orange-500",
        text: "Ending Soon",
        icon: null,
        pulse: true,
    },
};

export function StatusBadge({ type, status, className }: StatusBadgeProps) {
    const config = type === "status" && status ? getStatusConfig(status) : BADGE_CONFIGS[type as Exclude<StatusBadgeType, "status">];

    if (type === "status" && status) {
        return (
            <span
                className={cn(
                    "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold text-white shadow-lg",
                    config.bg,
                    config.pulse && "animate-pulse",
                    className
                )}
            >
                {config.icon && <FontAwesomeIcon icon={config.icon} className="w-3 h-3" />}
                {config.text}
            </span>
        );
    }

    if (type === "ending-soon") {
        return (
            <Badge className={cn(config.bg, "text-white px-3 py-1 text-xs border-0 shadow-lg", className)}>
                <span className="w-1.5 h-1.5 bg-white rounded-full animate-pulse mr-1.5" />
                {config.text}
            </Badge>
        );
    }

    return (
        <Badge className={cn(config.bg, "text-white px-3 py-1 text-xs border-0 shadow-lg", className)}>
            {config.icon && <FontAwesomeIcon icon={config.icon} className="w-3 h-3 mr-1" />}
            {config.text}
        </Badge>
    );
}

export { getStatusConfig };
export type { BadgeConfig };
