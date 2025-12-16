import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { notificationService } from "@/services/notification.service";
import { Notification, NotificationSummary } from "@/types/notification";
import { QUERY_KEYS } from "@/constants/api";

export function useNotificationsQuery(enabled = true) {
    return useQuery<Notification[]>({
        queryKey: [QUERY_KEYS.NOTIFICATIONS],
        queryFn: () => notificationService.getNotifications(),
        staleTime: 30 * 1000,
        enabled,
    });
}

export function useNotificationSummaryQuery(enabled = true) {
    return useQuery<NotificationSummary>({
        queryKey: [QUERY_KEYS.NOTIFICATIONS_SUMMARY],
        queryFn: () => notificationService.getSummary(),
        staleTime: 30 * 1000,
        refetchInterval: 60 * 1000,
        enabled,
    });
}

export function useMarkNotificationReadMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => notificationService.markAsRead(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS_SUMMARY] });
        },
    });
}

export function useMarkAllNotificationsReadMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: () => notificationService.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS_SUMMARY] });
        },
    });
}

export function useDeleteNotificationMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => notificationService.deleteNotification(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.NOTIFICATIONS_SUMMARY] });
        },
    });
}
