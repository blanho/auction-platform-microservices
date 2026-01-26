# Bidding Module

Complete bidding and auto-bidding functionality for the auction platform.

## 📁 Structure

```
bidding/
├── api/                    # API client layer
│   ├── bidding.api.ts     # All bidding endpoints
│   └── index.ts
├── components/             # Reusable bidding components
│   ├── AutoBidDialog.tsx
│   └── index.ts
├── constants/              # Module constants
│   ├── bid.constants.ts   # Bid-related constants, status colors, increments
│   ├── routes.constants.ts # Route definitions
│   └── index.ts
├── hooks/                  # React Query hooks
│   ├── useBidding.ts      # Core bid hooks
│   ├── useBids.ts         # Extended bid hooks (winning, history, retract)
│   ├── useAutoBids.ts     # Auto-bid CRUD hooks
│   └── index.ts
├── pages/                  # Page components
│   ├── MyBidsPage.tsx
│   ├── WinningBidsPage.tsx
│   ├── BidHistoryPage.tsx
│   ├── AutoBidManagementPage.tsx
│   └── index.ts
├── schemas/                # Validation schemas
│   ├── bid.schema.ts
│   └── index.ts
├── types/                  # TypeScript types
│   ├── bid.types.ts       # Bid interfaces & enums
│   ├── autobid.types.ts   # Auto-bid interfaces
│   ├── filters.types.ts   # Filter interfaces
│   └── index.ts
├── utils/                  # Utility functions
│   ├── bid.utils.ts       # Bid calculations, validations
│   ├── validation.utils.ts # Request validations
│   ├── format.utils.ts    # Formatting helpers
│   └── index.ts
├── permissions.ts          # RBAC permissions
├── types.ts                # Re-export from types/
└── index.ts                # Module exports
```

## 🎯 Features

### Bid Management
- ✅ Place bids on auctions
- ✅ View bid history with filters
- ✅ Retract bids with reason
- ✅ View winning bids
- ✅ Real-time bid increment calculations

### Auto-Bidding
- ✅ Create auto-bid strategies
- ✅ Update max amounts and increments
- ✅ Toggle auto-bids on/off
- ✅ Cancel auto-bids
- ✅ View all auto-bids with filters

### Pages
- `/my-bids` - All user bids with tabs (All, Winning, Outbid)
- `/winning-bids` - Auctions where user is currently winning
- `/bid-history` - Complete bid history with advanced filters
- `/auto-bids` - Auto-bid management dashboard

## 📦 API Endpoints

### Bids
```typescript
placeBid(data: PlaceBidRequest): Promise<Bid>
getBidById(bidId: string): Promise<BidDetail>
getBidsForAuction(auctionId: string): Promise<Bid[]>
getMyBids(): Promise<Bid[]>
getWinningBids(page, pageSize): Promise<PaginatedResponse<WinningBid>>
getBidHistory(filters: BidHistoryFilters): Promise<PaginatedResponse<BidHistory>>
retractBid(bidId: string, reason: string): Promise<RetractBidResult>
getBidIncrement(currentBid: number): Promise<BidIncrementInfo>
```

### Auto-Bids
```typescript
createAutoBid(data: CreateAutoBidRequest): Promise<CreateAutoBidResult>
getAutoBidById(autoBidId: string): Promise<AutoBidDetail>
getMyAutoBids(activeOnly?, page?, pageSize?): Promise<AutoBidsResult>
updateAutoBid(autoBidId: string, data: UpdateAutoBidRequest): Promise<UpdateAutoBidResult>
toggleAutoBid(autoBidId: string, activate: boolean): Promise<ToggleAutoBidResult>
cancelAutoBid(autoBidId: string): Promise<CancelAutoBidResult>
```

## 🪝 React Query Hooks

