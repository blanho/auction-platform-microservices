"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useNotifications } from "@/context/notification.context";
import { Notification, NotificationType, NotificationStatus } from "@/types/notification";
import { formatDistanceToNow, format } from "date-fns";
import { cn } from "@/lib/utils";
import Link from "next/link";
import {
    Bell,
    Check,
    CheckCheck,
    Trash2,
    ExternalLink,
    Loader2
} from "lucide-react";
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

const notificationTypeIcons: Record<NotificationType, IconDefinition> = {
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

export default function NotificationsPage() {
    const { status } = useSession();
    const router = useRouter();
    const [activeTab, setActiveTab] = useState<"all" | "unread">("all");
    const {
        notifications,
        unreadCount,
        isLoading,
        fetchNotifications,
        markAsRead,
        markAllAsRead,
        deleteNotification
    } = useNotifications();

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push("/auth/signin?callbackUrl=/notifications");
        } else if (status === "authenticated") {
            fetchNotifications();
        }
    }, [status, router, fetchNotifications]);

    const filteredNotifications = activeTab === "unread"
        ? notifications.filter(n => n.status === NotificationStatus.Unread)
        : notifications;

    if (status === "loading" || isLoading) {
        return (
            <MainLayout>
                <div className="container mx-auto px-4 py-8">
                    <div className="flex items-center justify-center h-64">
                        <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
                    </div>
                </div>
            </MainLayout>
        );
    }

    return (
        <MainLayout>
            <div className="container mx-auto px-4 py-8">
                <div className="max-w-3xl mx-auto">
                    <div className="flex items-center justify-between mb-6">
                        <div>
                            <h1 className="text-2xl font-bold">Notifications</h1>
                            <p className="text-gray-500 text-sm mt-1">
                                {unreadCount > 0
                                    ? `You have ${unreadCount} unread notification${unreadCount > 1 ? 's' : ''}`
                                    : 'All caught up!'}
                            </p>
                        </div>
                        {unreadCount > 0 && (
                            <Button variant="outline" size="sm" onClick={() => markAllAsRead()}>
                                <CheckCheck className="h-4 w-4 mr-2" />
                                Mark all as read
                            </Button>
                        )}
                    </div>

                    <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as "all" | "unread")}>
                        <TabsList className="mb-4">
                            <TabsTrigger value="all">
                                All ({notifications.length})
                            </TabsTrigger>
                            <TabsTrigger value="unread">
                                Unread ({unreadCount})
                            </TabsTrigger>
                        </TabsList>

                        <TabsContent value={activeTab}>
                            {filteredNotifications.length === 0 ? (
                                <Card>
                                    <CardContent className="flex flex-col items-center justify-center py-12">
                                        <Bell className="h-12 w-12 text-gray-300 mb-4" />
                                        <p className="text-gray-500">
                                            {activeTab === "unread"
                                                ? "No unread notifications"
                                                : "No notifications yet"}
                                        </p>
                                    </CardContent>
                                </Card>
                            ) : (
                                <div className="space-y-3">
                                    {filteredNotifications.map((notification) => (
                                        <NotificationCard
                                            key={notification.id}
                                            notification={notification}
                                            onMarkAsRead={markAsRead}
                                            onDelete={deleteNotification}
                                        />
                                    ))}
                                </div>
                            )}
                        </TabsContent>
                    </Tabs>
                </div>
            </div>
        </MainLayout>
    );
}

function NotificationCard({
    notification,
    onMarkAsRead,
    onDelete
}: {
    notification: Notification;
    onMarkAsRead: (id: string) => void;
    onDelete: (id: string) => void;
}) {
    const isUnread = notification.status === NotificationStatus.Unread;
    const icon = notificationTypeIcons[notification.type as NotificationType] || faEnvelope;

    return (
        <Card className={cn(
            "transition-all",
            isUnread && "border-l-4 border-l-blue-500 bg-blue-50/50 dark:bg-blue-950/20"
        )}>
            <CardContent className="p-4">
                <div className="flex items-start gap-4">
                    <div className="w-10 h-10 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
                        <FontAwesomeIcon icon={icon} className="h-5 w-5 text-gray-600 dark:text-gray-300" />
                    </div>
                    <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-2">
                            <div>
                                <h3 className={cn("font-medium", isUnread && "font-semibold")}>
                                    {notification.title}
                                </h3>
                                <p className="text-sm text-gray-600 dark:text-gray-300 mt-1">
                                    {notification.message}
                                </p>
                            </div>
                            {isUnread && (
                                <Badge variant="secondary" className="shrink-0">New</Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between mt-3">
                            <span className="text-xs text-gray-400">
                                {format(new Date(notification.createdAt), "MMM d, yyyy 'at' h:mm a")}
                                {' · '}
                                {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                            </span>
                            <div className="flex items-center gap-1">
                                {notification.auctionId && (
                                    <Button variant="ghost" size="sm" asChild>
                                        <Link href={`/auctions/${notification.auctionId}`}>
                                            <ExternalLink className="h-4 w-4 mr-1" />
                                            View Auction
                                        </Link>
                                    </Button>
                                )}
                                {isUnread && (
                                    <Button
                                        variant="ghost"
                                        size="sm"
                                        onClick={() => onMarkAsRead(notification.id)}
                                    >
                                        <Check className="h-4 w-4 mr-1" />
                                        Mark as read
                                    </Button>
                                )}
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="text-red-500 hover:text-red-600"
                                    onClick={() => onDelete(notification.id)}
                                >
                                    <Trash2 className="h-4 w-4" />
                                </Button>
                            </div>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
