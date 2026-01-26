import { http } from '@/services/http'
import type { SearchFilters, SearchResponse, SearchSuggestion } from '../types'

export const searchApi = {
  async search(filters: SearchFilters): Promise<SearchResponse> {
    const response = await http.get<SearchResponse>('/search', { params: filters })
    return response.data
  },

  async getSuggestions(query: string, limit = 10): Promise<SearchSuggestion[]> {
    const response = await http.get<SearchSuggestion[]>('/search/suggestions', {
      params: { query, limit },
    })
    return response.data
  },

  async getPopularSearches(): Promise<string[]> {
    const response = await http.get<string[]>('/search/popular')
    return response.data
  },

  async getRecentSearches(): Promise<string[]> {
    const response = await http.get<string[]>('/search/recent')
    return response.data
  },

  async clearRecentSearches(): Promise<void> {
    await http.delete('/search/recent')
  },
}
