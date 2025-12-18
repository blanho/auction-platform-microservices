import apiClient from "@/lib/api/axios";

export interface WishlistItem {
    id: string;
    auctionId: string;
    addedAt: string;
    auctionTitle: string;
    auctionImageUrl?: string;
    currentBid: number;
    auctionEnd: string;
    status: string;
}

export interface WishlistToggleResponse {
    isInWishlist: boolean;
    message: string;
}

const BASE_URL = "/bookmarks";

export const wishlistService = {
    async getWishlist(): Promise<WishlistItem[]> {
        const response = await apiClient.get<WishlistItem[]>(`${BASE_URL}/wishlist`);
        return response.data;
    },

    async getWishlistCount(): Promise<number> {
        const response = await apiClient.get<number>(`${BASE_URL}/wishlist/count`);
        return response.data;
    },

    async checkWishlist(auctionId: string): Promise<boolean> {
        const response = await apiClient.get<boolean>(`${BASE_URL}/wishlist/check/${auctionId}`);
        return response.data;
    },

    async addToWishlist(auctionId: string): Promise<WishlistItem> {
        const response = await apiClient.post<WishlistItem>(`${BASE_URL}/wishlist/${auctionId}`);
        return response.data;
    },

    async removeFromWishlist(auctionId: string): Promise<void> {
        await apiClient.delete(`${BASE_URL}/wishlist/${auctionId}`);
    },

    async toggleWishlist(auctionId: string): Promise<WishlistToggleResponse> {
        const response = await apiClient.post<WishlistToggleResponse>(`${BASE_URL}/wishlist/${auctionId}/toggle`);
        return response.data;
    },
};
