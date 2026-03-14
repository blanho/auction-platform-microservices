import type { Bid, AutoBid, BidHistory, WinningBid } from '../types'

export const formatBidSummary = (bid: Bid): string => {
  return `Bid #${bid.id.slice(0, 8)} - $${bid.amount.toFixed(2)} on ${new Date(
    bid.bidTime
  ).toLocaleDateString()}`
}

export const formatAutoBidSummary = (autoBid: AutoBid): string => {
  return `Auto Bid - Max: $${autoBid.maxAmount.toFixed(2)}, Current: $${autoBid.currentBidAmount.toFixed(
    2
  )}`
}

export const formatWinningBidSummary = (bid: WinningBid): string => {
  return `${bid.auctionTitle} - $${bid.currentBid.toFixed(2)} (${bid.bidCount} bids)`
}

export const formatBidHistorySummary = (bid: BidHistory): string => {
  return `${bid.auctionTitle} - $${bid.amount.toFixed(2)} - ${bid.status}`
}

export const getBidStatusLabel = (status: string, isWinning?: boolean): string => {
  if (isWinning) {return 'Winning'}
  return status
}

export const formatBidIncrement = (increment: number): string => {
  if (increment >= 1000) {
    return `$${(increment / 1000).toFixed(1)}K`
  }
  return `$${increment.toFixed(0)}`
}

export const formatBidRange = (min: number, max: number): string => {
  return `$${min.toFixed(2)} - $${max.toFixed(2)}`
}

export const formatAutoBidStatus = (autoBid: AutoBid): string => {
  const status = autoBid.isActive ? 'Active' : 'Inactive'
  const remaining = autoBid.maxAmount - autoBid.currentBidAmount
  return `${status} - ${remaining > 0 ? `$${remaining.toFixed(2)} remaining` : 'Max reached'}`
}
