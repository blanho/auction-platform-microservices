/**
 * Bid increment tiers shared between UI validation and display.
 * The authoritative source of truth is the backend (BidIncrementHelper.cs).
 * These constants must stay in sync with the backend thresholds.
 */

interface BidIncrementTier {
  /** Upper bound (exclusive). Infinity for the last tier. */
  threshold: number
  /** Amount added to the current bid to reach the minimum next bid. */
  increment: number
}

export const BID_INCREMENT_TIERS: readonly BidIncrementTier[] = [
  { threshold: 100, increment: 1 },
  { threshold: 1_000, increment: 5 },
  { threshold: 5_000, increment: 25 },
  { threshold: Infinity, increment: 100 },
] as const

/**
 * Returns the minimum next bid amount for a given current bid.
 * Mirrors the backend BidIncrementHelper.GetMinimumNextBid logic.
 */
export function getMinimumNextBid(currentBid: number): number {
  const tier = BID_INCREMENT_TIERS.find((t) => currentBid < t.threshold)!
  return currentBid + tier.increment
}

/**
 * Returns three suggested bid amounts above the minimum bid,
 * spaced by progressively larger increments.
 */
export function getSuggestedBids(currentBid: number, startingPrice: number): [number, number, number] {
  const base = getMinimumNextBid(currentBid > 0 ? currentBid : startingPrice)

  const getSuggestedIncrement = (amount: number): number => {
    if (amount < 100) return 5
    if (amount < 1_000) return 25
    if (amount < 5_000) return 100
    return 500
  }

  const increment = getSuggestedIncrement(base)
  return [base, base + increment, base + increment * 2]
}
