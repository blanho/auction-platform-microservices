import apiClient from "@/lib/api/axios";
import { Auction, CreateAuctionDto, UpdateAuctionDto } from "@/types/auction";
import { ApiResponse } from "@/types";
import { API_ENDPOINTS } from "@/constants/api";

export const auctionService = {
  createAuction: async (auction: CreateAuctionDto): Promise<Auction> => {
    const { data } = await apiClient.post<ApiResponse<Auction>>(
      API_ENDPOINTS.AUCTIONS,
      auction
    );
    return data.data;
  },

  updateAuction: async (
    id: string,
    auction: UpdateAuctionDto
  ): Promise<Auction> => {
    const { data } = await apiClient.put<ApiResponse<Auction>>(
      API_ENDPOINTS.AUCTION_BY_ID(id),
      auction
    );
    return data.data;
  },

  deleteAuction: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.AUCTION_BY_ID(id));
  }
};
