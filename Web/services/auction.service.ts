import apiClient from "@/lib/api/axios";
import { Auction, CreateAuctionDto, UpdateAuctionDto } from "@/types/auction";
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
  }
};
