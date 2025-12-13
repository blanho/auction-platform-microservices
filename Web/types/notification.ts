export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  data: string;
  status: NotificationStatus;
  readAt?: string;
  auctionId?: string;
  bidId?: string;
  createdAt: string;
}

export enum NotificationType {
  AuctionCreated = "AuctionCreated",
  AuctionUpdated = "AuctionUpdated",
  AuctionDeleted = "AuctionDeleted",
  AuctionFinished = "AuctionFinished",
  BidPlaced = "BidPlaced",
  BidAccepted = "BidAccepted",
  OutBid = "OutBid",
  AuctionWon = "AuctionWon",
  AuctionEndingSoon = "AuctionEndingSoon",
  SystemNotification = "SystemNotification",
  Broadcast = "Broadcast"
}

export enum NotificationStatus {
  Unread = "Unread",
  Read = "Read",
  Dismissed = "Dismissed"
}

export interface NotificationSummary {
  unreadCount: number;
  totalCount: number;
  recentNotifications: Notification[];
}

export interface PagedNotifications {
  items: Notification[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface BroadcastNotificationDto {
  type: NotificationType;
  title: string;
  message: string;
  targetRole?: string;
}

export interface NotificationStats {
  totalNotifications: number;
  unreadNotifications: number;
  todayCount: number;
  byType: Record<string, number>;
}
