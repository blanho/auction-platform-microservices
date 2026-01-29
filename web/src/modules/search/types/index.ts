export interface SearchResult {
  id: string
  type: SearchResultType
  title: string
  description: string
  imageUrl?: string
  price?: number
  relevanceScore: number
  highlights: SearchHighlight[]
}

export type SearchResultType = 'auction' | 'category' | 'seller'

export interface SearchHighlight {
  field: string
  fragments: string[]
}

export interface SearchFilters {
  query: string
  types?: SearchResultType[]
  categoryId?: string
  minPrice?: number
  maxPrice?: number
  status?: string
  sortBy?: 'relevance' | 'price-low' | 'price-high' | 'newest' | 'ending-soon'
  page?: number
  pageSize?: number
}

export interface SearchSuggestion {
  text: string
  type: 'query' | 'category' | 'auction'
  metadata?: {
    id?: string
    count?: number
  }
}

export interface SearchFacets {
  categories: FacetItem[]
  priceRanges: FacetItem[]
  statuses: FacetItem[]
}

export interface FacetItem {
  value: string
  label: string
  count: number
}

export interface SearchResponse {
  results: SearchResult[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  facets: SearchFacets
  suggestions: SearchSuggestion[]
}
