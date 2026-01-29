import { http } from '@/services/http'

export interface Category {
  id: string
  name: string
  slug: string
  icon?: string
  description?: string
  imageUrl?: string
  parentId?: string
  parentCategoryId?: string
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
  parentCategoryId?: string
  displayOrder?: number
  isActive?: boolean
}

export interface UpdateCategoryRequest extends Partial<CreateCategoryRequest> {
  isActive?: boolean
}

export interface CategoryFilters {
  search?: string
  parentId?: string
  isActive?: boolean
  activeOnly?: boolean
  includeCount?: boolean
  page?: number
  pageSize?: number
}

export const categoriesApi = {
  async getCategories(filters?: CategoryFilters): Promise<Category[]> {
    const response = await http.get<Category[]>('/categories', { params: filters })
    return response.data
  },

  async getCategoriesTree(): Promise<Category[]> {
    const response = await http.get<Category[]>('/categories/tree')
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

  async bulkUpdateCategories(updates: { id: string; sortOrder: number }[]): Promise<void> {
    await http.post('/categories/bulk-update', { updates })
  },

  async importCategories(file: File): Promise<{ imported: number; failed: number }> {
    const formData = new FormData()
    formData.append('file', file)
    const response = await http.post<{ imported: number; failed: number }>(
      '/categories/import',
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      }
    )
    return response.data
  },
}
