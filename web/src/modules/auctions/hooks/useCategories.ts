import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { categoriesApi } from '../api/categories.api'
import type {
  CategoryFilters,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  Category,
} from '../api/categories.api'

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

export function useCategoriesTree() {
  return useQuery({
    queryKey: categoryKeys.tree(),
    queryFn: () => categoriesApi.getCategoriesTree(),
    staleTime: 5 * 60 * 1000,
  })
}

export function useCategory(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: () => categoriesApi.getCategoryById(id),
    enabled: !!id,
  })
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

export function useBulkUpdateCategories() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (updates: { id: string; sortOrder: number }[]) =>
      categoriesApi.bulkUpdateCategories(updates),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() })
      queryClient.invalidateQueries({ queryKey: categoryKeys.tree() })
    },
  })
}

export function useImportCategories() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (file: File) => categoriesApi.importCategories(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() })
      queryClient.invalidateQueries({ queryKey: categoryKeys.tree() })
    },
  })
}
