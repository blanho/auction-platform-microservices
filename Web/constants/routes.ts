// Application route constants
export const ROUTES = {
  HOME: '/',
  
  // Auth routes
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    ERROR: '/auth/error',
    FORGOT_PASSWORD: '/auth/forgot-password',
  },
  
  // Auction routes
  AUCTIONS: {
    LIST: '/auctions',
    CREATE: '/auctions/create',
    DETAIL: (id: string) => `/auctions/${id}`,
    EDIT: (id: string) => `/auctions/${id}/edit`,
    MY_AUCTIONS: '/auctions/my-auctions',
  },
  
  // Dashboard routes
  DASHBOARD: {
    HOME: '/dashboard',
    PROFILE: '/dashboard/profile',
    BIDS: '/dashboard/bids',
    LISTINGS: '/dashboard/listings',
    WATCHLIST: '/dashboard/watchlist',
    WALLET: '/dashboard/wallet',
    SETTINGS: '/dashboard/settings',
  },
  
  // Admin routes
  ADMIN: {
    HOME: '/admin',
    USERS: '/admin/users',
    AUCTIONS: '/admin/auctions',
    REPORTS: '/admin/reports',
    PAYMENTS: '/admin/payments',
    SETTINGS: '/admin/settings',
  },
  
  // Other routes
  NOTIFICATIONS: '/notifications',
  SEARCH: '/search',
} as const;

// External URLs
export const EXTERNAL_URLS = {
  TWITTER_SHARE: (text: string, url: string) =>
    `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`,
  FACEBOOK_SHARE: (url: string) =>
    `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`,
} as const;
