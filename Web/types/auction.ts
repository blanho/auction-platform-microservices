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

export enum ItemCondition {
  New = "New",
  LikeNew = "LikeNew",
  Excellent = "Excellent",
  Good = "Good",
  Fair = "Fair",
  Poor = "Poor",
}

export const ITEM_CONDITION_LABELS: Record<ItemCondition, string> = {
  [ItemCondition.New]: "Brand New",
  [ItemCondition.LikeNew]: "Like New",
  [ItemCondition.Excellent]: "Excellent",
  [ItemCondition.Good]: "Good",
  [ItemCondition.Fair]: "Fair",
  [ItemCondition.Poor]: "Poor",
};

export const ITEM_CONDITION_DESCRIPTIONS: Record<ItemCondition, string> = {
  [ItemCondition.New]: "Never used, in original packaging",
  [ItemCondition.LikeNew]: "Used once or twice, no visible wear",
  [ItemCondition.Excellent]: "Minimal signs of use, excellent condition",
  [ItemCondition.Good]: "Normal wear, fully functional",
  [ItemCondition.Fair]: "Visible wear, works as intended",
  [ItemCondition.Poor]: "Heavy wear, may have defects",
};

export enum ShippingType {
  Free = "Free",
  Flat = "Flat",
  Calculated = "Calculated",
  LocalPickup = "LocalPickup",
  NoShipping = "NoShipping",
}

export const SHIPPING_TYPE_LABELS: Record<ShippingType, string> = {
  [ShippingType.Free]: "Free Shipping",
  [ShippingType.Flat]: "Flat Rate",
  [ShippingType.Calculated]: "Calculated at Checkout",
  [ShippingType.LocalPickup]: "Local Pickup Only",
  [ShippingType.NoShipping]: "No Shipping",
};

export interface ShippingOptions {
  shippingType: ShippingType;
  flatRateAmount?: number;
  estimatedWeight?: number;
  packageDimensions?: PackageDimensions;
  handlingTime: number;
  shipsFrom?: string;
  shipsTo: string[];
  localPickupAddress?: string;
}

export interface PackageDimensions {
  length: number;
  width: number;
  height: number;
  unit: "in" | "cm";
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
  condition?: ItemCondition;
  categoryId?: string;
  categoryName?: string;
  categorySlug?: string;
  categoryIcon?: string;
  isFeatured: boolean;
  files: AuctionFile[];
  shippingType?: ShippingType;
  shippingCost?: number;
  handlingTime?: number;
  shipsFrom?: string;
  shipsTo?: string[];
  localPickupAvailable?: boolean;
  localPickupAddress?: string;
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
  condition?: ItemCondition;
  shippingType?: ShippingType;
  shippingCost?: number;
  handlingTime?: number;
  shipsFrom?: string;
  shipsTo?: string[];
  localPickupAvailable?: boolean;
  localPickupAddress?: string;
  files?: FileInfoDto[];
}

export interface FileInfoDto {
  url: string;
  publicId: string;
  fileName: string;
  contentType: string;
  size: number;
  displayOrder: number;
  isPrimary: boolean;
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
