export interface SearchItem {
  id: string;
  title: string;
  description: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  imageUrl?: string;
  status: string;
  reservePrice: number;
  soldAmount?: number;
  currentHighBid?: number;
  createdAt: string;
  updatedAt: string;
  auctionEnd: string;
  seller: string;
  winner?: string;
}

export interface SearchRequestDto {
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  seller?: string;
  winner?: string;
  orderBy?: SearchOrderBy;
  filterBy?: SearchFilterBy;
}

export enum SearchOrderBy {
  Make = "make",
  New = "new",
  EndingSoon = "endingSoon"
}

export enum SearchFilterBy {
  Live = "live",
  EndingSoon = "endingSoon",
  Finished = "finished"
}

export interface SearchResultDto {
  results: SearchItem[];
  pageCount: number;
  totalCount: number;
}
