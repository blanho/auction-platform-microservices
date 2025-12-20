export interface Bid {
  id: string;
  auctionId: string;
  bidderId: string;
  bidderUsername: string;
  amount: number;
  bidTime: string;
  status: BidStatus;
  errorMessage?: string;
  minimumNextBid: number;
  minimumIncrement: number;
  createdAt: string;
  updatedAt?: string;
}

export enum BidStatus {
  Pending = "Pending",
  Accepted = "Accepted",
  AcceptedBelowReserve = "AcceptedBelowReserve",
  TooLow = "TooLow",
  Rejected = "Rejected"
}

export interface PlaceBidDto {
  auctionId: string;
  amount: number;
}
