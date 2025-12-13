import api from "@/lib/api";

export interface AutoBid {
    id: string;
    auctionId: string;
    bidder: string;
    maxAmount: number;
    currentBidAmount: number;
    bidIncrement: number;
    isActive: boolean;
    createdAt: string;
    lastBidAt: string | null;
}

export interface CreateAutoBidDto {
    auctionId: string;
    maxAmount: number;
}

export interface UpdateAutoBidDto {
    maxAmount?: number;
    isActive?: boolean;
}

const BASE_URL = "/bidsvc/api/autobids";

export const autoBidService = {
    createAutoBid: async (dto: CreateAutoBidDto): Promise<AutoBid> => {
        const response = await api.post<AutoBid>(BASE_URL, dto);
        return response.data;
    },

    getAutoBid: async (id: string): Promise<AutoBid> => {
        const response = await api.get<AutoBid>(`${BASE_URL}/${id}`);
        return response.data;
    },

    getMyAutoBids: async (): Promise<AutoBid[]> => {
        const response = await api.get<AutoBid[]>(`${BASE_URL}/my`);
        return response.data;
    },

    getMyAutoBidForAuction: async (auctionId: string): Promise<AutoBid | null> => {
        try {
            const response = await api.get<AutoBid>(`${BASE_URL}/auction/${auctionId}`);
            return response.data;
        } catch {
            return null;
        }
    },

    updateAutoBid: async (id: string, dto: UpdateAutoBidDto): Promise<AutoBid> => {
        const response = await api.put<AutoBid>(`${BASE_URL}/${id}`, dto);
        return response.data;
    },

    cancelAutoBid: async (id: string): Promise<void> => {
        await api.delete(`${BASE_URL}/${id}`);
    },
};
