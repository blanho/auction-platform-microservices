"use client";

import { createContext, useContext, useEffect, useCallback, useState, ReactNode } from "react";
import { useSession } from "next-auth/react";
import { useSignalR } from "@/hooks/use-signalr";
import { notificationService } from "@/services/notification.service";
import { Notification, NotificationStatus } from "@/types/notification";
import { toast } from "sonner";


interface NotificationContextValue {

    isConnected: boolean;

    isConnecting: boolean;

    unreadCount: number;

    notifications: Notification[];

    isLoading: boolean;

    error: string | null;

    fetchNotifications: () => Promise<void>;

    markAsRead: (id: string) => Promise<void>;

    markAllAsRead: () => Promise<void>;

    deleteNotification: (id: string) => Promise<void>;
}

interface NotificationProviderProps {
    children: ReactNode;
}

const NotificationContext = createContext<NotificationContextValue | null>(null);

export function useNotifications(): NotificationContextValue {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error("useNotifications must be used within NotificationProvider");
    }
    return context;
}

export function NotificationProvider({ children }: NotificationProviderProps) {
    const { status } = useSession();

    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchNotifications = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await notificationService.getNotifications();
            setNotifications(data);
            setUnreadCount(data.filter((n) => n.status === NotificationStatus.Unread).length);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Failed to fetch notifications";
            setError(message);
        } finally {
            setIsLoading(false);
        }
    }, []);

    const fetchSummary = useCallback(async () => {
        try {
            const summary = await notificationService.getSummary();
            setNotifications(summary.recentNotifications);
            setUnreadCount(summary.unreadCount);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Failed to fetch summary";
            setError(message);
        }
    }, []);

    const addNotification = useCallback((notification: Notification) => {
        setNotifications((prev) => [notification, ...prev]);
        if (notification.status === NotificationStatus.Unread) {
            setUnreadCount((prev) => prev + 1);
        }
    }, []);

    const markAsRead = useCallback(async (id: string) => {
        try {
            await notificationService.markAsRead(id);
            setNotifications((prev) =>
                prev.map((n) =>
                    n.id === id
                        ? { ...n, status: NotificationStatus.Read, readAt: new Date().toISOString() }
                        : n
                )
            );
            setUnreadCount((prev) => Math.max(0, prev - 1));
        } catch (err) {
            const message = err instanceof Error ? err.message : "Failed to mark as read";
            setError(message);
        }
    }, []);

    const markAllAsRead = useCallback(async () => {
        try {
            await notificationService.markAllAsRead();
            setNotifications((prev) =>
                prev.map((n) => ({
                    ...n,
                    status: NotificationStatus.Read,
                    readAt: n.readAt || new Date().toISOString()
                }))
            );
            setUnreadCount(0);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Failed to mark all as read";
            setError(message);
        }
    }, []);

    const deleteNotification = useCallback(async (id: string) => {
        try {
            await notificationService.deleteNotification(id);
            setNotifications((prev) => {
                const notification = prev.find((n) => n.id === id);
                if (notification?.status === NotificationStatus.Unread) {
                    setUnreadCount((count) => Math.max(0, count - 1));
                }
                return prev.filter((n) => n.id !== id);
            });
        } catch (err) {
            const message = err instanceof Error ? err.message : "Failed to delete notification";
            setError(message);
        }
    }, []);

    const handleNotification = useCallback((notification: Notification) => {
        addNotification(notification);

        toast(notification.title, {
            description: notification.message,
            action: notification.auctionId ? {
                label: "View",
                onClick: () => {
                    window.location.href = `/auctions/${notification.auctionId}`;
                }
            } : undefined
        });
    }, [addNotification]);

    const handleReconnected = useCallback(() => {
        fetchSummary();
    }, [fetchSummary]);

    const { isConnected, isConnecting } = useSignalR({
        onNotification: handleNotification,
        onConnected: () => {
            console.log("[NotificationProvider] SignalR connected");
        },
        onDisconnected: () => {
            console.log("[NotificationProvider] SignalR disconnected");
        },
        onReconnected: handleReconnected
    });

    useEffect(() => {
        if (status === "authenticated") {
            fetchSummary();
        }
    }, [status, fetchSummary]);

    const value: NotificationContextValue = {
        isConnected,
        isConnecting,
        unreadCount,
        notifications,
        isLoading,
        error,
        fetchNotifications,
        markAsRead,
        markAllAsRead,
        deleteNotification
    };

    return (
        <NotificationContext.Provider value={value}>
            {children}
        </NotificationContext.Provider>
    );
}
