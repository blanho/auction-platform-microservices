import apiClient from "@/lib/api/axios";
import { BookmarkType } from "@/types/auction";

export interface BookmarkItem {
  id: string;
  auctionId: string;
  bookmarkType: string;
  auctionTitle: string;
  imageUrl?: string;
  currentBid: number;
  reservePrice: number;
  auctionEnd: string;
  status: string;
  addedAt: string;
  notifyOnBid: boolean;
  notifyOnEnd: boolean;
}

export interface BookmarkToggleResponse {
  isBookmarked: boolean;
  message: string;
}

export interface AddWatchlistRequest {
  auctionId: string;
  notifyOnBid?: boolean;
  notifyOnEnd?: boolean;
}

export interface UpdateNotificationsRequest {
  notifyOnBid: boolean;
  notifyOnEnd: boolean;
}

const BASE_URL = "/auction/api/v1/bookmarks";

export const bookmarkService = {
  async getWatchlist(): Promise<BookmarkItem[]> {
    const response = await apiClient.get<BookmarkItem[]>(`${BASE_URL}/watchlist`);
    return response.data;
  },

  async getWishlist(): Promise<BookmarkItem[]> {
    const response = await apiClient.get<BookmarkItem[]>(`${BASE_URL}/wishlist`);
    return response.data;
  },

  async getWatchlistCount(): Promise<number> {
    const response = await apiClient.get<number>(`${BASE_URL}/watchlist/count`);
    return response.data;
  },

  async getWishlistCount(): Promise<number> {
    const response = await apiClient.get<number>(`${BASE_URL}/wishlist/count`);
    return response.data;
  },

  async isInWatchlist(auctionId: string): Promise<boolean> {
    const response = await apiClient.get<boolean>(`${BASE_URL}/watchlist/check/${auctionId}`);
    return response.data;
  },

  async isInWishlist(auctionId: string): Promise<boolean> {
    const response = await apiClient.get<boolean>(`${BASE_URL}/wishlist/check/${auctionId}`);
    return response.data;
  },

  async addToWatchlist(request: AddWatchlistRequest): Promise<BookmarkItem> {
    const response = await apiClient.post<BookmarkItem>(`${BASE_URL}/watchlist`, request);
    return response.data;
  },

  async addToWishlist(auctionId: string): Promise<BookmarkItem> {
    const response = await apiClient.post<BookmarkItem>(`${BASE_URL}/wishlist/${auctionId}`);
    return response.data;
  },

  async removeFromWatchlist(auctionId: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/watchlist/${auctionId}`);
  },

  async removeFromWishlist(auctionId: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/wishlist/${auctionId}`);
  },

  async toggleWishlist(auctionId: string): Promise<BookmarkToggleResponse> {
    const response = await apiClient.post<BookmarkToggleResponse>(`${BASE_URL}/wishlist/${auctionId}/toggle`);
    return response.data;
  },

  async updateWatchlistNotifications(
    auctionId: string,
    request: UpdateNotificationsRequest
  ): Promise<void> {
    await apiClient.put(`${BASE_URL}/watchlist/${auctionId}/notifications`, request);
  },

  async checkBookmark(auctionId: string, type: BookmarkType): Promise<boolean> {
    if (type === BookmarkType.Watchlist) {
      return this.isInWatchlist(auctionId);
    }
    return this.isInWishlist(auctionId);
  },

  async toggleBookmark(auctionId: string, type: BookmarkType): Promise<BookmarkToggleResponse> {
    if (type === BookmarkType.Wishlist) {
      return this.toggleWishlist(auctionId);
    }
    const isWatched = await this.isInWatchlist(auctionId);
    if (isWatched) {
      await this.removeFromWatchlist(auctionId);
      return { isBookmarked: false, message: "Removed from watchlist" };
    }
    await this.addToWatchlist({ auctionId });
    return { isBookmarked: true, message: "Added to watchlist" };
  },
};
