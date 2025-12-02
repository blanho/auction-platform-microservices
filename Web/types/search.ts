export interface SearchItem {
  id: string;
  title: string | null;
  description: string | null;
  category: string | null;
  tags: string | null;
  imageUrl: string | null;
  price: number;
  status: string;
  source: string;
  sourceId: string;
  createdAt: string;
  updatedAt: string;
  viewCount: number;
  relevance: number;
  lastIndexed: string;
}

export interface SearchRequestDto {
  query?: string;
  page?: number;
  pageSize?: number;
  seller?: string;
  winner?: string;
  status?: string;
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortOrder?: string;
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
  items: SearchItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  query: string;
}
