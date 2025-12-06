export interface Bid {
  id: string;
  auctionId: string;
  bidder: string;
  amount: number;
  bidTime: string;
  status: BidStatus;
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
