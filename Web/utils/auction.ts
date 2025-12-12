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
  // Cannot bid on non-live auctions
  if (!isAuctionActive(status)) return false;
  
  // Cannot bid on your own auction
  if (currentUser && seller && currentUser === seller) return false;
  
  // Cannot bid if auction has ended
  if (auctionEnd) {
    const endDate = new Date(auctionEnd);
    if (endDate.getTime() <= Date.now()) return false;
  }
  
  return true;
}

/**
 * Calculate minimum next bid
 */
export function calculateMinimumBid(
  currentHighBid: number | null | undefined,
  reservePrice: number = 0,
  incrementPercentage: number = 0.05
): number {
  const current = currentHighBid ?? reservePrice;
  const increment = Math.max(1, Math.ceil(current * incrementPercentage));
  return current + increment;
}

/**
 * Get default image URL for auction
 */
export function getAuctionImageUrl(
  files: Array<{ url?: string; fileType?: string; isPrimary?: boolean }> | null | undefined,
  fallback: string = '/placeholder-car.jpg'
): string {
  if (!files || files.length === 0) return fallback;
  
  // Find primary image first
  const primary = files.find(f => f.isPrimary && f.fileType === 'Image' && f.url);
  if (primary?.url) return primary.url;
  
  // Find any image
  const anyImage = files.find(f => f.fileType === 'Image' && f.url);
  if (anyImage?.url) return anyImage.url;
  
  return fallback;
}
