export interface Auction {
  id: string;
  title: string;
  description: string;
  reservePrice: number;
  soldAmount?: number;
  currentHighBid?: number;
  createdAt: string;
  updatedAt: string;
  auctionEnd: string;
  status: AuctionStatus;
  seller: string;
  winner?: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  imageUrl?: string;
}

export enum AuctionStatus {
  Live = "Live",
  Finished = "Finished",
  ReserveNotMet = "ReserveNotMet",
  Cancelled = "Cancelled"
}

export interface CreateAuctionDto {
  title: string;
  description: string;
  reservePrice: number;
  auctionEnd: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  imageUrl?: string;
}

export interface UpdateAuctionDto {
  title?: string;
  description?: string;
  make?: string;
  model?: string;
  year?: number;
  color?: string;
  mileage?: number;
}
