import { http } from '@/services/http'

export interface WatchlistItem {
  id: string
  auctionId: string
  auction: {
    id: string
    title: string
    imageUrl?: string
    currentPrice: number
    endTime: string
    status: string
    bidCount: number
  }
  notifyOnBid: boolean
  notifyOnEnd: boolean
  addedAt: string
}

export interface AddToWatchlistRequest {
  auctionId: string
  notifyOnBid?: boolean
  notifyOnEnd?: boolean
}

export interface UpdateWatchlistRequest {
  notifyOnBid?: boolean
  notifyOnEnd?: boolean
}

export interface WatchlistFilters {
  status?: 'active' | 'ended' | 'all'
  page?: number
  pageSize?: number
}

export const bookmarksApi = {
  async getWatchlist(filters?: WatchlistFilters): Promise<WatchlistItem[]> {
    const response = await http.get<WatchlistItem[]>('/bookmarks/watchlist', { params: filters })
    return response.data
  },

  async getWatchlistCount(): Promise<number> {
    const response = await http.get<number>('/bookmarks/watchlist/count')
    return response.data
  },

  async isInWatchlist(auctionId: string): Promise<boolean> {
    const response = await http.get<boolean>(`/bookmarks/watchlist/check/${auctionId}`)
    return response.data
  },

  async addToWatchlist(data: AddToWatchlistRequest): Promise<WatchlistItem> {
    const response = await http.post<WatchlistItem>('/bookmarks/watchlist', data)
    return response.data
  },

  async removeFromWatchlist(auctionId: string): Promise<void> {
    await http.delete(`/bookmarks/watchlist/${auctionId}`)
  },

  async updateNotificationSettings(auctionId: string, data: UpdateWatchlistRequest): Promise<void> {
    await http.put(`/bookmarks/watchlist/${auctionId}/notifications`, data)
  },
}
