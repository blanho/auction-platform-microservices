import { http } from '@/services/http'
import type { BackendPaginatedResponse, PaginatedResponse } from '@/shared/types'
import { mapPaginatedResponse } from '@/shared/types'
import type {
  AuctionFilters,
  CreateAuctionRequest,
  UpdateAuctionRequest,
} from '../types/auction-requests.types'
import type { AuctionDetails, AuctionListItem, AuctionStatus } from '../types/auction.types'
import type { BackendAuctionDto } from '../types/backend-dto.types'
import { mapAuctionDto, mapAuctionListDtos } from '../utils/auction.mappers'

const BACKEND_STATUS: Partial<Record<AuctionStatus, string>> = {
  draft: 'Draft',
  pending: 'Scheduled',
  active: 'Live',
  'ending-soon': 'Live',
  ended: 'Finished',
  sold: 'ReservedForBuyNow',
  cancelled: 'Cancelled',
}

function toAuctionQueryParams(filters: AuctionFilters) {
  return {
    status: filters.status ? BACKEND_STATUS[filters.status] : undefined,
    seller: filters.seller,
    winner: filters.winner,
    searchTerm: filters.searchTerm,
    category: filters.category,
    isFeatured: filters.isFeatured,
    page: filters.page,
    pageSize: filters.pageSize,
    orderBy: filters.orderBy ?? filters.sortBy,
    descending: filters.descending ?? filters.sortOrder === 'desc',
  }
}

export const auctionsApi = {
  async getAuctions(filters: AuctionFilters): Promise<PaginatedResponse<AuctionListItem>> {
    const { data } = await http.get<BackendPaginatedResponse<BackendAuctionDto>>('/auctions', {
      params: toAuctionQueryParams(filters),
    })
    return mapPaginatedResponse(data, mapAuctionListDtos)
  },

  async getFeaturedAuctions(pageSize = 8): Promise<PaginatedResponse<AuctionListItem>> {
    const { data } = await http.get<BackendPaginatedResponse<BackendAuctionDto>>(
      '/auctions/featured',
      { params: { pageSize } }
    )
    return mapPaginatedResponse(data, mapAuctionListDtos)
  },

  async getAuctionById(id: string): Promise<AuctionDetails> {
    const response = await http.get<BackendAuctionDto>(`/auctions/${id}`)
    return mapAuctionDto(response.data)
  },

  async getAuctionsByIds(ids: string[]): Promise<AuctionListItem[]> {
    const response = await http.post<BackendAuctionDto[]>('/auctions/batch', ids)
    return mapAuctionListDtos(response.data)
  },

  async createAuction(data: CreateAuctionRequest): Promise<{ id: string }> {
    const response = await http.post<{ id: string }>('/auctions', data)
    return response.data
  },

  async updateAuction(id: string, data: UpdateAuctionRequest): Promise<void> {
    await http.put(`/auctions/${id}`, data)
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
    const { data } = await http.get<BackendPaginatedResponse<BackendAuctionDto>>('/auctions/my', {
      params: toAuctionQueryParams(filters),
    })
    return mapPaginatedResponse(data, mapAuctionListDtos)
  },

  async buyNow(id: string): Promise<{ orderId: string; success: boolean }> {
    const response = await http.post<{ orderId: string; success: boolean }>(
      `/auctions/${id}/buy-now`
    )
    return response.data
  },

  async cancelAuction(id: string, reason?: string): Promise<void> {
    await http.post(`/auctions/${id}/cancel`, { reason })
  },

  async extendAuction(
    id: string,
    newEndTime: string
  ): Promise<{ auctionId: string; newEndTime: string }> {
    const response = await http.post<{ auctionId: string; newEndTime: string }>(
      `/auctions/${id}/extend`,
      { newEndTime }
    )
    return response.data
  },
}
