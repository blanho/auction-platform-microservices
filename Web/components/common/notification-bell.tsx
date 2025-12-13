"use client";

import { useState, useCallback } from "react";
import Link from "next/link";
import { motion, AnimatePresence } from "framer-motion";
import { Bell, Check, CheckCheck, Trash2, ExternalLink } from "lucide-react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faGift,
    faPenToSquare,
    faTrashCan,
    faFlagCheckered,
    faMoneyBillWave,
    faCircleCheck,
    faTriangleExclamation,
    faTrophy,
    faClock,
    faEnvelope,
    IconDefinition
} from "@fortawesome/free-solid-svg-icons";
import { formatDistanceToNow } from "date-fns";
import { Button } from "@/components/ui/button";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { useNotifications } from "@/context/notification.context";
import { cn } from "@/lib/utils";
import { Notification, NotificationType, NotificationStatus } from "@/types/notification";

const NOTIFICATION_ICONS: Record<NotificationType, IconDefinition> = {
    [NotificationType.AuctionCreated]: faGift,
    [NotificationType.AuctionUpdated]: faPenToSquare,
    [NotificationType.AuctionDeleted]: faTrashCan,
    [NotificationType.AuctionFinished]: faFlagCheckered,
    [NotificationType.BidPlaced]: faMoneyBillWave,
    [NotificationType.BidAccepted]: faCircleCheck,
    [NotificationType.OutBid]: faTriangleExclamation,
    [NotificationType.AuctionWon]: faTrophy,
    [NotificationType.AuctionEndingSoon]: faClock
};

const NOTIFICATION_COLORS: Record<NotificationType, string> = {
    [NotificationType.AuctionCreated]: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
    [NotificationType.AuctionUpdated]: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
    [NotificationType.AuctionDeleted]: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
    [NotificationType.AuctionFinished]: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
    [NotificationType.BidPlaced]: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
    [NotificationType.BidAccepted]: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
    [NotificationType.OutBid]: "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200",
    [NotificationType.AuctionWon]: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-200",
    [NotificationType.AuctionEndingSoon]: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
};

const DEFAULT_ICON = faEnvelope;
const DEFAULT_COLOR = "bg-gray-100 text-gray-800";
const MAX_BADGE_COUNT = 99;

function getNotificationIcon(type: NotificationType): IconDefinition {
    return NOTIFICATION_ICONS[type] || DEFAULT_ICON;
}

function getNotificationColor(type: NotificationType): string {
    return NOTIFICATION_COLORS[type] || DEFAULT_COLOR;
}

interface NotificationItemProps {
    notification: Notification;
    onMarkAsRead: (id: string) => void;
    onDelete: (id: string) => void;
}

function NotificationItem({ notification, onMarkAsRead, onDelete }: NotificationItemProps) {
    const isUnread = notification.status === NotificationStatus.Unread;
    const icon = getNotificationIcon(notification.type as NotificationType);
    const colorClass = getNotificationColor(notification.type as NotificationType);

    return (
        <motion.div
            initial={{ opacity: 0, x: -10 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 10 }}
            whileHover={{ backgroundColor: "rgba(0,0,0,0.02)" }}
            transition={{ duration: 0.2 }}
            className={cn(
                "p-3 border-b border-gray-100 dark:border-gray-800",
                "transition-colors",
                isUnread && "bg-blue-50/50 dark:bg-blue-950/20"
            )}
        >
            <div className="flex items-start gap-3">
                <motion.div
                    initial={{ scale: 0.8 }}
                    animate={{ scale: 1 }}
                    className={cn(
                        "w-8 h-8 rounded-full flex items-center justify-center shrink-0",
                        colorClass
                    )}
                >
                    <FontAwesomeIcon icon={icon} className="h-4 w-4" />
                </motion.div>
                <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                        <div>
                            <p className={cn("text-sm font-medium text-slate-900 dark:text-white", isUnread && "font-semibold")}>
                                {notification.title}
                            </p>
                            <p className="text-xs text-gray-500 dark:text-gray-400 line-clamp-2 mt-0.5">
                                {notification.message}
                            </p>
                        </div>
                        {isUnread && (
                            <motion.div
                                initial={{ scale: 0 }}
                                animate={{ scale: 1 }}
                                className="w-2 h-2 rounded-full bg-blue-500 shrink-0 mt-1.5"
                            />
                        )}
                    </div>
                    <div className="flex items-center justify-between mt-2">
                        <span className="text-xs text-gray-400 dark:text-gray-500">
                            {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                        </span>
                        <div className="flex items-center gap-1">
                            {notification.auctionId && (
                                <Link href={`/auctions/${notification.auctionId}`}>
                                    <Button variant="ghost" size="icon" className="h-6 w-6">
                                        <ExternalLink className="h-3 w-3" />
                                    </Button>
                                </Link>
                            )}
                            {isUnread && (
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="h-6 w-6"
                                    onClick={() => onMarkAsRead(notification.id)}
                                >
                                    <Check className="h-3 w-3" />
                                </Button>
                            )}
                            <Button
                                variant="ghost"
                                size="icon"
                                className="h-6 w-6 text-red-500 hover:text-red-600"
                                onClick={() => onDelete(notification.id)}
                            >
                                <Trash2 className="h-3 w-3" />
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </motion.div>
    );
}

function LoadingState() {
    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="flex items-center justify-center h-20"
        >
            <motion.div
                animate={{ rotate: 360 }}
                transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
                className="h-5 w-5 border-2 border-gray-300 dark:border-gray-600 border-t-purple-500 rounded-full"
            />
        </motion.div>
    );
}