### Bid Hooks
```typescript
usePlaceBid()              // Mutation to place a bid
useBidById(bidId)          // Query bid details
useBidsForAuction(auctionId) // Query bids for auction
useMyBids()                // Query user's bids
useWinningBids(page, pageSize) // Query winning bids
useBidHistory(filters)     // Query bid history
useRetractBid()            // Mutation to retract bid
useBidIncrement(currentBid) // Query bid increment info
```

### Auto-Bid Hooks
```typescript
useCreateAutoBid()         // Mutation to create auto-bid
useAutoBidById(autoBidId)  // Query auto-bid details
useMyAutoBids(activeOnly, page, pageSize) // Query user's auto-bids
useUpdateAutoBid()         // Mutation to update auto-bid
useToggleAutoBid()         // Mutation to toggle auto-bid
useCancelAutoBid()         // Mutation to cancel auto-bid
```

## 🛠️ Utilities

### Bid Calculations
```typescript
calculateBidIncrement(currentBid: number): number
calculateMinimumNextBid(currentBid: number, increment?: number): number
validateBidAmount(amount: number, currentBid: number, minimumBid?: number)
```

### Bid Status & Formatting
```typescript
getBidStatusColor(status: BidStatus)
formatBidTimeRemaining(milliseconds: number): string
formatBidSummary(bid: Bid): string
formatAutoBidSummary(autoBid: AutoBid): string
```

### Validations
```typescript
validatePlaceBidRequest(request: PlaceBidRequest)
validateCreateAutoBidRequest(request: CreateAutoBidRequest)
validateUpdateAutoBidRequest(request: UpdateAutoBidRequest)
validateRetractReason(reason: string)
```

## 🎨 Design System

Following **Glassmorphism** design:
- **Colors**: Blue primary (#2563EB), Orange CTA (#F97316)
- **Typography**: Russo One (headings), Chakra Petch (body)
- **Effects**: Backdrop blur (12-16px), semi-transparent backgrounds
- **Transitions**: Smooth 300ms transforms

## 🔒 Permissions

```typescript
export const BIDDING_PERMISSIONS = {
  VIEW_MY_BIDS: 'bidding:view:my',
  PLACE_BID: 'bidding:place',
  RETRACT_BID: 'bidding:retract',
  VIEW_WINNING_BIDS: 'bidding:view:winning',
  VIEW_BID_HISTORY: 'bidding:view:history',
  MANAGE_AUTO_BIDS: 'bidding:autobid:manage',
}
```

## 📊 Constants

### Bid Status
```typescript
BidStatus = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Rejected: 'Rejected',
  Retracted: 'Retracted',
  Outbid: 'Outbid',
}
```

### Increment Ranges
```typescript
BID_INCREMENT_RANGES = [
  { max: 100, increment: 5 },
  { max: 500, increment: 10 },
  { max: 1000, increment: 25 },
  { max: 5000, increment: 50 },
  { max: 10000, increment: 100 },
  { max: Infinity, increment: 250 },
]
```

## 🔄 State Management

Uses **TanStack Query (React Query v5)** for:
- Server state caching
- Automatic refetching
- Optimistic updates
- Cache invalidation
- Loading/error states

## 📝 Usage Examples

### Place a Bid
```typescript
const { mutate: placeBid, isPending } = usePlaceBid()

placeBid({
  auctionId: 'auction-123',
  amount: 150.00
})
```

### Create Auto-Bid
```typescript
const { mutate: createAutoBid } = useCreateAutoBid()

createAutoBid({
  auctionId: 'auction-123',
  maxAmount: 500.00,
  bidIncrement: 25
})
```

### Get Bid History
```typescript
const { data: history } = useBidHistory({
  auctionId: 'auction-123',
  status: BidStatus.Accepted,
  fromDate: '2026-01-01',
  page: 1,
  pageSize: 20
})
```

## 🧪 Testing

Run tests with:
```bash
npm test -- bidding
```

## 📚 Related Modules

- **Auctions** - Auction listings and details
- **Payments** - Payment processing for winning bids
- **Notifications** - Bid status notifications
- **Analytics** - Bidding analytics and reports
