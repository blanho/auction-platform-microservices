/**
 * Common pagination defaults matching backend BuildingBlocks.Application.Constants.PaginationDefaults
 */
export const PaginationDefaults = {
  DefaultPage: 1,
  DefaultPageSize: 10,
  MaxPageSize: 100,
} as const

/**
 * Base query parameters for pagination - matches backend QueryParameters
 */
export interface QueryParameters {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

/**
 * Generic query parameters with typed filter - matches backend QueryParameters<TFilter>
 */
export interface QueryParametersWithFilter<TFilter> extends QueryParameters {
  filter?: TFilter
}

export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface ApiError {
  message: string
  code?: string
  errors?: Record<string, string[]>
}
