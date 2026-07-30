export const AUCTION_SORT_CONFIG = {
  'ending-soon': { orderBy: 'enddate', descending: false },
  'newly-listed': { orderBy: 'createdat', descending: true },
  'price-low': { orderBy: 'price', descending: false },
  'price-high': { orderBy: 'price', descending: true },
} as const

export type AuctionSortOption = keyof typeof AUCTION_SORT_CONFIG
