import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { searchApi } from '../api'
import type { SearchFilters } from '../types'

export const searchKeys = {
  all: ['search'] as const,
  results: (filters: SearchFilters) => [...searchKeys.all, 'results', filters] as const,
  suggestions: (query: string) => [...searchKeys.all, 'suggestions', query] as const,
  popular: () => [...searchKeys.all, 'popular'] as const,
  recent: () => [...searchKeys.all, 'recent'] as const,
}

export const useSearch = (filters: SearchFilters, enabled = true) => {
  return useQuery({
    queryKey: searchKeys.results(filters),
    queryFn: () => searchApi.search(filters),
    enabled: enabled && !!filters.query,
  })
}

export const useSearchSuggestions = (query: string, enabled = true) => {
  return useQuery({
    queryKey: searchKeys.suggestions(query),
    queryFn: () => searchApi.getSuggestions(query),
    enabled: enabled && query.length >= 2,
  })
}

export const usePopularSearches = () => {
  return useQuery({
    queryKey: searchKeys.popular(),
    queryFn: () => searchApi.getPopularSearches(),
    staleTime: 1000 * 60 * 5,
  })
}

export const useRecentSearches = () => {
  return useQuery({
    queryKey: searchKeys.recent(),
    queryFn: () => searchApi.getRecentSearches(),
  })
}

export const useClearRecentSearches = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => searchApi.clearRecentSearches(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: searchKeys.recent() })
    },
  })
}
