import { http } from '@/services/http'

export interface ViewCountDto {
  auctionId: string
  viewCount: number
  uniqueViewCount: number
}

export const viewsApi = {
  async recordView(auctionId: string): Promise<{ success: boolean }> {
    const response = await http.post<{ success: boolean }>(`/auctions/${auctionId}/views`)
    return response.data
  },

  async getViewCount(auctionId: string): Promise<ViewCountDto> {
    const response = await http.get<ViewCountDto>(`/auctions/${auctionId}/views/count`)
    return response.data
  },
}
