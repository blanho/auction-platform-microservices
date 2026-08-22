interface BidIncrementTier {
  threshold: number
  increment: number
}

export const BID_INCREMENT_TIERS: readonly BidIncrementTier[] = [
  { threshold: 100, increment: 1 },
  { threshold: 1_000, increment: 5 },
  { threshold: 5_000, increment: 25 },
  { threshold: Infinity, increment: 100 },
] as const

export function getMinimumNextBid(currentBid: number): number {
  const tier =
    BID_INCREMENT_TIERS.find((candidate) => currentBid < candidate.threshold) ??
    BID_INCREMENT_TIERS[BID_INCREMENT_TIERS.length - 1]
  if (!tier) {
    return currentBid
  }
  return currentBid + tier.increment
}

export function getSuggestedBids(
  currentBid: number,
  startingPrice: number
): [number, number, number] {
  const base = getMinimumNextBid(currentBid > 0 ? currentBid : startingPrice)

  const getSuggestedIncrement = (amount: number): number => {
    if (amount < 100) {
      return 5
    }
    if (amount < 1_000) {
      return 25
    }
    if (amount < 5_000) {
      return 100
    }
    return 500
  }

  const increment = getSuggestedIncrement(base)
  return [base, base + increment, base + increment * 2]
}
