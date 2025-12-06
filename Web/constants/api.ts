export const API_ENDPOINTS = {
  AUCTIONS: "/auctions",
  AUCTION_BY_ID: (id: string) => `/auctions/${id}`,
  SEARCH: "/search",
  SEARCH_ITEM_BY_ID: (id: string) => `/search/items/${id}`,

  BIDS: "/bids",
  BIDS_BY_AUCTION: (auctionId: string) => `/bids/auction/${auctionId}`,
  BIDS_BY_BIDDER: (bidder: string) => `/bids/bidder/${bidder}`,

  NOTIFICATIONS: "/notifications",
  NOTIFICATIONS_UNREAD: "/notifications/unread",
  NOTIFICATIONS_SUMMARY: "/notifications/summary",
  NOTIFICATION_BY_ID: (id: string) => `/notifications/${id}`,
  NOTIFICATION_MARK_READ: (id: string) => `/notifications/${id}/read`,
  NOTIFICATIONS_MARK_ALL_READ: "/notifications/read-all",
  NOTIFICATIONS_HUB: "/hubs/notifications",

  AUTH: {
    LOGIN: "/api/auth/login",
    LOGOUT: "/api/auth/logout",
    REGISTER: "/api/auth/register",
    ME: "/api/auth/me",
    REFRESH: "/api/auth/refresh"
  }
} as const;

export const QUERY_KEYS = {
  AUCTIONS: "auctions",
  AUCTION: "auction",
  SEARCH: "search",
  USER: "user",
  BIDS: "bids",
  BIDS_FOR_AUCTION: "bids-for-auction",
  BIDS_FOR_BIDDER: "bids-for-bidder",
  NOTIFICATIONS: "notifications",
  NOTIFICATIONS_SUMMARY: "notifications-summary"
} as const;

export const DEFAULT_PAGE_SIZE = 12;
export const DEFAULT_STALE_TIME = 5 * 60 * 1000;
