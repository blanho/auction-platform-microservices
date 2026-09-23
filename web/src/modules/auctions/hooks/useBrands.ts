import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { BrandFilters, CreateBrandRequest, UpdateBrandRequest } from '../api/brands.api'
import { brandsApi } from '../api/brands.api'

export const brandKeys = {
  all: ['brands'] as const,
  lists: () => [...brandKeys.all, 'list'] as const,
  list: (filters?: BrandFilters) => [...brandKeys.lists(), filters] as const,
  detail: (id: string) => [...brandKeys.all, 'detail', id] as const,
}

export function useBrands(filters?: BrandFilters) {
  return useQuery({
    queryKey: brandKeys.list(filters),
    queryFn: () => brandsApi.getBrands(filters),
  })
}

export function useActiveBrands(search = '') {
  return useQuery({
    queryKey: brandKeys.list({ activeOnly: true, search, page: 1, pageSize: 20 }),
    queryFn: () => brandsApi.getBrands({ activeOnly: true, search, page: 1, pageSize: 20 }),
    select: (data) => data.items,
    staleTime: 5 * 60 * 1000,
  })
}

export function useCreateBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateBrandRequest) => brandsApi.createBrand(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.lists() })
    },
  })
}

export function useUpdateBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBrandRequest }) =>
      brandsApi.updateBrand(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: brandKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: brandKeys.lists() })
    },
  })
}

export function useDeleteBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => brandsApi.deleteBrand(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.lists() })
    },
  })
}
