export const BIDDING_ROUTES = {
  MY_BIDS: '/my-bids',
  WINNING_BIDS: '/winning-bids',
  BID_HISTORY: '/bid-history',
  AUTO_BIDS: '/auto-bids',
  AUCTION_DETAIL: (auctionId: string) => `/auctions/${auctionId}`,
} as const

export const BIDDING_ROUTE_LABELS = {
  MY_BIDS: 'My Bids',
  WINNING_BIDS: 'Winning Bids',
  BID_HISTORY: 'Bid History',
  AUTO_BIDS: 'Auto Bid Manager',
} as const
