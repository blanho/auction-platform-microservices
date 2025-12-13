import apiClient from "@/lib/api/axios";
import { API_ENDPOINTS } from "@/constants/api";
import { Category } from "@/types/auction";

export interface CreateCategoryDto {
  name: string;
  slug: string;
  icon: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  parentCategoryId?: string | null;
}

export interface UpdateCategoryDto {
  name: string;
  slug: string;
  icon: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  parentCategoryId?: string | null;
}

export interface CategoryStats {
  totalCategories: number;
  activeCategories: number;
  inactiveCategories: number;
  categoriesWithItems: number;
}

export const adminCategoryService = {
  getAllCategories: async (activeOnly: boolean = false): Promise<Category[]> => {
    const { data } = await apiClient.get<Category[]>(API_ENDPOINTS.CATEGORIES, {
      params: { activeOnly, includeCount: true }
    });
    return data;
  },

  createCategory: async (dto: CreateCategoryDto): Promise<Category> => {
    const { data } = await apiClient.post<Category>(API_ENDPOINTS.CATEGORIES, dto);
    return data;
  },

  updateCategory: async (id: string, dto: UpdateCategoryDto): Promise<Category> => {
    const { data } = await apiClient.put<Category>(
      API_ENDPOINTS.CATEGORY_BY_ID(id),
      dto
    );
    return data;
  },

  deleteCategory: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.CATEGORY_BY_ID(id));
  },

  getStats: async (): Promise<CategoryStats> => {
    const categories = await adminCategoryService.getAllCategories(false);
    
    const activeCategories = categories.filter(c => c.isActive);
    const inactiveCategories = categories.filter(c => !c.isActive);
    const categoriesWithItems = categories.filter(c => c.auctionCount > 0);

    return {
      totalCategories: categories.length,
      activeCategories: activeCategories.length,
      inactiveCategories: inactiveCategories.length,
      categoriesWithItems: categoriesWithItems.length
    };
  }
};