function EmptyState() {
    return (
        <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex flex-col items-center justify-center h-[200px] text-gray-500 dark:text-gray-400"
        >
            <motion.div
                initial={{ scale: 0.8 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.1 }}
            >
                <Bell className="h-10 w-10 mb-2 opacity-50" />
            </motion.div>
            <p className="text-sm">No notifications yet</p>
        </motion.div>
    );
}

export function NotificationBell() {
    const [open, setOpen] = useState(false);
    const {
        notifications,
        unreadCount,
        markAsRead,
        markAllAsRead,
        deleteNotification,
        isLoading
    } = useNotifications();

    const handleClose = useCallback(() => setOpen(false), []);

    const displayCount = unreadCount > MAX_BADGE_COUNT ? `${MAX_BADGE_COUNT}+` : unreadCount;

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button variant="ghost" size="icon" className="relative">
                    <Bell className="h-5 w-5" />
                    <AnimatePresence>
                        {unreadCount > 0 && (
                            <motion.div
                                initial={{ scale: 0 }}
                                animate={{ scale: 1 }}
                                exit={{ scale: 0 }}
                                transition={{ type: "spring", stiffness: 500, damping: 25 }}
                            >
                                <Badge
                                    className="absolute -top-1 -right-1 h-5 min-w-5 flex items-center justify-center p-0 text-xs"
                                    variant="destructive"
                                >
                                    {displayCount}
                                </Badge>
                            </motion.div>
                        )}
                    </AnimatePresence>
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-80 p-0 bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800" align="end">
                <div className="flex items-center justify-between p-3 border-b border-gray-100 dark:border-gray-800">
                    <h4 className="font-semibold text-slate-900 dark:text-white">Notifications</h4>
                    {unreadCount > 0 && (
                        <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={markAllAsRead}>
                            <CheckCheck className="h-3 w-3 mr-1" />
                            Mark all as read
                        </Button>
                    )}
                </div>

                <ScrollArea className="h-[300px]">
                    {isLoading ? (
                        <LoadingState />
                    ) : notifications.length === 0 ? (
                        <EmptyState />
                    ) : (
                        <AnimatePresence>
                            {notifications.map((notification, index) => (
                                <motion.div
                                    key={notification.id}
                                    initial={{ opacity: 0, y: 10 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    exit={{ opacity: 0, x: -20 }}
                                    transition={{ delay: index * 0.03 }}
                                >
                                    <NotificationItem
                                        notification={notification}
                                        onMarkAsRead={markAsRead}
                                        onDelete={deleteNotification}
                                    />
                                </motion.div>
                            ))}
                        </AnimatePresence>
                    )}
                </ScrollArea>

                {notifications.length > 0 && (
                    <>
                        <Separator />
                        <div className="p-2">
                            <Link href="/notifications" onClick={handleClose}>
                                <Button variant="ghost" size="sm" className="w-full text-xs">
                                    View all notifications
                                </Button>
                            </Link>
                        </div>
                    </>
                )}
            </PopoverContent>
        </Popover>
    );
}
