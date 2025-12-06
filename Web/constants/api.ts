export const API_ENDPOINTS = {
  AUCTIONS: "/auctions",
  AUCTION_BY_ID: (id: string) => `/auctions/${id}`,
  AUCTION_ACTIVATE: (id: string) => `/auctions/${id}/activate`,
  AUCTION_DEACTIVATE: (id: string) => `/auctions/${id}/deactivate`,
  AUCTIONS_IMPORT: "/auctions/import",
  AUCTIONS_IMPORT_EXCEL: "/auctions/import/excel",
  AUCTIONS_IMPORT_TEMPLATE: "/auctions/import/template",
  AUCTIONS_EXPORT: "/auctions/export",

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

  AUDIT_LOGS: "/auditlogs",
  AUDIT_LOG_BY_ID: (id: string) => `/auditlogs/${id}`,
  AUDIT_LOGS_BY_ENTITY: (entityType: string, entityId: string) =>
    `/auditlogs/entity/${entityType}/${entityId}`,

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
  NOTIFICATIONS_SUMMARY: "notifications-summary",
  AUDIT_LOGS: "audit-logs",
  AUDIT_LOG: "audit-log",
  AUDIT_LOGS_BY_ENTITY: "audit-logs-by-entity"
} as const;

export const DEFAULT_PAGE_SIZE = 12;
export const DEFAULT_STALE_TIME = 5 * 60 * 1000;
