export const API_ENDPOINTS = {
  AUCTIONS: "/auctions",
  MY_AUCTIONS: "/auctions/my",
  AUCTIONS_BATCH: "/auctions/batch",
  CATEGORIES: "/categories",
  CATEGORY_BY_ID: (id: string) => `/categories/${id}`,
  AUCTIONS_FEATURED: "/auctions/featured",
  AUCTION_BY_ID: (id: string) => `/auctions/${id}`,
  AUCTION_ACTIVATE: (id: string) => `/auctions/${id}/activate`,
  AUCTION_DEACTIVATE: (id: string) => `/auctions/${id}/deactivate`,
  AUCTION_ADMIN_DELETE: (id: string) => `/auctions/admin/${id}`,
  AUCTIONS_IMPORT: "/auctions/import",
  AUCTIONS_IMPORT_EXCEL: "/auctions/import/excel",
  AUCTIONS_IMPORT_TEMPLATE: "/auctions/import/template",
  AUCTIONS_EXPORT: "/auctions/export",

  BIDS: "/bids",
  MY_BIDS: "/bids/my",
  BIDS_BY_AUCTION: (auctionId: string) => `/bids/auction/${auctionId}`,
  BIDS_BY_BIDDER: (bidder: string) => `/bids/bidder/${bidder}`,

  NOTIFICATIONS: "/notifications",
  NOTIFICATIONS_UNREAD: "/notifications/unread",
  NOTIFICATIONS_SUMMARY: "/notifications/summary",
  NOTIFICATION_BY_ID: (id: string) => `/notifications/${id}`,
  NOTIFICATION_MARK_READ: (id: string) => `/notifications/${id}/read`,
  NOTIFICATIONS_MARK_ALL_READ: "/notifications/read-all",
  NOTIFICATIONS_HUB: "/hubs/notifications",
  NOTIFICATIONS_ADMIN_ALL: "/notifications/admin/all",
  NOTIFICATIONS_ADMIN_BROADCAST: "/notifications/admin/broadcast",
  NOTIFICATIONS_ADMIN_STATS: "/notifications/admin/stats",

  AUDIT_LOGS: "/utility/api/auditlogs",
  AUDIT_LOG_BY_ID: (id: string) => `/utility/api/auditlogs/${id}`,
  AUDIT_LOGS_BY_ENTITY: (entityType: string, entityId: string) =>
    `/utility/api/auditlogs/entity/${entityType}/${entityId}`,

  AUTH: {
    LOGIN: "/api/auth/login",
    LOGOUT: "/api/auth/logout",
    REGISTER: "/api/auth/register",
    ME: "/api/auth/me",
    REFRESH: "/api/auth/refresh"
  },

  ADMIN: {
    DASHBOARD_STATS: "/utility/api/v1/admin/dashboard/stats",
    RECENT_ACTIVITY: "/utility/api/v1/admin/dashboard/activity",
    PLATFORM_HEALTH: "/utility/api/v1/admin/dashboard/health",
    
    SETTINGS: "/utility/api/v1/admin/settings",
    SETTING_BY_ID: (id: string) => `/utility/api/v1/admin/settings/${id}`,
    SETTING_BY_KEY: (key: string) => `/utility/api/v1/admin/settings/key/${key}`,
    SETTINGS_BULK: "/utility/api/v1/admin/settings/bulk",

    USERS: "/identity/api/admin/users",
    USER_BY_ID: (id: string) => `/identity/api/admin/users/${id}`,
    USER_SUSPEND: (id: string) => `/identity/api/admin/users/${id}/suspend`,
    USER_ACTIVATE: (id: string) => `/identity/api/admin/users/${id}/activate`,
    USER_STATS: "/identity/api/admin/users/stats",

    REPORTS: "/utility/api/v1/reports",
    REPORT_BY_ID: (id: string) => `/utility/api/v1/reports/${id}`,
    REPORT_STATS: "/utility/api/v1/reports/stats",

    PAYMENTS_PENDING: "/utility/api/v1/admin/payments/withdrawals/pending",
    PAYMENTS_STATS: "/utility/api/v1/admin/payments/stats",
    PAYMENT_APPROVE: (id: string) => `/utility/api/v1/admin/payments/withdrawals/${id}/approve`,
    PAYMENT_REJECT: (id: string) => `/utility/api/v1/admin/payments/withdrawals/${id}/reject`
  },

  WALLET: {
    BALANCE: "/utility/api/v1/wallet/balance",
    TRANSACTIONS: "/utility/api/v1/wallet/transactions",
    DEPOSIT: "/utility/api/v1/wallet/deposit",
    WITHDRAW: "/utility/api/v1/wallet/withdraw"
  },

  PAYMENTS: {
    CREATE_PAYMENT_INTENT: "/utility/api/payments/create-payment-intent",
    CREATE_CHECKOUT_SESSION: "/utility/api/payments/create-checkout-session",
    PAYMENT_INTENT_STATUS: (id: string) => `/utility/api/payments/payment-intent/${id}`,
    REFUND: "/utility/api/payments/refund"
  }
} as const;

export const QUERY_KEYS = {
  AUCTIONS: "auctions",
  MY_AUCTIONS: "my-auctions",
  AUCTION: "auction",
  CATEGORIES: "categories",
  ADMIN_CATEGORIES: "admin-categories",
  FEATURED_AUCTIONS: "featured-auctions",
  USER: "user",
  BIDS: "bids",
  MY_BIDS: "my-bids",
  BIDS_FOR_AUCTION: "bids-for-auction",
  BIDS_FOR_BIDDER: "bids-for-bidder",
  NOTIFICATIONS: "notifications",
  NOTIFICATIONS_SUMMARY: "notifications-summary",
  AUDIT_LOGS: "audit-logs",
  AUDIT_LOG: "audit-log",
  AUDIT_LOGS_BY_ENTITY: "audit-logs-by-entity",
  
  ADMIN_DASHBOARD_STATS: "admin-dashboard-stats",
  ADMIN_RECENT_ACTIVITY: "admin-recent-activity",
  ADMIN_PLATFORM_HEALTH: "admin-platform-health",
  ADMIN_SETTINGS: "admin-settings",
  ADMIN_USERS: "admin-users",
  ADMIN_USER_STATS: "admin-user-stats",
  ADMIN_REPORTS: "admin-reports",
  ADMIN_REPORT_STATS: "admin-report-stats",
  ADMIN_PAYMENTS: "admin-payments",
  ADMIN_PAYMENT_STATS: "admin-payment-stats",
  ADMIN_NOTIFICATIONS: "admin-notifications",
  ADMIN_NOTIFICATION_STATS: "admin-notification-stats"
} as const;

export const DEFAULT_PAGE_SIZE = 12;
export const DEFAULT_STALE_TIME = 5 * 60 * 1000;
