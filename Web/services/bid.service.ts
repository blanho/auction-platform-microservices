import apiClient from "@/lib/api/axios";
import { Bid, PlaceBidDto } from "@/types/bid";
import { API_ENDPOINTS } from "@/constants/api";

export const bidService = {
  placeBid: async (dto: PlaceBidDto): Promise<Bid> => {
    const { data } = await apiClient.post<Bid>(API_ENDPOINTS.BIDS, dto);
    return data;
  },

  getBidsForAuction: async (auctionId: string): Promise<Bid[]> => {
    const { data } = await apiClient.get<Bid[]>(
      API_ENDPOINTS.BIDS_BY_AUCTION(auctionId)
    );
    return data;
  },

  getBidsForBidder: async (bidder: string): Promise<Bid[]> => {
    const { data } = await apiClient.get<Bid[]>(
      API_ENDPOINTS.BIDS_BY_BIDDER(bidder)
    );
    return data;
  }
} as const;
