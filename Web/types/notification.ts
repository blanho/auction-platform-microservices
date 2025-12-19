export interface Notification {
  id: string;
  userId: string;
  username?: string;
  type: NotificationType;
  title: string;
  message: string;
  htmlContent?: string;
  data: string;
  status: NotificationStatus;
  channels?: string;
  readAt?: string;
  auctionId?: string;
  bidId?: string;
  orderId?: string;
  createdAt: string;
}

export enum NotificationType {
  AuctionCreated = "AuctionCreated",
  AuctionUpdated = "AuctionUpdated",
  AuctionDeleted = "AuctionDeleted",
  AuctionFinished = "AuctionFinished",
  AuctionStarted = "AuctionStarted",
  AuctionEnding = "AuctionEnding",
  AuctionEndingSoon = "AuctionEndingSoon",
  AuctionLost = "AuctionLost",
  BidPlaced = "BidPlaced",
  BidAccepted = "BidAccepted",
  BidRejected = "BidRejected",
  Outbid = "Outbid",
  OutBid = "OutBid",
  AuctionWon = "AuctionWon",
  BuyNowExecuted = "BuyNowExecuted",
  OrderCreated = "OrderCreated",
  OrderShipped = "OrderShipped",
  OrderDelivered = "OrderDelivered",
  PaymentReceived = "PaymentReceived",
  PaymentCompleted = "PaymentCompleted",
  ReviewReceived = "ReviewReceived",
  WelcomeEmail = "WelcomeEmail",
  PasswordReset = "PasswordReset",
  AccountVerification = "AccountVerification",
  System = "System"
}

export enum NotificationStatus {
  Pending = "Pending",
  Sent = "Sent",
  Delivered = "Delivered",
  Failed = "Failed",
  Unread = "Unread",
  Read = "Read",
  Dismissed = "Dismissed"
}

export enum ChannelType {
  None = 0,
  InApp = 1,
  Email = 2,
  Sms = 4,
  Push = 8
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
