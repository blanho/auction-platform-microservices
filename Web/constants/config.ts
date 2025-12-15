export const PAGINATION = {
  DEFAULT_PAGE: 1,
  DEFAULT_PAGE_SIZE: 12,
  PAGE_SIZE_OPTIONS: [10, 12, 20, 50, 100],
  MAX_PAGE_SIZE: 100,
  TRANSACTIONS_PAGE_SIZE: 20,
} as const;

export const TIME = {
  SECOND: 1000,
  MINUTE: 60 * 1000,
  HOUR: 60 * 60 * 1000,
  DAY: 24 * 60 * 60 * 1000,
  WEEK: 7 * 24 * 60 * 60 * 1000,
  COUNTDOWN_INTERVAL: 1000,
  CAROUSEL_INTERVAL: 5000,
  REFRESH_INTERVAL: 60000,
  DEBOUNCE_DELAY: 300,
  TOAST_DURATION: 5000,
} as const;

export const AUCTION_BID = {
  MIN_INCREMENT: 100,
  DEFAULT_RESERVE_PRICE: 0,
  AUTO_EXTEND_MINUTES: 5,
} as const;

export const URGENCY = {
  CRITICAL_HOURS: 1,
  WARNING_HOURS: 24,
  CRITICAL_MINUTES: 60,
} as const;

export const CACHE = {
  STALE_TIME: 5 * TIME.MINUTE,
  CACHE_TIME: 10 * TIME.MINUTE,
  REFETCH_INTERVAL: TIME.MINUTE,
} as const;

export const SIGNALR = {
  RECONNECT_TIMEOUT: TIME.MINUTE,
  MAX_RETRY_DELAY: 10 * TIME.SECOND,
} as const;

export const FILE_UPLOAD = {
  MAX_FILE_SIZE: 10 * 1024 * 1024,
  MAX_FILES: 10,
  ACCEPTED_IMAGE_TYPES: ['image/jpeg', 'image/png', 'image/webp', 'image/gif'],
  ACCEPTED_DOCUMENT_TYPES: ['application/pdf', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'],
} as const;

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

export const STORAGE_KEYS = {
  WATCHLIST: 'watchlist',
  THEME: 'theme',
  SIDEBAR_STATE: 'sidebar-state',
  RECENT_SEARCHES: 'recent-searches',
} as const;

export const SORT_OPTIONS = {
  AUCTION: {
    NEW: 'new',
    ENDING_SOON: 'endingSoon',
    PRICE_LOW: 'priceLow',
    PRICE_HIGH: 'priceHigh',
    BIDS: 'bids',
  },
} as const;

export const UI = {
  CAROUSEL: {
    MAX_ITEMS: 5,
    AUTO_PLAY_INTERVAL: TIME.CAROUSEL_INTERVAL,
    INTERVAL: TIME.CAROUSEL_INTERVAL,
  },
  SKELETON: {
    CARD_COUNT: 6,
    LIST_COUNT: 4,
    DASHBOARD_CARDS: 6,
  },
  SCROLL_AMOUNT: 340,
  ANIMATION: {
    FADE_DURATION: 0.3,
    SLIDE_DURATION: 0.5,
    STAGGER_DELAY: 0.1,
    DURATION_FAST: 0.3,
    DURATION_DEFAULT: 0.5,
  },
} as const;

export const FEATURED = {
  DEFAULT_LIMIT: 6,
  MAX_LIMIT: 8,
  HOME_DISPLAY_COUNT: 4,
} as const;

export const ACTIVITY = {
  DEFAULT_LIMIT: 5,
  MAX_RECENT: 10,
} as const;
