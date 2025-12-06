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
  Cancelled = "Cancelled",
  Inactive = "Inactive"
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

// Import/Export types
export interface ImportAuctionDto {
  title: string;
  description: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  reservePrice: number;
  auctionEnd: string;
}

export interface ImportAuctionResultDto {
  rowNumber: number;
  success: boolean;
  auctionId?: string;
  error?: string;
}

export interface ImportAuctionsResultDto {
  totalRows: number;
  successCount: number;
  failureCount: number;
  results: ImportAuctionResultDto[];
}

export interface ExportAuctionsRequest {
  status?: string;
  seller?: string;
  startDate?: string;
  endDate?: string;
  format?: "json" | "csv" | "excel";
}

export interface ExportAuctionDto {
  id: string;
  title: string;
  description: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  reservePrice: number;
  seller: string;
  winner?: string;
  soldAmount?: number;
  currentHighBid?: number;
  createdAt: string;
  auctionEnd: string;
  status: string;
}
