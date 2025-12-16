import { AUCTION_STATUS_COLORS } from '@/constants/status';

/**
 * Get status color class for auction status
 */
export function getStatusColor(status: string): string {
  const normalizedStatus = status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
  return AUCTION_STATUS_COLORS[normalizedStatus] || 'bg-gray-500';
}

/**
 * Get status color by checking various formats
 */
export function getStatusColorSafe(status: string | null | undefined): string {
  if (!status) return 'bg-gray-500';
  
  const statusUpper = status.toUpperCase();
  
  switch (statusUpper) {
    case 'LIVE':
      return 'bg-green-500';
    case 'FINISHED':
      return 'bg-gray-500';
    case 'RESERVENOTMET':
    case 'RESERVE_NOT_MET':
      return 'bg-yellow-500';
    case 'CANCELLED':
      return 'bg-red-500';
    default:
      return 'bg-gray-500';
  }
}

/**
 * Check if auction is currently active
 */
export function isAuctionActive(status: string): boolean {
  return status.toUpperCase() === 'LIVE';
}

/**
 * Check if auction has ended
 */
export function isAuctionEnded(status: string): boolean {
  const statusUpper = status.toUpperCase();
  return statusUpper === 'FINISHED' || statusUpper === 'CANCELLED';
}

/**
 * Check if auction can accept bids
 */
export function canPlaceBid(
  status: string,
  auctionEnd: string | Date | null,
  currentUser?: string,
  seller?: string
): boolean {
  if (!isAuctionActive(status)) return false;
  
  if (currentUser && seller && currentUser === seller) return false;
  
  if (auctionEnd) {
    const endDate = new Date(auctionEnd);
    if (endDate.getTime() <= Date.now()) return false;
  }
  
  return true;
}

const BID_INCREMENT_RULES = [
  { minBid: 0, maxBid: 99, increment: 5 },
  { minBid: 100, maxBid: 499, increment: 10 },
  { minBid: 500, maxBid: 999, increment: 25 },
  { minBid: 1000, maxBid: 4999, increment: 50 },
  { minBid: 5000, maxBid: 9999, increment: 100 },
  { minBid: 10000, maxBid: 24999, increment: 250 },
  { minBid: 25000, maxBid: 49999, increment: 500 },
  { minBid: 50000, maxBid: 99999, increment: 1000 },
  { minBid: 100000, maxBid: 249999, increment: 2500 },
  { minBid: 250000, maxBid: 499999, increment: 5000 },
  { minBid: 500000, maxBid: Number.MAX_SAFE_INTEGER, increment: 10000 },
];

export function getMinimumBidIncrement(currentBid: number): number {
  const rule = BID_INCREMENT_RULES.find(
    (r) => currentBid >= r.minBid && currentBid <= r.maxBid
  );
  return rule?.increment ?? 10;
}

export function calculateMinimumBid(
  currentHighBid: number | null | undefined,
  reservePrice: number = 0
): number {
  const current = currentHighBid ?? reservePrice;
  if (current === 0) return reservePrice > 0 ? reservePrice : 1;
  
  const increment = getMinimumBidIncrement(current);
  return current + increment;
}

export function isValidBidAmount(
  bidAmount: number,
  currentHighBid: number | null | undefined,
  reservePrice: number = 0
): boolean {
  const minimumNextBid = calculateMinimumBid(currentHighBid, reservePrice);
  return bidAmount >= minimumNextBid;
}

export function getBidIncrementError(
  bidAmount: number,
  currentHighBid: number | null | undefined,
  reservePrice: number = 0
): string | null {
  if (isValidBidAmount(bidAmount, currentHighBid, reservePrice)) {
    return null;
  }
  
  const current = currentHighBid ?? reservePrice;
  const increment = getMinimumBidIncrement(current);
  const minimumNextBid = calculateMinimumBid(currentHighBid, reservePrice);
  
  return `Bid must be at least $${minimumNextBid.toLocaleString()}. Minimum increment is $${increment.toLocaleString()} for bids at this level.`;
}

/**
 * Get default image URL for auction
 */
export function getAuctionImageUrl(
  files: Array<{ url?: string; fileType?: string; isPrimary?: boolean }> | null | undefined,
  fallback: string = '/placeholder-car.jpg'
): string {
  if (!files || files.length === 0) return fallback;
  
  const primary = files.find(f => f.isPrimary && f.fileType === 'Image' && f.url);
  if (primary?.url) return primary.url;
  
  const anyImage = files.find(f => f.fileType === 'Image' && f.url);
  if (anyImage?.url) return anyImage.url;
  
  return fallback;
}

export interface AuctionLike {
  id: string;
  title?: string | null;
  description?: string | null;
  condition?: string | null;
  yearManufactured?: number | null;
  attributes?: Record<string, string> | null;
  categoryId?: string | null;
  categoryName?: string | null;
  categorySlug?: string | null;
  seller?: string | null;
  winner?: string | null;
  currency?: string | null;
}

export function getAuctionTitle(auction: AuctionLike): string {
  return auction.title || 'Untitled';
}

export function getAuctionDescription(auction: AuctionLike): string {
  return auction.description || '';
}

export function getAuctionCondition(auction: AuctionLike): string | undefined {
  return auction.condition || undefined;
}

export function getAuctionYearManufactured(auction: AuctionLike): number | undefined {
  return auction.yearManufactured || undefined;
}

export function getAuctionCategoryName(auction: AuctionLike): string | undefined {
  return auction.categoryName || undefined;
}

export function getAuctionCategorySlug(auction: AuctionLike): string | undefined {
  return auction.categorySlug || undefined;
}

export function getAuctionAttributes(auction: AuctionLike): Record<string, string> {
  return auction.attributes || {};
}

export function getAuctionSellerUsername(auction: AuctionLike): string {
  return auction.seller || '';
}

export function getAuctionWinnerUsername(auction: AuctionLike): string | undefined {
  return auction.winner || undefined;
}

export function getAuctionCurrency(auction: AuctionLike): string {
  return auction.currency || 'USD';
}
