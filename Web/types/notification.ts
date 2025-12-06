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
  AuctionEndingSoon = "AuctionEndingSoon"
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
