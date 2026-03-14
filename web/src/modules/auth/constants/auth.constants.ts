export const AUTH_ROUTES = {
  LOGIN: '/login',
  REGISTER: '/register',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',
  CONFIRM_EMAIL: '/confirm-email',
} as const

export const PROTECTED_ROUTES = [
  '/dashboard',
  '/profile',
  '/settings',
  '/auctions/create',
  '/bids',
  '/orders',
  '/wallet',
] as const

export const ADMIN_ROUTES = ['/admin'] as const

export const SELLER_ROUTES = ['/seller'] as const

export const SESSION_TIMEOUT_MS = 30 * 60 * 1000
export const TOKEN_REFRESH_THRESHOLD_MS = 60 * 1000
export const REFRESH_INTERVAL_MS = 4 * 60 * 1000
export const SESSION_CHECK_INTERVAL_MS = 30 * 1000
export const MAX_LOGIN_ATTEMPTS = 5
export const LOCKOUT_DURATION_MS = 15 * 60 * 1000
