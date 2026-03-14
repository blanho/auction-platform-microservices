import { http } from '@/services/http'
import type {
  Bid,
  PlaceBidRequest,
  BidDetail,
  BidHistory,
  BidIncrementInfo,
  RetractBidResult,
  WinningBid,
  BidHistoryFilters,
  AutoBidDetail,
  CreateAutoBidRequest,
  UpdateAutoBidRequest,
  AutoBidsResult,
  CreateAutoBidResult,
  UpdateAutoBidResult,
  ToggleAutoBidResult,
  CancelAutoBidResult,
} from '../types'
import type { PaginatedResponse } from '@/shared/types'

export const biddingApi = {
  async placeBid(data: PlaceBidRequest): Promise<Bid> {
    const response = await http.post<Bid>('/bids', data)
    return response.data
  },

  async getBidById(bidId: string): Promise<BidDetail> {
    const response = await http.get<BidDetail>(`/bids/${bidId}`)
    return response.data
  },

  async getBidsForAuction(auctionId: string): Promise<Bid[]> {
    const response = await http.get<Bid[]>(`/bids/auction/${auctionId}`)
    return response.data
  },

  async getMyBids(): Promise<Bid[]> {
    const response = await http.get<Bid[]>('/bids/my')
    return response.data
  },

  async getWinningBids(
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResponse<WinningBid>> {
    const response = await http.get<PaginatedResponse<WinningBid>>('/bids/winning', {
      params: { page, pageSize },
    })
    return response.data
  },

  async getBidHistory(filters: BidHistoryFilters): Promise<PaginatedResponse<BidHistory>> {
    const response = await http.get<PaginatedResponse<BidHistory>>('/bids/history', {
      params: filters,
    })
    return response.data
  },

  async retractBid(bidId: string, reason: string): Promise<RetractBidResult> {
    const response = await http.post<RetractBidResult>(`/bids/${bidId}/retract`, { reason })
    return response.data
  },

  async getBidIncrement(currentBid: number): Promise<BidIncrementInfo> {
    const response = await http.get<BidIncrementInfo>(`/bids/increment/${currentBid}`)
    return response.data
  },

  async createAutoBid(data: CreateAutoBidRequest): Promise<CreateAutoBidResult> {
    const response = await http.post<CreateAutoBidResult>('/autobids', data)
    return response.data
  },

  async getAutoBidById(autoBidId: string): Promise<AutoBidDetail> {
    const response = await http.get<AutoBidDetail>(`/autobids/${autoBidId}`)
    return response.data
  },

  async getMyAutoBids(
    activeOnly?: boolean,
    page = 1,
    pageSize = 20
  ): Promise<AutoBidsResult> {
    const response = await http.get<AutoBidsResult>('/autobids/my', {
      params: { activeOnly, page, pageSize },
    })
    return response.data
  },

  async getAutoBidForAuction(auctionId: string): Promise<AutoBidDetail | null> {
    const response = await http.get<AutoBidDetail | null>(`/autobids/auction/${auctionId}`)
    return response.data
  },

  async updateAutoBid(autoBidId: string, data: UpdateAutoBidRequest): Promise<UpdateAutoBidResult> {
    const response = await http.put<UpdateAutoBidResult>(`/autobids/${autoBidId}`, data)
    return response.data
  },

  async toggleAutoBid(autoBidId: string, activate: boolean): Promise<ToggleAutoBidResult> {
    const response = await http.post<ToggleAutoBidResult>(`/autobids/${autoBidId}/toggle`, {
      activate,
    })
    return response.data
  },

  async cancelAutoBid(autoBidId: string): Promise<CancelAutoBidResult> {
    const response = await http.post<CancelAutoBidResult>(`/autobids/${autoBidId}/cancel`)
    return response.data
  },
}
