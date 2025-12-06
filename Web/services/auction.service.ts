import apiClient from "@/lib/api/axios";
import {
  Auction,
  CreateAuctionDto,
  UpdateAuctionDto,
  ImportAuctionDto,
  ImportAuctionsResultDto,
  ExportAuctionsRequest,
  ExportAuctionDto
} from "@/types/auction";
import { API_ENDPOINTS } from "@/constants/api";

export const auctionService = {
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

  // Activate/Deactivate
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

  // Import
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
    const { data } = await apiClient.get(API_ENDPOINTS.AUCTIONS_IMPORT_TEMPLATE, {
      responseType: "blob"
    });
    return data;
  },

  // Export
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

    // For CSV and Excel, return as blob
    const { data } = await apiClient.get(API_ENDPOINTS.AUCTIONS_EXPORT, {
      params,
      responseType: "blob"
    });
    return data;
  }
} as const;