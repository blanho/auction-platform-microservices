// Pagination defaults
export const PAGINATION = {
  DEFAULT_PAGE: 1,
  DEFAULT_PAGE_SIZE: 12,
  PAGE_SIZE_OPTIONS: [10, 12, 20, 50, 100],
  MAX_PAGE_SIZE: 100,
  TRANSACTIONS_PAGE_SIZE: 20,
} as const;

// Time constants (in milliseconds)
export const TIME = {
  SECOND: 1000,
  MINUTE: 60 * 1000,
  HOUR: 60 * 60 * 1000,
  DAY: 24 * 60 * 60 * 1000,
  WEEK: 7 * 24 * 60 * 60 * 1000,
  COUNTDOWN_INTERVAL: 1000,
} as const;

// Auction bid defaults
export const AUCTION_BID = {
  MIN_INCREMENT: 100,
  DEFAULT_RESERVE_PRICE: 0,
  AUTO_EXTEND_MINUTES: 5,
} as const;

// Cache/stale time for React Query
export const CACHE = {
  STALE_TIME: 5 * TIME.MINUTE,
  CACHE_TIME: 10 * TIME.MINUTE,
  REFETCH_INTERVAL: TIME.MINUTE,
} as const;

// SignalR reconnection
export const SIGNALR = {
  RECONNECT_TIMEOUT: TIME.MINUTE,
  MAX_RETRY_DELAY: 10 * TIME.SECOND,
} as const;

// File upload limits
export const FILE_UPLOAD = {
  MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
  MAX_FILES: 10,
  ACCEPTED_IMAGE_TYPES: ['image/jpeg', 'image/png', 'image/webp', 'image/gif'],
  ACCEPTED_DOCUMENT_TYPES: ['application/pdf', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'],
} as const;

// Validation limits
export const VALIDATION = {
  MIN_TITLE_LENGTH: 3,
  MAX_TITLE_LENGTH: 200,
  MIN_DESCRIPTION_LENGTH: 10,
  MAX_DESCRIPTION_LENGTH: 5000,
  MIN_PASSWORD_LENGTH: 8,
  MIN_USERNAME_LENGTH: 3,
  MAX_USERNAME_LENGTH: 50,
  MIN_YEAR: 1900,
  MAX_YEAR: new Date().getFullYear() + 1,
} as const;

// Local storage keys
export const STORAGE_KEYS = {
  WATCHLIST: 'watchlist',
  THEME: 'theme',
  SIDEBAR_STATE: 'sidebar-state',
  RECENT_SEARCHES: 'recent-searches',
} as const;

// Sort options
export const SORT_OPTIONS = {
  AUCTION: {
    NEW: 'new',
    ENDING_SOON: 'endingSoon',
    PRICE_LOW: 'priceLow',
    PRICE_HIGH: 'priceHigh',
    BIDS: 'bids',
  },
} as const;
