import { http } from '@/services/http'
import type { PaginatedResponse, QueryParameters } from '@/shared/types'

export interface Brand {
  id: string
  name: string
  slug: string
  description?: string
  logoUrl?: string
  websiteUrl?: string
  displayOrder: number
  isActive: boolean
  isFeatured: boolean
  auctionCount?: number
  createdAt: string
  updatedAt: string
}

export interface CreateBrandRequest {
  name: string
  description?: string
  logoUrl?: string
  websiteUrl?: string
  displayOrder?: number
  isFeatured?: boolean
}

export interface UpdateBrandRequest extends Partial<CreateBrandRequest> {
  isActive?: boolean
}

export interface BrandFilters extends QueryParameters {
  search?: string
  isActive?: boolean
  activeOnly?: boolean
  isFeatured?: boolean
  limit?: number
}

export const brandsApi = {
  async getBrands(filters?: BrandFilters): Promise<PaginatedResponse<Brand>> {
    const response = await http.get<PaginatedResponse<Brand>>('/brands', { params: filters })
    return response.data
  },

  async getAllBrands(filters?: { activeOnly?: boolean; isFeatured?: boolean }): Promise<Brand[]> {
    const response = await http.get<Brand[]>('/brands', { params: { ...filters, limit: 1000 } })
    return response.data
  },

  async getBrandById(id: string): Promise<Brand> {
    const response = await http.get<Brand>(`/brands/${id}`)
    return response.data
  },

  async createBrand(data: CreateBrandRequest): Promise<Brand> {
    const response = await http.post<Brand>('/brands', data)
    return response.data
  },

  async updateBrand(id: string, data: UpdateBrandRequest): Promise<Brand> {
    const response = await http.put<Brand>(`/brands/${id}`, data)
    return response.data
  },

  async deleteBrand(id: string): Promise<void> {
    await http.delete(`/brands/${id}`)
  },
}
