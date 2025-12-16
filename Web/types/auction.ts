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

export interface AuctionItem {
  id: string;
  title: string;
  description: string;
  condition?: string;
  yearManufactured?: number;
  attributes: Record<string, string>;
  categoryId?: string;
  categoryName?: string;
  categorySlug?: string;
  categoryIcon?: string;
  brandId?: string;
  brandName?: string;
  tags?: Tag[];
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
  color?: string;
  isActive: boolean;
}

export interface Brand {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  description?: string;
  isActive: boolean;
  auctionCount: number;
}

export enum BookmarkType {
  Watchlist = 0,
  Wishlist = 1,
}

export interface UserAuctionBookmark {
  id: string;
  userId: string;
  username: string;
  auctionId: string;
  type: BookmarkType;
  notifyOnBid: boolean;
  notifyOnEnd: boolean;
  addedAt: string;
  auction?: Auction;
}

export interface SavedSearch {
  id: string;
  userId: string;
  username: string;
  name: string;
  searchCriteria: string;
  notifyOnNewMatch: boolean;
  lastNotifiedAt?: string;
  createdAt: string;
}

export interface AuctionQuestion {
  id: string;
  auctionId: string;
  askerId: string;
  askerUsername: string;
  question: string;
  answer?: string;
  answeredAt?: string;
  isPublic: boolean;
  createdAt: string;
}

export interface Auction {
  id: string;
  reservePrice: number;
  buyNowPrice?: number;
  currency: string;
  isBuyNowAvailable: boolean;
  soldAmount?: number;
  currentHighBid?: number;
  createdAt: string;
  updatedAt: string;
  auctionEnd: string;
  status: AuctionStatus;
  sellerId: string;
  seller: string;
  winnerId?: string;
  winner?: string;
  isFeatured: boolean;

  title: string;
  description: string;
  condition?: string;
  yearManufactured?: number;
  attributes?: Record<string, string>;
  categoryId?: string;
  categoryName?: string;
  categorySlug?: string;
  categoryIcon?: string;

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
  reservePrice: number;
  buyNowPrice?: number;
  currency?: string;
  auctionEnd: string;
  isFeatured?: boolean;

  title: string;
  description: string;
  condition?: string;
  yearManufactured?: number;
  attributes?: Record<string, string>;
  categoryId?: string;
  brandId?: string;
  tagIds?: string[];

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
  reservePrice?: number;
  buyNowPrice?: number;
  auctionEnd?: string;
  isFeatured?: boolean;

  title?: string;
  description?: string;
  condition?: string;
  yearManufactured?: number;
  attributes?: Record<string, string>;
  categoryId?: string;
  brandId?: string;
  tagIds?: string[];

  shippingType?: ShippingType;
  shippingCost?: number;
  handlingTime?: number;
  shipsFrom?: string;
  localPickupAvailable?: boolean;
  localPickupAddress?: string;
  files?: FileInfoDto[];
}

export interface ImportAuctionDto {
  title: string;
  description: string;
  condition?: string;
  yearManufactured?: number;
  reservePrice: number;
  auctionEnd: string;
  categoryId?: string;
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
  sellerUsername?: string;
  startDate?: string;
  endDate?: string;
  format?: "json" | "csv" | "excel";
}

export interface ExportAuctionDto {
  id: string;
  title: string;
  description: string;
  condition?: string;
  yearManufactured?: number;
  reservePrice: number;
  currency: string;
  sellerId: string;
  sellerUsername: string;
  winnerId?: string;
  winnerUsername?: string;
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
  buyerId: string;
  buyerUsername: string;
  message: string;
}
