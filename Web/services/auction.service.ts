import apiClient from "@/lib/api/axios";
import {
  Auction,
  CreateAuctionDto,
  UpdateAuctionDto,
  ImportAuctionDto,
  ImportAuctionsResultDto,
  ExportAuctionsRequest,
  ExportAuctionDto,
  Category,
  Brand,
  Tag,
  BuyNowResult
} from "@/types/auction";
import { API_ENDPOINTS } from "@/constants/api";
import { PaginatedResponse } from "@/types";

export interface GetAuctionsParams {
  status?: string;
  sellerUsername?: string;
  winnerUsername?: string;
  searchTerm?: string;
  category?: string;
  brandId?: string;
  condition?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
  descending?: boolean;
}

export interface GetMyAuctionsParams {
  status?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
  descending?: boolean;
}

export interface AuctionPagedResult {
  items: Auction[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export const auctionService = {
  getCategories: async (): Promise<Category[]> => {
    const { data } = await apiClient.get<Category[]>(
      API_ENDPOINTS.CATEGORIES
    );
    return data;
  },

  getFeaturedAuctions: async (limit: number = 8): Promise<Auction[]> => {
    const { data } = await apiClient.get<AuctionPagedResult>(
      API_ENDPOINTS.AUCTIONS_FEATURED,
      { params: { pageSize: limit } }
    );
    return data.items || [];
  },

  getAuctions: async (
    params?: GetAuctionsParams
  ): Promise<AuctionPagedResult> => {
    const { data } = await apiClient.get<AuctionPagedResult>(
      API_ENDPOINTS.AUCTIONS,
      { params }
    );
    return data;
  },

  getAuctionById: async (id: string): Promise<Auction> => {
    const { data } = await apiClient.get<Auction>(
      API_ENDPOINTS.AUCTION_BY_ID(id)
    );
    return data;
  },

  getAuctionsByIds: async (ids: string[]): Promise<Auction[]> => {
    if (ids.length === 0) return [];
    const { data } = await apiClient.post<Auction[]>(
      API_ENDPOINTS.AUCTIONS_BATCH,
      ids
    );
    return data;
  },

  getMyAuctions: async (
    params?: GetMyAuctionsParams
  ): Promise<PaginatedResponse<Auction>> => {
    const { data } = await apiClient.get<PaginatedResponse<Auction>>(
      API_ENDPOINTS.MY_AUCTIONS,
      { params }
    );
    return data;
  },

  createAuction: async (auction: CreateAuctionDto): Promise<Auction> => {
    const { data } = await apiClient.post<Auction>(
      API_ENDPOINTS.AUCTIONS,
      auction
    );
    return data;
  },

  updateAuction: async (
    id: string,
    auction: UpdateAuctionDto
  ): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.AUCTION_BY_ID(id), auction);
  },

  deleteAuction: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.AUCTION_BY_ID(id));
  },

  adminDeleteAuction: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.AUCTION_ADMIN_DELETE(id));
  },

  activateAuction: async (id: string): Promise<Auction> => {
    const { data } = await apiClient.post<Auction>(
      API_ENDPOINTS.AUCTION_ACTIVATE(id)
    );
    return data;
  },

  deactivateAuction: async (id: string, reason?: string): Promise<Auction> => {
    const { data } = await apiClient.post<Auction>(
      API_ENDPOINTS.AUCTION_DEACTIVATE(id),
      null,
      { params: { reason } }
    );
    return data;
  },

  buyNow: async (id: string): Promise<BuyNowResult> => {
    const { data } = await apiClient.post<BuyNowResult>(
      `${API_ENDPOINTS.AUCTIONS}/${id}/buy-now`
    );
    return data;
  },

  bulkUpdateAuctions: async (
    auctionIds: string[],
    activate: boolean,
    reason?: string
  ): Promise<number> => {
    const { data } = await apiClient.post<number>(
      `${API_ENDPOINTS.AUCTIONS}/bulk-update`,
      { auctionIds, activate, reason }
    );
    return data;
  },

  importAuctions: async (
    auctions: ImportAuctionDto[]
  ): Promise<ImportAuctionsResultDto> => {
    const { data } = await apiClient.post<ImportAuctionsResultDto>(
      API_ENDPOINTS.AUCTIONS_IMPORT,
      auctions
    );
    return data;
  },

  importFromExcel: async (file: File): Promise<ImportAuctionsResultDto> => {
    const formData = new FormData();
    formData.append("file", file);
    const { data } = await apiClient.post<ImportAuctionsResultDto>(
      API_ENDPOINTS.AUCTIONS_IMPORT_EXCEL,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data"
        }
      }
    );
    return data;
  },

  downloadImportTemplate: async (): Promise<Blob> => {
    const { data } = await apiClient.get(
      API_ENDPOINTS.AUCTIONS_IMPORT_TEMPLATE,
      {
        responseType: "blob"
      }
    );
    return data;
  },

  exportAuctions: async (
    params?: ExportAuctionsRequest
  ): Promise<ExportAuctionDto[] | Blob> => {
    const format = params?.format || "json";

    if (format === "json") {
      const { data } = await apiClient.get<ExportAuctionDto[]>(
        API_ENDPOINTS.AUCTIONS_EXPORT,
        { params }
      );
      return data;
    }

    const { data } = await apiClient.get(API_ENDPOINTS.AUCTIONS_EXPORT, {
      params,
      responseType: "blob"
    });
    return data;
  },

  getBrands: async (): Promise<Brand[]> => {
    const { data } = await apiClient.get<Brand[]>("/auctions/brands");
    return data;
  },

  getBrandBySlug: async (slug: string): Promise<Brand> => {
    const { data } = await apiClient.get<Brand>(`/auctions/brands/slug/${slug}`);
    return data;
  },

  getTags: async (): Promise<Tag[]> => {
    const { data } = await apiClient.get<Tag[]>("/auctions/tags");
    return data;
  },

  getTagBySlug: async (slug: string): Promise<Tag> => {
    const { data } = await apiClient.get<Tag>(`/auctions/tags/slug/${slug}`);
    return data;
  },
} as const;
