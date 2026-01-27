import { http } from '@/services/http'
import type { Auction, AuctionDetails, AuctionFilters, AuctionListItem, CreateAuctionRequest, UpdateAuctionRequest } from '../types'
import type { PaginatedResponse } from '@/shared/types'

export const auctionsApi = {
  async getAuctions(filters: AuctionFilters): Promise<PaginatedResponse<AuctionListItem>> {
    const response = await http.get<PaginatedResponse<AuctionListItem>>('/auctions', { params: filters })
    return response.data
  },

  async getFeaturedAuctions(pageSize: number = 8): Promise<PaginatedResponse<AuctionListItem>> {
    const response = await http.get<PaginatedResponse<AuctionListItem>>('/auctions/featured', { params: { pageSize } })
    return response.data
  },

  async getAuctionById(id: string): Promise<AuctionDetails> {
    const response = await http.get<AuctionDetails>(`/auctions/${id}`)
    return response.data
  },

  async getAuctionsByIds(ids: string[]): Promise<AuctionListItem[]> {
    const response = await http.post<AuctionListItem[]>('/auctions/batch', ids)
    return response.data
  },

  async createAuction(data: CreateAuctionRequest): Promise<Auction> {
    const response = await http.post<Auction>('/auctions', data)
    return response.data
  },

  async updateAuction(id: string, data: UpdateAuctionRequest): Promise<Auction> {
    const response = await http.put<Auction>(`/auctions/${id}`, data)
    return response.data
  },

  async deleteAuction(id: string): Promise<void> {
    await http.delete(`/auctions/${id}`)
  },

  async activateAuction(id: string): Promise<void> {
    await http.post(`/auctions/${id}/activate`)
  },

  async deactivateAuction(id: string): Promise<void> {
    await http.post(`/auctions/${id}/deactivate`)
  },

  async getMyAuctions(filters: AuctionFilters): Promise<PaginatedResponse<AuctionListItem>> {
    const response = await http.get<PaginatedResponse<AuctionListItem>>('/auctions/my', { params: filters })
    return response.data
  },
}
