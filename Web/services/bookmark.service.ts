import apiClient from "@/lib/api/axios";
import { BookmarkType } from "@/types/auction";

export interface BookmarkItem {
  id: string;
  auctionId: string;
  type: BookmarkType;
  notifyOnBid: boolean;
  notifyOnEnd: boolean;
  addedAt: string;
  auction?: {
    id: string;
    title: string;
    imageUrl?: string;
    currentHighBid?: number;
    reservePrice: number;
    currency: string;
    auctionEnd: string;
    status: string;
  };
}

export interface BookmarkToggleResponse {
  isBookmarked: boolean;
  bookmarkId?: string;
  message: string;
}

export interface AddBookmarkRequest {
  auctionId: string;
  type: BookmarkType;
  notifyOnBid?: boolean;
  notifyOnEnd?: boolean;
}

export interface UpdateBookmarkRequest {
  notifyOnBid?: boolean;
  notifyOnEnd?: boolean;
}

export const bookmarkService = {
  async getBookmarks(type?: BookmarkType): Promise<BookmarkItem[]> {
    const params = type !== undefined ? { type } : {};
    const response = await apiClient.get<BookmarkItem[]>("/auctions/bookmarks", { params });
    return response.data;
  },

  async getWatchlist(): Promise<BookmarkItem[]> {
    return this.getBookmarks(BookmarkType.Watchlist);
  },

  async getWishlist(): Promise<BookmarkItem[]> {
    return this.getBookmarks(BookmarkType.Wishlist);
  },

  async getBookmarkIds(type?: BookmarkType): Promise<string[]> {
    const params = type !== undefined ? { type } : {};
    const response = await apiClient.get<string[]>("/auctions/bookmarks/ids", { params });
    return response.data;
  },

  async getBookmarkCount(type?: BookmarkType): Promise<number> {
    const params = type !== undefined ? { type } : {};
    const response = await apiClient.get<number>("/auctions/bookmarks/count", { params });
    return response.data;
  },

  async checkBookmark(auctionId: string, type: BookmarkType): Promise<boolean> {
    const response = await apiClient.get<boolean>(
      `/auctions/bookmarks/${auctionId}/check`,
      { params: { type } }
    );
    return response.data;
  },

  async addBookmark(request: AddBookmarkRequest): Promise<BookmarkItem> {
    const response = await apiClient.post<BookmarkItem>("/auctions/bookmarks", request);
    return response.data;
  },

  async addToWatchlist(auctionId: string): Promise<BookmarkItem> {
    return this.addBookmark({ auctionId, type: BookmarkType.Watchlist });
  },

  async addToWishlist(auctionId: string): Promise<BookmarkItem> {
    return this.addBookmark({ auctionId, type: BookmarkType.Wishlist });
  },

  async removeBookmark(auctionId: string, type: BookmarkType): Promise<void> {
    await apiClient.delete(`/auctions/bookmarks/${auctionId}`, { params: { type } });
  },

  async removeFromWatchlist(auctionId: string): Promise<void> {
    return this.removeBookmark(auctionId, BookmarkType.Watchlist);
  },

  async removeFromWishlist(auctionId: string): Promise<void> {
    return this.removeBookmark(auctionId, BookmarkType.Wishlist);
  },

  async toggleBookmark(auctionId: string, type: BookmarkType): Promise<BookmarkToggleResponse> {
    const response = await apiClient.post<BookmarkToggleResponse>(
      `/auctions/bookmarks/${auctionId}/toggle`,
      null,
      { params: { type } }
    );
    return response.data;
  },

  async toggleWatchlist(auctionId: string): Promise<BookmarkToggleResponse> {
    return this.toggleBookmark(auctionId, BookmarkType.Watchlist);
  },

  async toggleWishlist(auctionId: string): Promise<BookmarkToggleResponse> {
    return this.toggleBookmark(auctionId, BookmarkType.Wishlist);
  },

  async updateBookmark(
    auctionId: string,
    type: BookmarkType,
    request: UpdateBookmarkRequest
  ): Promise<void> {
    await apiClient.put(
      `/auctions/bookmarks/${auctionId}`,
      request,
      { params: { type } }
    );
  },
};
