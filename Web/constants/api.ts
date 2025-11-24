export const API_ENDPOINTS = {
  AUCTIONS: "/api/auctions",
  AUCTION_BY_ID: (id: string) => `/api/auctions/${id}`,
  SEARCH: "/api/search",
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
  USER: "user"
} as const;

export const DEFAULT_PAGE_SIZE = 12;
export const DEFAULT_STALE_TIME = 5 * 60 * 1000;
