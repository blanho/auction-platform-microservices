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

export const wishlistService = {
    async getWishlist(): Promise<WishlistItem[]> {
        const response = await apiClient.get<WishlistItem[]>("/auctions/wishlist");
        return response.data;
    },

    async getWishlistIds(): Promise<string[]> {
        const response = await apiClient.get<string[]>("/auctions/wishlist/ids");
        return response.data;
    },

    async getWishlistCount(): Promise<number> {
        const response = await apiClient.get<number>("/auctions/wishlist/count");
        return response.data;
    },

    async checkWishlist(auctionId: string): Promise<boolean> {
        const response = await apiClient.get<boolean>(`/auctions/wishlist/${auctionId}/check`);
        return response.data;
    },

    async addToWishlist(auctionId: string): Promise<WishlistItem> {
        const response = await apiClient.post<WishlistItem>(`/auctions/wishlist/${auctionId}`);
        return response.data;
    },

    async removeFromWishlist(auctionId: string): Promise<void> {
        await apiClient.delete(`/auctions/wishlist/${auctionId}`);
    },

    async toggleWishlist(auctionId: string): Promise<WishlistToggleResponse> {
        const response = await apiClient.post<WishlistToggleResponse>(`/auctions/wishlist/${auctionId}/toggle`);
        return response.data;
    },
};
