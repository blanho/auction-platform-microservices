export interface AuctionFile {
  storageFileId: string;
  fileName: string;
  contentType: string;
  size: number;
  url?: string;
  fileType: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface Auction {
  id: string;
  title: string;
  description: string;
  reservePrice: number;
  buyNowPrice?: number;
  isBuyNowAvailable: boolean;
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
  categoryId?: string;
  categoryName?: string;
  categorySlug?: string;
  categoryIcon?: string;
  isFeatured: boolean;
  files: AuctionFile[];
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  icon: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  parentCategoryId?: string;
  auctionCount: number;
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
  buyNowPrice?: number;
  auctionEnd: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  categoryId?: string;
  isFeatured?: boolean;
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

export interface BuyNowResult {
  isSuccess: boolean;
  auctionId: string;
  buyNowPrice: number;
  buyer: string;
  message: string;
}
