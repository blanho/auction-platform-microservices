import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { brandsApi } from '../api/brands.api'
import type { BrandFilters, CreateBrandRequest, UpdateBrandRequest } from '../api/brands.api'

export const brandKeys = {
  all: ['brands'] as const,
  lists: () => [...brandKeys.all, 'list'] as const,
  list: (filters?: BrandFilters) => [...brandKeys.lists(), filters] as const,
  allBrands: () => [...brandKeys.all, 'all'] as const,
  detail: (id: string) => [...brandKeys.all, 'detail', id] as const,
}

export function useBrands(filters?: BrandFilters) {
  return useQuery({
    queryKey: brandKeys.list(filters),
    queryFn: () => brandsApi.getBrands(filters),
  })
}

export function useAllBrands(filters?: { activeOnly?: boolean; isFeatured?: boolean }) {
  return useQuery({
    queryKey: brandKeys.allBrands(),
    queryFn: () => brandsApi.getAllBrands(filters),
    staleTime: 5 * 60 * 1000,
  })
}

export function useActiveBrands() {
  return useAllBrands({ activeOnly: true })
}

export function useBrand(id: string) {
  return useQuery({
    queryKey: brandKeys.detail(id),
    queryFn: () => brandsApi.getBrandById(id),
    enabled: !!id,
  })
}

export function useCreateBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateBrandRequest) => brandsApi.createBrand(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.lists() })
      queryClient.invalidateQueries({ queryKey: brandKeys.allBrands() })
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
      queryClient.invalidateQueries({ queryKey: brandKeys.allBrands() })
    },
  })
}

export function useDeleteBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => brandsApi.deleteBrand(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.lists() })
      queryClient.invalidateQueries({ queryKey: brandKeys.allBrands() })
    },
  })
}
