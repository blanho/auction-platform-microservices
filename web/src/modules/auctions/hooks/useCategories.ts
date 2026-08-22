import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type {
  Category,
  CategoryFilters,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../api/categories.api'
import { categoriesApi } from '../api/categories.api'

export type { Category, CategoryFilters, CreateCategoryRequest, UpdateCategoryRequest }

export const categoryKeys = {
  all: ['categories'] as const,
  lists: () => [...categoryKeys.all, 'list'] as const,
  list: (filters?: CategoryFilters) => [...categoryKeys.lists(), filters] as const,
  tree: () => [...categoryKeys.all, 'tree'] as const,
  detail: (id: string) => [...categoryKeys.all, 'detail', id] as const,
}

export function useCategories(filters?: CategoryFilters) {
  return useQuery({
    queryKey: categoryKeys.list(filters),
    queryFn: () => categoriesApi.getCategories(filters),
    staleTime: 5 * 60 * 1000,
  })
}

export function useActiveCategories() {
  return useCategories({ activeOnly: true })
}

export function useCreateCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateCategoryRequest) => categoriesApi.createCategory(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() })
      queryClient.invalidateQueries({ queryKey: categoryKeys.tree() })
    },
  })
}

export function useUpdateCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCategoryRequest }) =>
      categoriesApi.updateCategory(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() })
      queryClient.invalidateQueries({ queryKey: categoryKeys.tree() })
    },
  })
}

export function useDeleteCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => categoriesApi.deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() })
      queryClient.invalidateQueries({ queryKey: categoryKeys.tree() })
    },
  })
}
