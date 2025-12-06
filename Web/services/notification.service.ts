import apiClient from "@/lib/api/axios";
import { Notification, NotificationSummary } from "@/types/notification";
import { API_ENDPOINTS } from "@/constants/api";

export const notificationService = {
  getNotifications: async (): Promise<Notification[]> => {
    const { data } = await apiClient.get<Notification[]>(
      API_ENDPOINTS.NOTIFICATIONS
    );
    return data;
  },

  getUnreadNotifications: async (): Promise<Notification[]> => {
    const { data } = await apiClient.get<Notification[]>(
      API_ENDPOINTS.NOTIFICATIONS_UNREAD
    );
    return data;
  },

  getSummary: async (): Promise<NotificationSummary> => {
    const { data } = await apiClient.get<NotificationSummary>(
      API_ENDPOINTS.NOTIFICATIONS_SUMMARY
    );
    return data;
  },

  markAsRead: async (id: string): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.NOTIFICATION_MARK_READ(id));
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.NOTIFICATIONS_MARK_ALL_READ);
  },

  deleteNotification: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.NOTIFICATION_BY_ID(id));
  }
} as const;
