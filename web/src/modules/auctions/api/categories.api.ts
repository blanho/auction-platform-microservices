import { http } from '@/services/http'
import type { QueryParameters } from '@/shared/types'

export interface Category {
  id: string
  name: string
  slug: string
  icon?: string
  description?: string
  imageUrl?: string
  parentCategoryId?: string | null
  displayOrder: number
  sortOrder: number
  isActive: boolean
  children?: Category[]
  auctionCount?: number
  createdAt: string
  updatedAt: string
}

export interface CreateCategoryRequest {
  name: string
  slug: string
  icon?: string
  description?: string
  imageUrl?: string
  parentCategoryId?: string | null
  displayOrder?: number
  isActive?: boolean
}

export interface UpdateCategoryRequest {
  name: string
  slug?: string
  icon: string
  description?: string
  displayOrder: number
  isActive: boolean
  parentCategoryId: string | null
}

export interface CategoryFilters extends QueryParameters {
  search?: string
  isActive?: boolean
  activeOnly?: boolean
  includeCount?: boolean
}

export const categoriesApi = {
  async getCategories(filters?: CategoryFilters): Promise<Category[]> {
    const response = await http.get<Category[]>('/categories', { params: filters })
    return response.data
  },

  async getCategoriesTree(activeOnly = true): Promise<Category[]> {
    const response = await http.get<Category[]>('/categories/tree', { params: { activeOnly } })
    return response.data
  },

  async getCategoryById(id: string): Promise<Category> {
    const response = await http.get<Category>(`/categories/${id}`)
    return response.data
  },

  async createCategory(data: CreateCategoryRequest): Promise<Category> {
    const response = await http.post<Category>('/categories', data)
    return response.data
  },

  async updateCategory(id: string, data: UpdateCategoryRequest): Promise<Category> {
    const response = await http.put<Category>(`/categories/${id}`, data)
    return response.data
  },

  async deleteCategory(id: string): Promise<void> {
    await http.delete(`/categories/${id}`)
  },
}
