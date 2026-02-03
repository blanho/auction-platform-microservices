import { http } from '@/services/http'
import type { Auction, AuctionDetails, AuctionListItem } from '../types/auction.types'
import type {
  AuctionFilters,
  CreateAuctionRequest,
  UpdateAuctionRequest,
} from '../types/auction-requests.types'
import type { BackendAuctionDto, BackendAuctionListDto } from '../types/backend-dto.types'
import type { PaginatedResponse } from '@/shared/types'
import type {
  ExportAuctionDto,
  ImportAuctionDto,
  ImportAuctionsResult,
  ExportFilters,
  BulkImportProgress,
  BulkImportStartResponse,
} from '../types/import-export.types'
import { mapAuctionDto, mapAuctionListDtos } from '../utils/auction.mappers'

interface BackendPaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export const auctionsApi = {
  async getAuctions(filters: AuctionFilters): Promise<PaginatedResponse<AuctionListItem>> {
    const response = await http.get<BackendPaginatedResponse<BackendAuctionListDto>>('/auctions', {
      params: filters,
    })
    const data = response.data
    return {
      items: mapAuctionListDtos(data.items),
      page: data.page,
      pageSize: data.pageSize,
      totalCount: data.totalCount,
      totalPages: data.totalPages,
      hasNextPage: data.hasNextPage,
      hasPreviousPage: data.hasPreviousPage,
    }
  },

  async getFeaturedAuctions(pageSize = 8): Promise<PaginatedResponse<AuctionListItem>> {
    const response = await http.get<BackendPaginatedResponse<BackendAuctionListDto>>(
      '/auctions/featured',
      { params: { pageSize } }
    )
    const data = response.data
    return {
      items: mapAuctionListDtos(data.items),
      page: data.page,
      pageSize: data.pageSize,
      totalCount: data.totalCount,
      totalPages: data.totalPages,
      hasNextPage: data.hasNextPage,
      hasPreviousPage: data.hasPreviousPage,
    }
  },

  async getAuctionById(id: string): Promise<AuctionDetails> {
    const response = await http.get<BackendAuctionDto>(`/auctions/${id}`)
    // Note: AuctionDetails extends Auction, additional fields handled separately
    return mapAuctionDto(response.data) as AuctionDetails
  },

  async getAuctionsByIds(ids: string[]): Promise<AuctionListItem[]> {
    const response = await http.post<BackendAuctionListDto[]>('/auctions/batch', ids)
    return mapAuctionListDtos(response.data)
  },

  async createAuction(data: CreateAuctionRequest): Promise<Auction> {
    const response = await http.post<BackendAuctionDto>('/auctions', data)
    return mapAuctionDto(response.data)
  },

  async updateAuction(id: string, data: UpdateAuctionRequest): Promise<Auction> {
    const response = await http.put<BackendAuctionDto>(`/auctions/${id}`, data)
    return mapAuctionDto(response.data)
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
    const response = await http.get<BackendPaginatedResponse<BackendAuctionListDto>>(
      '/auctions/my',
      { params: filters }
    )
    const data = response.data
    return {
      items: mapAuctionListDtos(data.items),
      page: data.page,
      pageSize: data.pageSize,
      totalCount: data.totalCount,
      totalPages: data.totalPages,
      hasNextPage: data.hasNextPage,
      hasPreviousPage: data.hasPreviousPage,
    }
  },

  async exportAuctions(filters: ExportFilters): Promise<ExportAuctionDto[] | Blob> {
    const { format, ...params } = filters

    if (format === 'json') {
      const response = await http.get<ExportAuctionDto[]>('/auctions/export', {
        params: { ...params, format },
      })
      return response.data
    }

    const response = await http.get('/auctions/export', {
      params: { ...params, format },
      responseType: 'blob',
    })
    return response.data as Blob
  },

  async importAuctionsFile(file: File): Promise<ImportAuctionsResult> {
    const formData = new FormData()
    formData.append('file', file)

    const response = await http.post<ImportAuctionsResult>('/auctions/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  },

  async importAuctionsJson(auctions: ImportAuctionDto[]): Promise<ImportAuctionsResult> {
    const response = await http.post<ImportAuctionsResult>('/auctions/import/json', auctions)
    return response.data
  },

  downloadExport(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  },

  async startBulkImport(file: File): Promise<BulkImportStartResponse> {
    const formData = new FormData()
    formData.append('file', file)

    const response = await http.post<BulkImportStartResponse>('/auctions/import/bulk', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  },

  async getBulkImportProgress(jobId: string): Promise<BulkImportProgress> {
    const response = await http.get<BulkImportProgress>(`/auctions/import/bulk/${jobId}`)
    return response.data
  },

  async getAllBulkImportJobs(): Promise<BulkImportProgress[]> {
    const response = await http.get<BulkImportProgress[]>('/auctions/import/bulk')
    return response.data
  },

  async cancelBulkImport(jobId: string): Promise<void> {
    await http.delete(`/auctions/import/bulk/${jobId}`)
  },
}
