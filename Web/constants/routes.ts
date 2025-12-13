export const ROUTES = {
  HOME: '/',
  
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    ERROR: '/auth/error',
    FORGOT_PASSWORD: '/auth/forgot-password',
  },
  
  AUCTIONS: {
    LIST: '/auctions',
    CREATE: '/auctions/create',
    DETAIL: (id: string) => `/auctions/${id}`,
    EDIT: (id: string) => `/auctions/${id}/edit`,
    MY_AUCTIONS: '/auctions/my-auctions',
  },
  
  DASHBOARD: {
    ROOT: '/dashboard',
    HOME: '/dashboard',
    PROFILE: '/dashboard/profile',
    BIDS: '/dashboard/bids',
    LISTINGS: '/dashboard/listings',
    WATCHLIST: '/dashboard/watchlist',
    ANALYTICS: '/dashboard/analytics',
    WALLET: '/dashboard/wallet',
    SETTINGS: '/dashboard/settings',
    ORDERS: '/dashboard/orders',
    CREATE_LISTING: '/auctions/create',
  },
  
  ADMIN: {
    ROOT: '/admin',
    HOME: '/admin',
    USERS: '/admin/users',
    AUCTIONS: '/admin/auctions',
    REPORTS: '/admin/reports',
    PAYMENTS: '/admin/payments',
    SETTINGS: '/admin/settings',
  },
  
  DEALS: '/deals',
  LIVE: '/live',
  GIFTS: '/gifts',
  WISHLIST: '/wishlist',
  HELP: '/help',
  
  NOTIFICATIONS: '/notifications',
  SEARCH: '/search',
} as const;

export const EXTERNAL_URLS = {
  TWITTER_SHARE: (text: string, url: string) =>
    `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`,
  FACEBOOK_SHARE: (url: string) =>
    `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`,
} as const;
